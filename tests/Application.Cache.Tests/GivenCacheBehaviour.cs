using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FM.Application.Ports;
using FM.Tests;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Moq;
using NUnit.Framework;

namespace FM.Application.Cache.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class GivenCacheBehaviour
    {
        private readonly Assembly _thisAssembly = typeof(GivenCacheBehaviour).GetTypeInfo().Assembly;
        private readonly IComparer _publicProperty = ClassComparer.PublicProperty.Build();
        private DateTimeOffset? _utcNow;
        private string _logMessage = string.Empty;
        private string _cacheRemovedMessage = string.Empty;
        private readonly Startup _bootstrapper = new();

        [SetUp]
        public void Setup() => _bootstrapper.ConfigureServices((services, _) => services
            .AddSingleton(_ => new FM.Domain.Common.GeneralConfig { MaxLongRunningTask = 1000 })
            .AddTransient(_ => Mock.Of<ICurrentUserService>())
            .AddTransient(_ => Mock.Of<IIdentityService>())
            .AddFakeLogger<Cached.Query>(log => _logMessage = log)
            .AddFakeLogger<Cached.WithAbsoluteExpiration>(log => _logMessage = log)
            .AddFakeLogger<Cached.WithAbsoluteExpirationRelativeToNow>(log => _logMessage = log)
            .AddFakeLogger<Cached.Command>(log => _cacheRemovedMessage = log)
            .AddCache(_thisAssembly)
            .AddMediatRHandler<Cached.Query, Cached.Result>(q => new Cached.Result($"result for {q.Id}"))
            .AddMediatRHandler<Cached.WithAbsoluteExpiration, Cached.Result>(q =>
                new Cached.Result($"result for {q.Id}"))
            .AddMediatRHandler<Cached.WithAbsoluteExpirationRelativeToNow, Cached.Result>(q =>
                new Cached.Result($"result for {q.Id}"))
            .AddMediatRHandler<Cached.Command>()
            .AddDistributedMemoryCache(options =>
            {
                var clock = new Mock<ISystemClock>();
                clock.SetupGet(x => x.UtcNow).Returns(() => _utcNow ?? DateTimeOffset.UtcNow);
                options.Clock = clock.Object;
            }), _thisAssembly);

        private TService GetService<TService>() where TService : notnull => _bootstrapper.GetService<TService>();

        [Test]
        public async Task WhenRequestingToCachedQueryForTheFirstTimeThenItWillCacheTheResultForAllStoringIdentifiers()
        {
            var mediator = GetService<IMediator>();
            var query = new Cached.Query(1);

            var result = await mediator.Send(query);

            Assert.That(_logMessage, Does.Match("^Cache miss, saving (.+) to cache for (.+)$"));
            var cacheService = GetService<IDistributedCache>();
            string[] identifiers = { GetCacheKey(query), GetCacheKey<Cached.Query>(result) };
            foreach (string identifier in identifiers)
            {
                var cachedResult = await cacheService.GetAsync<Cached.Result>(identifier);
                Assert.That(cachedResult, Is.Not.Null.And.EqualTo(result).Using(_publicProperty));
            }
        }

        [Test]
        public async Task WhenRequestingToCachedQueryAfterFirstTimeThenItWillReturnCachedResult()
        {
            var mediator = GetService<IMediator>();
            var query = new Cached.Query(1);
            var savedResult = new Cached.Result("Cached result");
            var cacheService = GetService<IDistributedCache>();
            await cacheService.SetAsync(savedResult,
                new[] { GetCacheKey(query), GetCacheKey<Cached.Query>(savedResult) }, new());

            var result = await mediator.Send(query);

            Assert.That(_logMessage, Does.Match("^Cache hit, returning (.+) for (.+)$"));
            Assert.That(result, Is.Not.Null.And.EqualTo(savedResult).Using(_publicProperty));
        }

        [Test]
        public async Task WhenCacheInvalidatedByOtherRequestThenItWillBeRemovedFromCache()
        {
            var mediator = GetService<IMediator>();
            const int id = 1;
            var command = new Cached.Command(id);
            var query = new Cached.Query(id);
            var savedResult = new Cached.Result("Cached result");
            var cacheService = GetService<IDistributedCache>();
            string[] identifiers = { GetCacheKey(query), GetCacheKey<Cached.Query>(savedResult) };
            await cacheService.SetAsync(savedResult, identifiers, new());

            await mediator.Send(command);

            Assert.That(_cacheRemovedMessage, Does.Match("^Removing cache after getting (.+)CachedCommand"));
            foreach (string identifier in identifiers)
            {
                var cache = await cacheService.GetAsync<Cached.Result>(identifier);
                Assert.That(cache, Is.Null, identifier);
            }
        }

        [Test]
        public async Task WhenCacheConfiguredForCertainSlidingThenItWillNotHitAfterThatTime()
        {
            var mediator = GetService<IMediator>();
            var query = new Cached.Query(1);
            // first request to cache
            var firstCachingTime = DateTimeOffset.UtcNow;
            _utcNow = firstCachingTime;
            await mediator.Send(query);
            // change last time accessing cache
            _utcNow = _utcNow.Value.AddMinutes(1);
            await mediator.Send(query);
            _utcNow = _utcNow.Value.Add(Config.SlidingExpiration).AddSeconds(1);

            var result = await mediator.Send(query);

            Assert.That(_logMessage, Does.Match("^Cache miss, saving (.+) to cache for (.+)$"));
            var cacheService = GetService<IDistributedCache>();
            var cachedResult = await cacheService.GetAsync<Cached.Result>(GetCacheKey(query));
            Assert.That(cachedResult, Is.Not.Null.And.EqualTo(result).Using(_publicProperty));
        }

        [Test]
        public async Task WhenCacheConfiguredForCertainSlidingThenItWillHitDuringThatTime()
        {
            var mediator = GetService<IMediator>();
            var cacheService = GetService<IDistributedCache>();
            var query = new Cached.Query(1);
            // first request to cache
            var firstCachingTime = DateTimeOffset.UtcNow;
            _utcNow = firstCachingTime;
            await mediator.Send(query);
            // change last time accessing cache
            _utcNow = _utcNow.Value.AddMinutes(1);
            await mediator.Send(query);
            _utcNow = _utcNow.Value.Add(Config.SlidingExpiration).AddMinutes(-5);

            var result = await mediator.Send(query);

            Assert.That(_logMessage, Does.Match("^Cache hit, returning (.+) for (.+)$"));
            var cachedResult = await cacheService.GetAsync<Cached.Result>(GetCacheKey(query));
            Assert.That(cachedResult, Is.Not.Null.And.EqualTo(result).Using(_publicProperty));
        }

        [Test]
        public async Task WhenCacheConfiguredForFixedTimeThenItWillNotHitAfterThatTimeAndWillNeverBeCachedAnymore()
        {
            var mediator = GetService<IMediator>();
            var query = new Cached.WithAbsoluteExpiration(1);
            // first request to cache
            await mediator.Send(query);
            _utcNow = Config.AbsoluteExpiration.AddSeconds(1);

            await mediator.Send(query);

            Assert.That(_logMessage, Does.Match("^Cache miss, saving (.+) to cache for (.+)$"));
            var cacheService = GetService<IDistributedCache>();
            var cachedResult = await cacheService.GetAsync<Cached.Result>(GetCacheKey(query));
            Assert.That(cachedResult, Is.Null);
        }

        [Test]
        public async Task WhenCacheConfiguredForFixedTimeThenItWillHitBeforeReachingThatTime()
        {
            var mediator = GetService<IMediator>();
            var query = new Cached.WithAbsoluteExpiration(1);
            // first request to cache
            _utcNow = Config.AbsoluteExpiration.AddSeconds(-1);
            await mediator.Send(query);

            var result = await mediator.Send(query);

            Assert.That(_logMessage, Does.Match("^Cache hit, returning (.+) for (.+)$"));
            var cacheService = GetService<IDistributedCache>();
            var cachedResult = await cacheService.GetAsync<Cached.Result>(GetCacheKey(query));
            Assert.That(cachedResult, Is.Not.Null.And.EqualTo(result).Using(_publicProperty));
        }

        [Test]
        public async Task WhenCacheConfiguredRelativeToNowTimeThenItWillNotHitAfterThatTime()
        {
            var mediator = GetService<IMediator>();
            var query = new Cached.WithAbsoluteExpirationRelativeToNow(1);
            // first request to cache
            var timeWhenCaching = DateTimeOffset.UtcNow;
            _utcNow = timeWhenCaching;
            await mediator.Send(query);
            _utcNow = timeWhenCaching.Add(Config.AbsoluteExpirationRelativeToNow).AddSeconds(1);

            var result = await mediator.Send(query);

            Assert.That(_logMessage, Does.Match("^Cache miss, saving (.+) to cache for (.+)$"));
            var cacheService = GetService<IDistributedCache>();
            var cachedResult = await cacheService.GetAsync<Cached.Result>(GetCacheKey(query));
            Assert.That(cachedResult, Is.Not.Null.And.EqualTo(result).Using(_publicProperty));
        }

        private static string GetCacheKey<TQuery>(TQuery query) where TQuery : Cached.Query =>
            $"{typeof(TQuery).FullName}:{query.Id}";

        private static string GetCacheKey<TQuery>(Cached.Result result) where TQuery : Cached.Query =>
            $"{typeof(TQuery).FullName}:{result.Name}";

        public struct Cached
        {
            public record Command(int Id) : IRequest;
            public record Query(int Id) : IRequest<Result>;
            public record WithAbsoluteExpiration(int Id) : Query(Id);
            public record WithAbsoluteExpirationRelativeToNow(int Id) : Query(Id);
            public record Result(string Name);

            public class NormalCache : CacheRegistrar<Query, Result>
            {
                public NormalCache(IDistributedCache distributedCache) : base(distributedCache)
                { }

                protected override string GetRetrievingIdentifier(Query request) => request.Id.ToString();
                protected override IEnumerable<string> GetStoringIdentifiers(Query query, Result result)
                {
                    yield return query.Id.ToString();
                    yield return result.Name;
                }

                protected override TimeSpan? SlidingExpiration => Config.SlidingExpiration;
            }

            public class CacheInvalidate : CacheRemover<Command, Query, Result>
            {
                public CacheInvalidate(ICacheRegistrar<Query, Result> cacheRegistrar) : base(cacheRegistrar)
                { }

                protected override string GetRetrievingIdentifier(Command command) => command.Id.ToString();
            }

            public class AbsoluteExpirationCache : CacheRegistrar<WithAbsoluteExpiration, Result>
            {
                public AbsoluteExpirationCache(IDistributedCache distributedCache) : base(distributedCache)
                { }

                protected override DateTime? AbsoluteExpiration => Config.AbsoluteExpiration;
                protected override string GetRetrievingIdentifier(WithAbsoluteExpiration request) => request.Id.ToString();
                protected override IEnumerable<string> GetStoringIdentifiers(WithAbsoluteExpiration query, Result result)
                {
                    yield return query.Id.ToString();
                    yield return result.Name;
                }
            }

            public class AbsoluteExpirationRelativeToNowCache : CacheRegistrar<WithAbsoluteExpirationRelativeToNow, Result>
            {
                public AbsoluteExpirationRelativeToNowCache(IDistributedCache distributedCache) : base(distributedCache)
                { }

                protected override string GetRetrievingIdentifier(WithAbsoluteExpirationRelativeToNow request) => request.Id.ToString();
                protected override IEnumerable<string> GetStoringIdentifiers(WithAbsoluteExpirationRelativeToNow query, Result result)
                {
                    yield return query.Id.ToString();
                    yield return result.Name;
                }
                protected override TimeSpan? AbsoluteExpirationRelativeToNow => Config.AbsoluteExpirationRelativeToNow;
            }
        }

        private struct Config
        {
            public static readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(1);
            public static readonly DateTime AbsoluteExpiration = new(2030, 1, 1);
            public static readonly TimeSpan AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2);
        }
    }
}

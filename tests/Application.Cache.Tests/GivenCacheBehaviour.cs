using System.Collections;
using System.Reflection;
using DevKit.Application.Behaviour;
using DevKit.Application.Ports;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;

namespace DevKit.Application.Cache.Tests;

using static Testing;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public class GivenCacheBehaviour
{
    [Test]
    public async Task
        WhenRequestingToCachedQueryForTheFirstTimeThenItWillCacheTheResultForAllStoringIdentifiers() {
        string logMessage = string.Empty;
        var provider = Setup(services => services
            .MockCacheLog<Cached.Query>(log => logMessage = log)
            .MockCacheHandler<Cached.Query>());
        var mediator = provider.Resolve<IMediator>();
        var query = new Cached.Query(1);

        var result = await mediator.Send(query);

        Assert.That(logMessage, Does.Match("^Cache miss, saving (.+) to cache for (.+)$"));
        var cacheService = provider.Resolve<IDistributedCache>();
        string[] identifiers = { GetCacheKey(query), GetCacheKey<Cached.Query>(result) };
        foreach (string identifier in identifiers) {
            var cachedResult = await cacheService.GetAsync<Cached.Result>(identifier);
            Assert.That(cachedResult, Is.Not.Null.And.EqualTo(result).Using(_publicProperty));
        }
    }

    [Test]
    public async Task WhenRequestingToCachedQueryAfterFirstTimeThenItWillReturnCachedResult() {
        string logMessage = string.Empty;
        var provider = Setup(services => services
            .MockCacheHandler<Cached.Query>()
            .MockCacheLog<Cached.Query>(log => logMessage = log));
        var mediator = provider.Resolve<IMediator>();
        var query = new Cached.Query(1);
        var savedResult = new Cached.Result("Cached result");
        var cacheService = provider.Resolve<IDistributedCache>();
        await cacheService.SetAsync(savedResult,
            new[] { GetCacheKey(query), GetCacheKey<Cached.Query>(savedResult) }, new());

        var result = await mediator.Send(query);

        Assert.That(logMessage, Does.Match("^Cache hit, returning (.+) for (.+)$"));
        Assert.That(result, Is.Not.Null.And.EqualTo(savedResult).Using(_publicProperty));
    }

    [Test]
    public async Task WhenCacheInvalidatedByOtherRequestThenItWillBeRemovedFromCache() {
        string cacheRemovedMessage = string.Empty;
        var provider = Setup(services => services
            .MockCommandCacheLog(log => cacheRemovedMessage = log)
            .MockHandler<Cached.Command>()
            .MockCacheHandler<Cached.Query>());
        var mediator = provider.Resolve<IMediator>();
        const int id = 1;
        var command = new Cached.Command(id);
        var query = new Cached.Query(id);
        var savedResult = new Cached.Result("Cached result");
        var cacheService = provider.Resolve<IDistributedCache>();
        string[] identifiers = { GetCacheKey(query), GetCacheKey<Cached.Query>(savedResult) };
        await cacheService.SetAsync(savedResult, identifiers, new());

        await mediator.Send(command);

        Assert.That(cacheRemovedMessage, Does.Match("^Removing cache after getting (.+)CachedCommand"));
        foreach (string identifier in identifiers) {
            var cache = await cacheService.GetAsync<Cached.Result>(identifier);
            Assert.That(cache, Is.Null, identifier);
        }
    }

    [Test]
    public async Task WhenCacheConfiguredForCertainSlidingThenItWillNotHitAfterThatTime() {
        var time = new TimeWrapper();
        string logMessage = string.Empty;
        var provider = Setup(services => services
            .MockCacheHandler<Cached.Query>()
            .MockCacheLog<Cached.Query>(log => logMessage = log), time);
        var mediator = provider.Resolve<IMediator>();
        var query = new Cached.Query(1);
        // first request to cache
        var firstCachingTime = DateTimeOffset.UtcNow;
        time.UtcNow = firstCachingTime;
        await mediator.Send(query);
        // change last time accessing cache
        time.UtcNow = time.UtcNow.AddMinutes(1);
        await mediator.Send(query);
        time.UtcNow = time.UtcNow.Add(Config.SlidingExpiration).AddSeconds(1);

        var result = await mediator.Send(query);

        Assert.That(logMessage, Does.Match("^Cache miss, saving (.+) to cache for (.+)$"));
        var cacheService = provider.Resolve<IDistributedCache>();
        var cachedResult = await cacheService.GetAsync<Cached.Result>(GetCacheKey(query));
        Assert.That(cachedResult, Is.Not.Null.And.EqualTo(result).Using(_publicProperty));
    }

    [Test]
    public async Task WhenCacheConfiguredForCertainSlidingThenItWillHitDuringThatTime() {
        var time = new TimeWrapper();
        string logMessage = string.Empty;
        var provider = Setup(services => services
            .MockCacheHandler<Cached.Query>()
            .MockCacheLog<Cached.Query>(log => logMessage = log), time);
        var mediator = provider.Resolve<IMediator>();
        var cacheService = provider.Resolve<IDistributedCache>();
        var query = new Cached.Query(1);
        // first request to cache
        var firstCachingTime = time.UtcNow;
        time.UtcNow = firstCachingTime;
        await mediator.Send(query);
        // change last time accessing cache
        time.UtcNow = time.UtcNow.AddMinutes(1);
        await mediator.Send(query);
        time.UtcNow = time.UtcNow.Add(Config.SlidingExpiration).AddMinutes(-5);

        var result = await mediator.Send(query);

        Assert.That(logMessage, Does.Match("^Cache hit, returning (.+) for (.+)$"));
        var cachedResult = await cacheService.GetAsync<Cached.Result>(GetCacheKey(query));
        Assert.That(cachedResult, Is.Not.Null.And.EqualTo(result).Using(_publicProperty));
    }

    [Test]
    public async Task
        WhenCacheConfiguredForFixedTimeThenItWillNotHitAfterThatTimeAndWillNeverBeCachedAnymore() {
        string logMessage = string.Empty;
        var time = new TimeWrapper();
        var provider = Setup(services => services
            .MockCacheHandler<Cached.WithAbsoluteExpiration>()
            .MockCacheLog<Cached.WithAbsoluteExpiration>(log => logMessage = log), time);
        var mediator = provider.Resolve<IMediator>();
        var query = new Cached.WithAbsoluteExpiration(1);
        // first request to cache
        await mediator.Send(query);
        time.UtcNow = Config.AbsoluteExpiration.AddSeconds(1);

        await mediator.Send(query);

        Assert.That(logMessage, Does.Match("^Cache miss, saving (.+) to cache for (.+)$"));
        var cacheService = provider.Resolve<IDistributedCache>();
        var cachedResult = await cacheService.GetAsync<Cached.Result>(GetCacheKey(query));
        Assert.That(cachedResult, Is.Null);
    }

    [Test]
    public async Task WhenCacheConfiguredForFixedTimeThenItWillHitBeforeReachingThatTime() {
        string logMessage = string.Empty;
        var time = new TimeWrapper();
        var provider = Setup(services => services
            .MockCacheHandler<Cached.WithAbsoluteExpiration>()
            .MockCacheLog<Cached.WithAbsoluteExpiration>(log => logMessage = log), time);
        var mediator = provider.Resolve<IMediator>();
        var query = new Cached.WithAbsoluteExpiration(1);
        // first request to cache
        time.UtcNow = Config.AbsoluteExpiration.AddSeconds(-1);
        await mediator.Send(query);

        var result = await mediator.Send(query);

        Assert.That(logMessage, Does.Match("^Cache hit, returning (.+) for (.+)$"));
        var cacheService = provider.Resolve<IDistributedCache>();
        var cachedResult = await cacheService.GetAsync<Cached.Result>(GetCacheKey(query));
        Assert.That(cachedResult, Is.Not.Null.And.EqualTo(result).Using(_publicProperty));
    }

    [Test]
    public async Task WhenCacheConfiguredRelativeToNowTimeThenItWillNotHitAfterThatTime() {
        string logMessage = string.Empty;
        var time = new TimeWrapper();
        var provider = Setup(services => services
            .MockCacheHandler<Cached.WithAbsoluteExpirationRelativeToNow>()
            .MockCacheLog<Cached.WithAbsoluteExpirationRelativeToNow>(log => logMessage = log), time);
        var mediator = provider.Resolve<IMediator>();
        var query = new Cached.WithAbsoluteExpirationRelativeToNow(1);
        // first request to cache
        var timeWhenCaching = DateTimeOffset.UtcNow;
        time.UtcNow = timeWhenCaching;
        await mediator.Send(query);
        time.UtcNow = timeWhenCaching.Add(Config.AbsoluteExpirationRelativeToNow).AddSeconds(1);

        var result = await mediator.Send(query);

        Assert.That(logMessage, Does.Match("^Cache miss, saving (.+) to cache for (.+)$"));
        var cacheService = provider.Resolve<IDistributedCache>();
        var cachedResult = await cacheService.GetAsync<Cached.Result>(GetCacheKey(query));
        Assert.That(cachedResult, Is.Not.Null.And.EqualTo(result).Using(_publicProperty));
    }

    #region Helper

    private static readonly Assembly _thisAssembly = typeof(GivenCacheBehaviour).GetTypeInfo().Assembly;
    private readonly IComparer _publicProperty = ClassComparer.PublicProperty.Build();

    private IServiceProvider Setup(SetupService setup, TimeWrapper? time = null) => Configure(svc => setup(svc
        .AddCacheKit(_thisAssembly)
        .AddDistributedMemoryCache(options =>
        {
            var clock = new Mock<ISystemClock>();
            clock.SetupGet(x => x.UtcNow).Returns(() => time?.UtcNow ?? DateTimeOffset.UtcNow);
            options.Clock = clock.Object;
        })));

    private static string GetCacheKey<TQuery>(TQuery query) where TQuery : Cached.Query =>
        $"{typeof(TQuery).FullName}:{query.Id}";

    private static string GetCacheKey<TQuery>(Cached.Result result) where TQuery : Cached.Query =>
        $"{typeof(TQuery).FullName}:{result.Name}";

    class TimeWrapper
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
    }

    public struct Cached
    {
        public record Command(int Id) : IRequest;

        public record Query(int Id) : IRequest<Result>;

        public record WithAbsoluteExpiration(int Id) : Query(Id);

        public record WithAbsoluteExpirationRelativeToNow(int Id) : Query(Id);

        public record Result(string Name);

        public class NormalCache : CacheRegistrar<Query, Result>
        {
            public NormalCache(IDistributedCache distributedCache) : base(distributedCache) { }

            protected override TimeSpan? SlidingExpiration => Config.SlidingExpiration;

            protected override string GetRetrievingIdentifier(Query request) => request.Id.ToString();

            protected override IEnumerable<string> GetStoringIdentifiers(Query query, Result result) {
                yield return query.Id.ToString();
                yield return result.Name;
            }
        }

        public class CacheInvalidate : CacheRemover<Command, Query, Result>
        {
            public CacheInvalidate(ICacheRegistrar<Query, Result> cacheRegistrar) : base(cacheRegistrar) { }

            protected override string GetRetrievingIdentifier(Command command) => command.Id.ToString();
        }

        public class AbsoluteExpirationCache : CacheRegistrar<WithAbsoluteExpiration, Result>
        {
            public AbsoluteExpirationCache(IDistributedCache distributedCache) : base(distributedCache) { }

            protected override DateTime? AbsoluteExpiration => Config.AbsoluteExpiration;

            protected override string GetRetrievingIdentifier(WithAbsoluteExpiration request) =>
                request.Id.ToString();

            protected override IEnumerable<string> GetStoringIdentifiers(WithAbsoluteExpiration query,
                Result result) {
                yield return query.Id.ToString();
                yield return result.Name;
            }
        }

        public class AbsoluteExpirationRelativeToNowCache :
            CacheRegistrar<WithAbsoluteExpirationRelativeToNow, Result>
        {
            public AbsoluteExpirationRelativeToNowCache(IDistributedCache distributedCache) :
                base(distributedCache) {
            }

            protected override TimeSpan? AbsoluteExpirationRelativeToNow =>
                Config.AbsoluteExpirationRelativeToNow;

            protected override string GetRetrievingIdentifier(WithAbsoluteExpirationRelativeToNow request) =>
                request.Id.ToString();

            protected override IEnumerable<string> GetStoringIdentifiers(
                WithAbsoluteExpirationRelativeToNow query, Result result) {
                yield return query.Id.ToString();
                yield return result.Name;
            }
        }
    }

    private struct Config
    {
        public static readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(1);
        public static readonly DateTime AbsoluteExpiration = new(2030, 1, 1);
        public static readonly TimeSpan AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2);
    }

    #endregion
}

file static class DependencyExtensions
{
    public static IServiceCollection MockCacheHandler<TRequest>(this IServiceCollection services)
        where TRequest : GivenCacheBehaviour.Cached.Query =>
        services.MockHandler<TRequest, GivenCacheBehaviour.Cached.Result>(q => new($"result {q.Id}"));

    public static IServiceCollection MockCacheLog<TRequest>(this IServiceCollection services,
        Action<string> logCallback) where TRequest : IRequest<GivenCacheBehaviour.Cached.Result> =>
        services.MockLogger<CacheBehavior<TRequest, GivenCacheBehaviour.Cached.Result>>(logCallback);

    public static IServiceCollection MockCommandCacheLog(this IServiceCollection services,
        Action<string> logCallback) => services
        .MockLogger<CacheInvalidationBehavior<GivenCacheBehaviour.Cached.Command, Unit>>(logCallback);
}

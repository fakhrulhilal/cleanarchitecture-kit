using DevKit.Application.Models;
using DevKit.Domain.Models;
using DevKit.Infrastructure.Mail.MailKit.Tests.HMailServer;
using DevKit.Infrastructure.Mail.MailKit.Tests.Smtp4Dev;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace DevKit.Infrastructure.Mail.MailKit.Tests;

[SetUpFixture]
public class Testing
{
    private static IServiceProvider _provider = default!;
    private static readonly object _lock = new();
    private static int _testId;

    internal static int NextSeed
    {
        get
        {
            lock (_lock) return ++_testId;
        }
    }

    [OneTimeSetUp]
    public void SetUp() => _provider = Configure();

    internal static async Task<Message> FindEmail(Func<Message, bool> filter) {
        var client = _provider.Resolve<ISmtp4DevClient>();
        var messages = await client.GetMessagesAsync();
        var message = messages.FirstOrDefault(filter);
        if (message == null) return Message.Empty;

        message.HtmlBody = await client.GetHtmlBodyAsync(message.Id);
        return message;
    }

    internal static IServiceProvider Configure(SetupService? configure = null) => DevKit.Testing.Configure(
        services => configure?.Invoke(services.AddSingleton(provider =>
            {
                var cfg = provider.GetRequiredService<IConfiguration>();
                string baseKey = typeof(EmailConfig.OutgoingConfig).GetConfigName();
                int port = cfg.GetValue<int>($"{baseKey}:ManagementPort");
                var mailConfig = provider.GetRequiredService<EmailConfig.OutgoingConfig>();
                return RestService.For<ISmtp4DevClient>($"http://{mailConfig.ServerAddress}:{port}");
            })
            .AddMailKit()
            .AddScoped<Server>()));

    internal static TService Resolve<TService>() where TService : notnull => _provider.Resolve<TService>();

    internal static TService Resolve<TService>(Func<TService, bool> filter) where TService : notnull =>
        _provider.Resolve(filter);
}

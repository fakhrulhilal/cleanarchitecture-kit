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
    private static readonly Startup _bootstrapper = new();
    private static readonly object _lock = new();
    private static int _testId;

    internal static int NextSeed
    {
        get
        {
            lock (_lock) return ++_testId;
        }
    }

    [OneTimeTearDown]
    public void TearDown() => _bootstrapper.CleanUp();

    [OneTimeSetUp]
    public void SetUp() => _bootstrapper.ConfigureServices(WithDefaultSetup);

    internal static async Task<Message> FindEmail(Func<Message, bool> filter) {
        var client = _bootstrapper.GetService<ISmtp4DevClient>();
        var messages = await client.GetMessagesAsync();
        var message = messages.FirstOrDefault(filter);
        if (message == null) return Message.Empty;

        message.HtmlBody = await client.GetHtmlBodyAsync(message.Id);
        return message;
    }

    private static void WithDefaultSetup(IServiceCollection services, IConfiguration _) => services
        .AddSingleton(provider =>
        {
            var cfg = provider.GetRequiredService<IConfiguration>();
            string baseKey = typeof(EmailConfig.OutgoingConfig).GetConfigName();
            int port = cfg.GetValue<int>($"{baseKey}:ManagementPort");
            var mailConfig = provider.GetRequiredService<EmailConfig.OutgoingConfig>();
            return RestService.For<ISmtp4DevClient>($"http://{mailConfig.ServerAddress}:{port}");
        })
        .AddMailKit()
        .AddScoped<Server>();

    internal static Startup ConfigureServices(Action<IServiceCollection> configure) {
        var bootstrapper = new Startup();
        bootstrapper.ConfigureServices((services, config) =>
        {
            WithDefaultSetup(services, config);
            configure(services);
        });
        return bootstrapper;
    }

    internal static TService GetService<TService>() where TService : notnull =>
        _bootstrapper.GetService<TService>();

    internal static TService GetService<TService>(Func<TService, bool> filter) where TService : notnull =>
        _bootstrapper.GetService(filter);
}

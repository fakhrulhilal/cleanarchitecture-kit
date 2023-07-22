using DevKit.Application.Models;
using DevKit.Application.Ports;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DevKit.Infrastructure.Mail.MailKit.Tests;

using static Testing;

/// <summary>
///     run SMTP4Dev with this command:
///     docker run --rm -it -p 8025:80 -p 2525:25 --name mailsrv -d rnwood/smtp4dev
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Fixtures)]
public class GivenMailKitMailSender
{
    [Test]
    public async Task WhenServerAvailableWithValidMessageThenItShouldBeSentSuccessfully() {
        string id = Guid.NewGuid().ToString("n");
        string domain =
            $"{nameof(WhenServerAvailableWithValidMessageThenItShouldBeSentSuccessfully)}.{nameof(GivenMailKitMailSender)}";
        var message = new MailMessage {
            Subject = id,
            HtmlBody = "HTML body",
            From = $"sender@{domain}",
            To = { $"to@{domain}" }
        };
        var sut = Resolve<IMailSender>();
        var config = Resolve<EmailConfig.OutgoingConfig>();

        await sut.SendAsync(config, message);

        var sentMessage = await FindEmail(email => id == email.Subject);
        Assert.That(sentMessage, Is.Not.Null);
        Assert.That(sentMessage.Subject, Is.EqualTo(message.Subject));
        Assert.That(sentMessage.HtmlBody, Is.EqualTo(message.HtmlBody));
        Assert.That(sentMessage.From, Is.EqualTo(message.From.Address));
        Assert.That(sentMessage.To, Does.Contain(message.To[0].Address));
    }

    [Test]
    public async Task WhenConnectingUsingSecureConnectionThenItShouldWork() {
        string id = Guid.NewGuid().ToString("n");
        string domain =
            $"{nameof(WhenServerAvailableWithValidMessageThenItShouldBeSentSuccessfully)}.{nameof(GivenMailKitMailSender)}";
        var message = new MailMessage {
            Subject = id,
            HtmlBody = "HTML body",
            From = $"sender@{domain}",
            To = { $"to@{domain}" }
        };
        var sut = Resolve<IMailSender>();
        var config = Resolve<EmailConfig.OutgoingConfig>();

        config.UseSecureMode = true;
        await sut.SendAsync(config, message);

        var sentMessage = await FindEmail(email => id == email.Subject);
        Assert.That(sentMessage, Is.Not.Null);
        Assert.That(sentMessage.Subject, Is.EqualTo(message.Subject));
    }

    [Test]
    public async Task WhenBothUsernameAndPasswordProvidedThenItWillUseThemToAuthenticate() {
        var client = new Mock<ISmtpClient>();
        var bootstrapper = Configure(services => services.AddTransient(_ => client.Object));
        var sut = bootstrapper.Resolve<IMailSender>();
        var account = new EmailConfig.OutgoingConfig { Username = "sender@domain", Password = "secret" };
        var message = new MailMessage {
            Subject = Guid.NewGuid().ToString("n"),
            HtmlBody = nameof(WhenBothUsernameAndPasswordProvidedThenItWillUseThemToAuthenticate),
            From = account.Username,
            To = { "random@domain" }
        };

        await sut.SendAsync(account, message);

        client.Verify(x => x.AuthenticateAsync(It.Is<string>(u => u == account.Username),
            It.Is<string>(p => p == account.Password), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task WhenOneOfUsernameOrPasswordNotProvidedThenItWillConnectToServerAnonymously() {
        var client = new Mock<ISmtpClient>();
        var bootstrapper = Configure(services => services.AddTransient(_ => client.Object));
        var sut = bootstrapper.Resolve<IMailSender>();
        var account = new EmailConfig.OutgoingConfig { Username = "sender@domain", Password = string.Empty };
        var message = new MailMessage {
            Subject = Guid.NewGuid().ToString("n"),
            HtmlBody = nameof(WhenBothUsernameAndPasswordProvidedThenItWillUseThemToAuthenticate),
            From = account.Username,
            To = { "random@domain" }
        };

        await sut.SendAsync(account, message);

        client.Verify(x => x.AuthenticateAsync(It.Is<string>(u => u == account.Username),
            It.Is<string>(p => p == account.Password), It.IsAny<CancellationToken>()), Times.Never);
    }
}

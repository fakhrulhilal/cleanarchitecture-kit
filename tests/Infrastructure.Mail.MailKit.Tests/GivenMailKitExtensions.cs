using System.Security.Authentication;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace DevKit.Infrastructure.Mail.MailKit.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public class GivenMailKitExtensions
{
    [Test]
    public void WhenUsingSecureModeThenItWillAllTlsSpecifications() {
        using IMailService client = new SmtpClient();

        client.ConfigureSecurity(true);

        Assert.That(client.SslProtocols & SslProtocols.Tls12, Is.EqualTo(SslProtocols.Tls12));
        Assert.That(client.SslProtocols & SslProtocols.Tls13, Is.EqualTo(SslProtocols.Tls13));
    }

    [Test]
    public void WhenUsingSecureModeThenItWillByPassCertificateRevocationCheck() {
        using IMailService client = new ImapClient();

        client.ConfigureSecurity(true);

        Assert.That(client.CheckCertificateRevocation, Is.False);
    }

    [Test]
    public void WhenUsingSecureModeThenItWillUseAutomaticSecureSocketOptionResolution() {
        using IMailService client = new ImapClient();

        var socketOption = client.ConfigureSecurity(true);

        Assert.That(socketOption, Is.EqualTo(SecureSocketOptions.Auto));
    }

    [Test]
    public void WhenUsingNonSecureModeThenSslProtocolWillBeOff() {
        using IMailService client = new ImapClient();

        client.ConfigureSecurity(false);

        Assert.That(client.SslProtocols, Is.EqualTo(SslProtocols.None));
    }

    [Test]
    public void WhenUsingNonSecureModeThenNoSecureSocketOptionWillBeUsed() {
        using IMailService client = new SmtpClient();

        var socketOptions = client.ConfigureSecurity(false);

        Assert.That(socketOptions, Is.EqualTo(SecureSocketOptions.None));
    }
}

using MailKit;
using MailKit.Security;
using static System.Security.Authentication.SslProtocols;

namespace DevKit.Infrastructure.Mail;

internal static class MailClientExtensions
{
    internal static SecureSocketOptions ConfigureSecurity(this IMailService mailer, bool useSecureMode) {
        if (!useSecureMode) {
            mailer.SslProtocols = None;
            return SecureSocketOptions.None;
        }

        mailer.SslProtocols = Tls12 | Tls13;
        mailer.CheckCertificateRevocation = false;
        mailer.ServerCertificateValidationCallback = (_, _, _, _) => true;
        return SecureSocketOptions.Auto;
    }
}

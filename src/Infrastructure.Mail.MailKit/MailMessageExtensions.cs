using System;
using System.Linq;
using FM.Application.Mail;
using FM.Domain.ValueObjects;
using MimeKit;

namespace FM.Infrastructure.Mail
{
    internal static class MailMessageExtensions
    {
        internal static MimeMessage ToMailKitMessage(this IMailMessage original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            var output = new MimeMessage();

            output.From.Add(original.From.Convert());
            output.To.AddRange(original.To.Select(Convert));
            output.Subject = original.Subject;
            var builder = new BodyBuilder();
            if (!string.IsNullOrWhiteSpace(original.HtmlBody))
                builder.HtmlBody = original.HtmlBody;
            if (!string.IsNullOrWhiteSpace(original.PlainBody))
                builder.TextBody = original.PlainBody;
            output.Body = builder.ToMessageBody();
            return output;
        }

        internal static IMailMessage ToPort(this MimeMessage original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (original.From.Count <= 0)
                throw new ArgumentException("An email must have at least valid FROM address", nameof(original));
            var message = new MailKitMessage
            {
                Subject = original.Subject,
                PlainBody = original.TextBody,
                HtmlBody = original.HtmlBody,
                From = original.From.First().Convert()
            };
            message.To.AddRange(original.To.Select(Convert));
            message.Cc.AddRange(original.Cc.Select(Convert));
            return message;
        }

        private static InternetAddress Convert(this Email email) =>
            new MailboxAddress(email.DisplayName, email.Address);

        private static Email Convert(this InternetAddress address) => Email.Parse(address.ToString());
    }
}

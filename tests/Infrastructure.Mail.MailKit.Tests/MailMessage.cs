using System.Collections.Generic;
using FM.Application.Mail;
using FM.Domain.ValueObjects;

namespace FM.Infrastructure.Mail.MailKit.Tests
{
    public class MailMessage : IMailMessage
    {
        public Email From { get; init; } = Email.Empty;
        public List<Email> To { get; } = new();
        public List<Email> Cc { get; } = new();
        public string Subject { get; init; } = string.Empty;
        public string HtmlBody { get; init; } = string.Empty;
        public string PlainBody => HtmlBody;
    }
}

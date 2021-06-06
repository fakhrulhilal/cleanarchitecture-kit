using System;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace FM.Infrastructure.Mail.MailKit.Tests.Smtp4Dev
{
    public class Message
    {
        public static Message Empty => new() { IsEmpty = true };
        public bool IsEmpty { get; private init; }

        public Guid Id { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public DateTime ReceiveDate { get; set; }
        public string Subject { get; set; } = string.Empty;
        public bool IsUnread { get; set; }
        public int AttachmentCount { get; set; }
        public string HtmlBody { get; set; } = string.Empty;
    }
}

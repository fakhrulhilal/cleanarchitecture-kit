using System;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FM.Infrastructure.Mail.MailKit.Tests.Smtp4Dev
{
    public class Message
    {
        public Guid Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string Subject { get; set; }
        public bool IsUnread { get; set; }
        public int AttachmentCount { get; set; }
        public string HtmlBody { get; set; }
    }
}

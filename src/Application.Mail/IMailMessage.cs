using System.Collections.Generic;
using FM.Domain.ValueObjects;

namespace FM.Application.Mail
{
    public interface IMailMessage
    {
        public Email From { get; }
        public List<Email> To { get; }
        public List<Email> Cc { get; }
        public string Subject { get; }
        public string HtmlBody { get; }
        public string PlainBody { get; }
    }
}

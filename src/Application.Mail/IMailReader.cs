using System.Collections.Generic;
using System.Threading;

namespace FM.Application.Mail
{
    public interface IMailReader
    {
        MailProtocol SupportedProtocol { get; }

        IAsyncEnumerable<IMailMessage> GetMessagesAsync(EmailConfig.IncomingConfig cfg, MailQuery? query = null,
            CancellationToken cancellationToken = default);
    }
}

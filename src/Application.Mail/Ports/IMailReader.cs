using DevKit.Application.Models;

namespace DevKit.Application.Ports;

public interface IMailReader
{
    MailProtocol SupportedProtocol { get; }

    IAsyncEnumerable<IMailMessage> GetMessagesAsync(EmailConfig.IncomingConfig cfg, MailQuery? query = null,
        CancellationToken cancellationToken = default);
}

using DevKit.Application.Models;

namespace DevKit.Application.Ports;

public interface IMailSender
{
    Task SendAsync(EmailConfig.OutgoingConfig config, IMailMessage message,
        CancellationToken cancellationToken = default);
}

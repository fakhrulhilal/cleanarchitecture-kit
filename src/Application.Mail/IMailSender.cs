using System.Threading;
using System.Threading.Tasks;

namespace FM.Application.Mail
{
    public interface IMailSender
    {
        Task SendAsync(EmailConfig.OutgoingConfig config, IMailMessage message,
            CancellationToken cancellationToken = default);
    }
}

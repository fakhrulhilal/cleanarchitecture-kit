using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FM.Application.Mail;
using FM.Domain.Common;
using MailKit.Net.Pop3;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace FM.Infrastructure.Mail
{
    /// <summary>
    /// POP3 client
    /// </summary>
    public class MailKitPop3Reader : MailKitMailReader<MailKitPop3Reader, IPop3Client, IPop3Client, int>
    {
        public MailKitPop3Reader(ILogger<MailKitPop3Reader> log, RetryConfig retryConfig, IPop3Client client)
            : base(log, retryConfig, client)
        { }

        public override MailProtocol SupportedProtocol => MailProtocol.Pop3;

        protected override async Task<bool> DeleteMessageAsync(IPop3Client container, int id, CancellationToken cancellationToken)
        {
            await container.DeleteMessageAsync(id, cancellationToken);
            return true;
        }

        protected override Task<int[]> GetMessageIdsAsync(IPop3Client container, int? limit, CancellationToken cancellationToken)
        {
            int totalNewMessage = container.GetMessageCount(cancellationToken);
            int totalFetchPop3 = limit.HasValue && totalNewMessage > limit
                ? limit.Value
                : totalNewMessage;
            return Task.FromResult(Enumerable.Range(0, totalFetchPop3).ToArray());
        }

        protected override async Task<MimeMessage?> GetMessageAsync(IPop3Client container, int id,
            CancellationToken cancellationToken) => await container.GetMessageAsync(id, cancellationToken);
    }
}

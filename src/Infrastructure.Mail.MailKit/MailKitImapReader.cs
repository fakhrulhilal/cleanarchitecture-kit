using DevKit.Application.Models;
using DevKit.Domain.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace DevKit.Infrastructure.Mail;

public class MailKitImapReader(ILogger<MailKitImapReader> log, RetryConfig retryConfig, IImapClient client)
    : MailKitMailReader<MailKitImapReader, IImapClient, IMailFolder, UniqueId>(log, retryConfig, client)
{
    public override MailProtocol SupportedProtocol => MailProtocol.Imap;

    protected override async Task<IMailFolder?> InitContainerAsync(IImapClient client,
        EmailConfig.IncomingConfig config, CancellationToken cancellationToken) {
        _ = await base.InitContainerAsync(client, config, cancellationToken);
        var mailbox = await client.GetFolderAsync(config.MailBox, cancellationToken);
        if (mailbox == null)
            throw new InvalidOperationException(
                $"Email account {config.Username} doesn't have mailbox '{config.MailBox}'");
        _ = await mailbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken);
        return mailbox;
    }

    protected override async Task<bool> DeleteMessageAsync(IMailFolder container, UniqueId id,
        CancellationToken cancellationToken) {
        await container.AddFlagsAsync(id, MessageFlags.Deleted, true, cancellationToken);
        await container.ExpungeAsync(cancellationToken);
        return true;
    }

    protected override async Task<UniqueId[]> GetMessageIdsAsync(IMailFolder container, int? limit,
        CancellationToken cancellationToken) {
        var ids = await container.SearchAsync(SearchQuery.All, cancellationToken);
        int totalFetchImap = limit.HasValue && ids.Count > limit ? limit.Value : ids.Count;
        return ids.Take(totalFetchImap).ToArray();
    }

    protected override async Task<MimeMessage?> GetMessageAsync(IMailFolder container, UniqueId id,
        CancellationToken cancellationToken) => await container.GetMessageAsync(id, cancellationToken);
}

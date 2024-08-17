using System.Net.Sockets;
using System.Runtime.CompilerServices;
using DevKit.Application.Models;
using DevKit.Application.Ports;
using DevKit.Domain.Models;
using MailKit;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using Polly;

namespace DevKit.Infrastructure.Mail;

public abstract class MailKitMailReader<TReader, TClient, TMessageContainer, TMessageId>(
    ILogger<MailKitMailReader<TReader, TClient, TMessageContainer, TMessageId>> log,
    RetryConfig retryConfig,
    TClient client)
    : IMailReader
    where TReader : IMailReader
    where TClient : IMailService
    where TMessageId : struct
    where TMessageContainer : class
{
    private readonly TClient _client = client;

    public abstract MailProtocol SupportedProtocol { get; }

    public async IAsyncEnumerable<IMailMessage> GetMessagesAsync(EmailConfig.IncomingConfig cfg,
        MailQuery? query = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        if (!new[] { MailProtocol.Imap, MailProtocol.Pop3 }.Contains(cfg.Protocol))
            throw new InvalidOperationException($"Invalid email protocol: {cfg.Protocol}");
        var container = await Policy
            .Handle<SocketException>().Or<IOException>().Or<SslHandshakeException>()
            .WaitAndRetryAsync(retryConfig.Max, _ => retryConfig.Delay,
                (exc, delay, attempt, _) => log.LogError(exc, Message.ConnError, attempt, cfg, delay))
            .ExecuteAsync(token => InitContainerAsync(_client, cfg, token), cancellationToken);
        if (container == null)
            throw new InvalidOperationException("Message container must be initialized properly");

        var ids = await GetMessageIdsAsync(container, query?.Limit, cancellationToken);
        foreach (var id in ids) {
            var message = await TryOrNull(token => GetMessageAsync(container, id, token), cancellationToken,
                Message.ReceiveError, retryConfig.Max, id, cfg.Username);
            if (message == null || (!query?.AutoDelete ?? false)) continue;

            _ = await TryOrNull(token => DeleteMessageAsync(container, id, token), cancellationToken,
                Message.DeleteError, retryConfig.Max, id, cfg.Username);
            yield return message.ToPort();
        }

        await _client.DisconnectAsync(true, cancellationToken);
    }

    private async Task<TResult?> TryOrNull<TResult>(Func<CancellationToken, Task<TResult?>> action,
        CancellationToken cancellationToken, string errorMessage, params object[] arguments) {
        try {
            return await Policy.Handle<SocketException>()
                .WaitAndRetryAsync(retryConfig.Max, _ => retryConfig.Delay,
                    (exc, delay, attempt, _) => log.LogError(exc, Message.RetryError, delay, attempt))
                .ExecuteAsync(action, cancellationToken);
        }
        catch (Exception exception) {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            log.LogError(exception, errorMessage, arguments);
        }

        return default;
    }

    protected virtual async Task<TMessageContainer?> InitContainerAsync(TClient client,
        EmailConfig.IncomingConfig config, CancellationToken cancellationToken) {
        if (string.IsNullOrWhiteSpace(config.Username))
            throw new ArgumentException("Email account username cannot be empty", nameof(config));
        log.LogDebug(Message.ConnDebug, config);
        var socketOptions = client.ConfigureSecurity(config.UseSecureMode);
        await client.ConnectAsync(config.ServerAddress, config.Port, socketOptions, cancellationToken);
        await client.AuthenticateAsync(config.Username, config.Password, cancellationToken);
        return client as TMessageContainer;
    }

    protected abstract Task<bool> DeleteMessageAsync(TMessageContainer container, TMessageId id,
        CancellationToken cancellationToken);

    protected abstract Task<TMessageId[]> GetMessageIdsAsync(TMessageContainer container, int? limit,
        CancellationToken cancellationToken);

    protected abstract Task<MimeMessage?> GetMessageAsync(TMessageContainer container, TMessageId id,
        CancellationToken cancellationToken);

    private struct Message
    {
        internal const string ConnDebug = "Connecting to mail server with {@Config}";

        internal const string ConnError =
            "Error connecting to mail server for the {Attempt} time(s) with config {@Config}, retrying after {Delay}";

        internal const string ReceiveError =
            "Error getting message after {Attempt} for ID {Id} from {Username}";

        internal const string DeleteError =
            "An error occurs after {Attempt} when trying to delete {Id} from {Username}";

        internal const string RetryError = "Failed for the {Attempt}, retrying after {Delay}";
    }
}

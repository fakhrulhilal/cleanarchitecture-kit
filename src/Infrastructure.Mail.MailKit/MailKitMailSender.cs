using System.Net.Sockets;
using DevKit.Application.Models;
using DevKit.Application.Ports;
using DevKit.Domain.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Polly;

namespace DevKit.Infrastructure.Mail;

public class MailKitMailSender(RetryConfig retryConfig, ILogger<MailKitMailSender> log, ISmtpClient client)
    : IMailSender
{
    public async Task SendAsync(EmailConfig.OutgoingConfig config, IMailMessage message,
        CancellationToken cancellationToken = default) {
        var mailMessage = message.ToMailKitMessage();
        await ConnectAsync(config, cancellationToken);
        log.LogInformation(Message.SendInfo, message, config);
        await Policy.Handle<SocketException>()
            .WaitAndRetryAsync(retryConfig.Max, _ => retryConfig.Delay,
                (exception, ts) => log.LogError(exception, "Error sending mail, retrying in {Duration}", ts))
            .ExecuteAsync(token => client.SendAsync(mailMessage, token), cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private async Task ConnectAsync(EmailConfig.OutgoingConfig cfg, CancellationToken cancellationToken) {
        log.LogDebug("Connecting to mail server using {@Config}", cfg);
        var socketOptions = client.ConfigureSecurity(cfg.UseSecureMode);
        await Policy
            .Handle<SocketException>().Or<IOException>().Or<SslHandshakeException>()
            .WaitAndRetryAsync(retryConfig.Max, _ => retryConfig.Delay,
                (exc, delay, attempt, _) => log.LogError(exc, Message.ConnError, attempt, cfg, delay))
            .ExecuteAsync(token => client.ConnectAsync(cfg.ServerAddress, cfg.Port, socketOptions, token),
                cancellationToken);
        if (!string.IsNullOrWhiteSpace(cfg.Username) && !string.IsNullOrWhiteSpace(cfg.Password))
            await Policy.Handle<SocketException>()
                .WaitAndRetryAsync(retryConfig.Max, _ => retryConfig.Delay,
                    (exc, delay, attempt, _) => log.LogError(exc, Message.AuthError, attempt, cfg, delay))
                .ExecuteAsync(token => client.AuthenticateAsync(cfg.Username, cfg.Password, token),
                    cancellationToken);
    }

    private struct Message
    {
        internal const string AuthError =
            "Error authenticating for the {Attempt} with config {@Config}, retrying after {Delay}";

        internal const string ConnError =
            "Error connecting to mail server for the {Attempt} time(s) with config {@Config}, retrying after {Delay}";

        internal const string SendInfo = "Sending email {@Message} with {@Option}";
    }
}

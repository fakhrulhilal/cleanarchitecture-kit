using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FM.Application.Mail;
using FM.Domain.Common;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Polly;

namespace FM.Infrastructure.Mail
{
    public class MailKitMailSender : IMailSender
    {
        private readonly ILogger<MailKitMailSender> _log;
        private readonly RetryConfig _retryConfig;
        private readonly ISmtpClient _client;

        public MailKitMailSender(RetryConfig retryConfig, ILogger<MailKitMailSender> log, ISmtpClient client)
        {
            _client = client;
            _retryConfig = retryConfig;
            _log = log;
        }

        public async Task SendAsync(EmailConfig.OutgoingConfig config, IMailMessage message,
            CancellationToken cancellationToken = default)
        {
            var mailMessage = message.ToMailKitMessage();
            await ConnectAsync(_client, config, cancellationToken);
            _log.LogInformation(Message.SendInfo, message, config);
            await Policy.Handle<SocketException>()
                .WaitAndRetryAsync(_retryConfig.Max, _ => _retryConfig.Delay,
                    (exception, ts) => _log.LogError(exception, $"Error sending mail, retrying in {ts}."))
                .ExecuteAsync(token => _client.SendAsync(mailMessage, token), cancellationToken);
            await _client.DisconnectAsync(true, cancellationToken);
        }

        private async Task ConnectAsync(ISmtpClient client, EmailConfig.OutgoingConfig cfg,
            CancellationToken cancellationToken)
        {
            _log.LogDebug("Connecting to mail server using {@Config}", cfg);
            var socketOptions = client.ConfigureSecurity(cfg.UseSecureMode);
            await Policy
                .Handle<SocketException>().Or<System.IO.IOException>().Or<SslHandshakeException>()
                .WaitAndRetryAsync(_retryConfig.Max, _ => _retryConfig.Delay,
                    (exc, delay, attempt, _) => _log.LogError(exc, Message.ConnError, attempt, cfg, delay))
                .ExecuteAsync(token => client.ConnectAsync(cfg.ServerAddress, cfg.Port, socketOptions, token),
                    cancellationToken);
            if (!string.IsNullOrWhiteSpace(cfg.Username) && !string.IsNullOrWhiteSpace(cfg.Password))
                await Policy.Handle<SocketException>()
                    .WaitAndRetryAsync(_retryConfig.Max, _ => _retryConfig.Delay,
                        (exc, delay, attempt, _) => _log.LogError(exc, Message.AuthError, attempt, cfg, delay))
                    .ExecuteAsync(token => client.AuthenticateAsync(cfg.Username, cfg.Password, token),
                        cancellationToken);
        }

        private struct Message
        {
            internal const string AuthError = "Error authenticating for the {Attempt} with config {@Config}, retrying after {Delay}";
            internal const string ConnError =
                "Error connecting to mail server for the {Attempt} time(s) with config {@Config}, retrying after {Delay}";
            internal const string SendInfo = "Sending email {@Message} with {@Option}";
        }
    }
}

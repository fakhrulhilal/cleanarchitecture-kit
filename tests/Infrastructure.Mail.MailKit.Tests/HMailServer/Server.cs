using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FM.Application.Mail;
using FM.Domain.ValueObjects;
using hMailServer;
using Microsoft.Extensions.Configuration;

namespace FM.Infrastructure.Mail.MailKit.Tests.HMailServer
{
    using static Testing;

    /// <summary>
    /// Connect to existing hMailServer and manage the account
    /// </summary>
    internal class Server
    {
        private readonly List<Email> _addedAccounts = new();
        private hMailServer.Application _mailServer = Moq.Mock.Of<hMailServer.Application>();
        private readonly IConfiguration _config;
        private const string DefaultPassword = "P@ssw0rd";

        private readonly List<MailConnectionConfig> _connections = new();

        public Server(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        internal void Connect()
        {
            var serverConfig = _config.GetSection("Email:Server").Get<Config>();
            try
            {
                _mailServer = InitComType<hMailServer.Application>();
                _ = _mailServer.Authenticate(serverConfig.Username, serverConfig.Password);
                _mailServer.Connect();
                PopulateConnections();
            }
            catch (COMException exception)
            {
                string connection = $"{serverConfig.Username}:{serverConfig.Password}";
                string info =
                    "Make sure you have configured server running hMailServer to allow launch permission for COM+ of hMailServer.";
                string errorMessage = $"Can't connect to hMailServer ({connection}). {info}";
                throw new InvalidOperationException(errorMessage, exception);
            }
        }

        private void PopulateConnections()
        {
            var validProtocols = new[] { eSessionType.eSTIMAP, eSessionType.eSTPOP3, eSessionType.eSTSMTP };
            for (int i = 0; i < _mailServer.Settings.TCPIPPorts.Count; i++)
            {
                var portSetting = _mailServer.Settings.TCPIPPorts[i];
                if (!validProtocols.Contains(portSetting.Protocol)) continue;
                string protocolAsString = portSetting.Protocol.ToString().Replace("eST", string.Empty);
                var protocol = Enum.TryParse(protocolAsString, true, out MailProtocol parsed) ? parsed : default;
                _connections.Add(new MailConnectionConfig
                {
                    Port = portSetting.PortNumber,
                    UseSecureMode = portSetting.UseSSL,
                    Protocol = protocol
                });
            }
        }

        public EmailConfig.IncomingConfig CreateUniqueAccount(string domainAddress)
        {
            var domainManager = FindAndEnsureDomainAvailable(domainAddress);
            var account = domainManager.Accounts.Add();
            int number = NextSeed;
            var email = Email.Create($"User {number}", $"user.{number}@{domainManager.Name}");
            account.Active = true;
            account.Address = email.Address;
            account.PersonFirstName = email.DisplayName;
            account.Password = DefaultPassword;
            account.Save();
            _addedAccounts.Add(email);
            // right now, it only supports hMailServer within the same machine
            return new EmailConfig.IncomingConfig
            {
                Username = email.Address,
                Password = DefaultPassword,
                ServerAddress = "localhost"
            };
        }

        public void PopulateConnection(EmailConfig.IncomingConfig config, MailProtocol protocol)
        {
            var connection = _connections.FirstOrDefault(c => c.Protocol == protocol);
            if (connection == null)
                throw new InvalidOperationException($"hMailServer is not configured for protocol {protocol}");
            config.Port = connection.Port;
            config.UseSecureMode = connection.UseSecureMode;
            config.Protocol = connection.Protocol;
        }

        public IEnumerable<IMailMessage> GenerateMessages(Email sender, Email recipient, int total = 5)
        {
            for (int i = 1; i <= total; i++)
            {
                var message = new MailMessage
                {
                    To = { recipient },
                    From = sender,
                    Subject = $"email-{NextSeed}",
                    HtmlBody = $"HTML body-{NextSeed}"
                };
                AddMessage(message);

                yield return message;
            }
        }

        private static void AddMessage(IMailMessage mailMessage)
        {
            var message = InitComType<Message>();
            message.From = mailMessage.From;
            message.FromAddress = mailMessage.From.Address;
            if (!string.IsNullOrWhiteSpace(mailMessage.HtmlBody))
                message.HTMLBody = mailMessage.HtmlBody;
            if (!string.IsNullOrWhiteSpace(mailMessage.PlainBody))
                message.Body = mailMessage.PlainBody;
            message.Subject = mailMessage.Subject;
            foreach (var address in mailMessage.To)
                message.AddRecipient(
                    !string.IsNullOrWhiteSpace(address.DisplayName) ? address.DisplayName : address.Address,
                    address.Address);
            message.Save();
        }

        public void CleanUp()
        {
            foreach (var emails in _addedAccounts.GroupBy(account => account.Domain))
            {
                string domain = emails.Key;
                var domainManager = FindAndEnsureDomainAvailable(domain);
                foreach (var email in emails)
                {
                    string address = email.ToString();
                    var account = TryOrNull(() => domainManager.Accounts.ItemByAddress[address]);
                    if (account == null) continue;

                    account.DeleteMessages();
                    account.Delete();
                }

                domainManager.Delete();
            }
        }

        private hMailServer.Domain FindAndEnsureDomainAvailable(string domain)
        {
            var domainManager = TryOrNull(() => _mailServer.Domains.ItemByName[domain]);
            if (domainManager != null) return domainManager;

            domainManager = _mailServer.Domains.Add();
            domainManager.Name = domain;
            domainManager.Active = true;
            domainManager.Save();
            return domainManager;
        }

        private static TOutput InitComType<TOutput>()
        {
            string fullName = typeof(TOutput).FullName ??
                              throw new InvalidOperationException($"Unknown type: {typeof(TOutput).Name}");
            var comType = Type.GetTypeFromProgID(fullName) ??
                          throw new InvalidOperationException($"Failed to get COM type: {fullName}");
            object instance = Activator.CreateInstance(comType) ??
                              throw new InvalidOperationException($"Unable to instantiate COM type: {fullName}");
            return (TOutput)instance;
        }

        private static TOutput? TryOrNull<TOutput>(Func<TOutput> factory)
            where TOutput : class
        {
            try
            {
                return factory();
            }
            catch (COMException)
            {
                return null;
            }
        }
    }
}

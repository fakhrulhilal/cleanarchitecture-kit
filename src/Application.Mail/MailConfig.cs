namespace FM.Application.Mail
{
    public class MailConnectionConfig
    {
        public int Port { get; set; }
        public bool UseSecureMode { get; set; }
        public MailProtocol Protocol { get; set; }
    }

    public class EmailConfig
    {
        public class OutgoingConfig : MailConnectionConfig
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string ServerAddress { get; set; } = string.Empty;
        }

        public class IncomingConfig : OutgoingConfig
        {
            public string MailBox { get; set; } = "INBOX";
        }
    }

    public enum MailProtocol
    {
        Smtp = 1,
        Imap,
        Pop3
    }
}

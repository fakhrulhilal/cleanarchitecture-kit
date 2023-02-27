using DevKit.Domain.ValueObjects;

namespace DevKit.Application.Ports;

public interface IMailMessage
{
    public Email From { get; }
    public List<Email> To { get; }
    public List<Email> Cc { get; }
    public string Subject { get; }
    public string HtmlBody { get; }
    public string PlainBody { get; }
}

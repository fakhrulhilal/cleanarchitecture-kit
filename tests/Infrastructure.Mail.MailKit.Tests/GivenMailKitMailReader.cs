using System.Collections;
using DevKit.Application.Models;
using DevKit.Application.Ports;
using DevKit.Infrastructure.Mail.MailKit.Tests.HMailServer;

namespace DevKit.Infrastructure.Mail.MailKit.Tests;

using static Testing;

[TestFixture]
[Parallelizable(ParallelScope.Fixtures)]
public class GivenMailKitMailReader
{
    [OneTimeSetUp]
    public void SingleSetup() => _server.Connect();

    [TearDown]
    public void Setup() => _server.CleanUp();

    private readonly Server _server = GetService<Server>();
    private readonly IComparer _collectionComparer = ClassComparer.PublicProperty.IgnoreLineEnding().Build();

    [Test]
    public async Task WhenValidConnectionUsingImapThenItShouldWork() {
        var recipient = _server.CreateUniqueAccount($"{nameof(GivenMailKitMailReader)}.lab");
        _server.PopulateConnection(recipient, MailProtocol.Imap);
        var sender = _server.CreateUniqueAccount($"{nameof(GivenMailKitMailReader)}.lab");
        var messages = _server.GenerateMessages(sender.Username, recipient.Username).ToArray();
        var sut = GetService<IMailReader>(reader => reader.SupportedProtocol == recipient.Protocol);

        var result = await sut.GetMessagesAsync(recipient).ToArrayAsync();

        Assert.That(result, Is.EquivalentTo(messages).Using(_collectionComparer));
    }

    [Test]
    public async Task WhenValidConnectionUsingPop3ThenItShouldWork() {
        var recipient = _server.CreateUniqueAccount($"{nameof(GivenMailKitMailReader)}.lab");
        _server.PopulateConnection(recipient, MailProtocol.Pop3);
        var sender = _server.CreateUniqueAccount($"{nameof(GivenMailKitMailReader)}.lab");
        var messages = _server.GenerateMessages(sender.Username, recipient.Username).ToArray();
        var sut = GetService<IMailReader>(reader => reader.SupportedProtocol == recipient.Protocol);

        var result = await sut.GetMessagesAsync(recipient).ToArrayAsync();

        Assert.That(result, Is.EquivalentTo(messages).Using(_collectionComparer));
    }
}

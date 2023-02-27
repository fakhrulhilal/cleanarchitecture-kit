using static DevKit.Application.Validations.IpAddressValidator;

namespace DevKit.Application.Tests.Validation;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public class GivenIpAddressValidator
{
    [Test]
    public void WhenEmptyThenItIsNotValidIpv4() => Assert.That(IsValid("  "), Is.False);

    [Test]
    public void WhenSeparatedByDotAndContainsLessThanFourSegmentThenItIsNotValidIpv4() =>
        Assert.That(IsValid("127.0.0"), Is.False);

    [Test]
    public void WhenSeparatedByDotAndFirstSegmentIsZeroThenItIsNotValidIpv4() =>
        Assert.That(IsValid("0.1.1.1"), Is.False);

    [Test]
    public void WhenSeparatedByDotAndLastSegmentIsZeroThenItIsNotValidIpv4() =>
        Assert.That(IsValid("1.0.0.0"), Is.False);

    [Test]
    public void WhenSeparatedByDotAndOneOfSegmentIsGreaterThan255ThenItIsNotValidIpv4() =>
        Assert.That(IsValid("123.256.0.120"), Is.False);

    [Test]
    public void
        WhenSeparatedByDotAndContainsFourSegmentSeparatedByDotHavingLessThan255ForEachThenItWillBeValidIpv4() =>
        Assert.That(IsValid("127.0.0.1"), Is.True);

    [Test]
    public void WhenSeparatedBySemiColonHavingMaxEightSegmentAndLessThanFfffThenItWillBeValidIpv6() =>
        Assert.That(IsValid("2001:db8:ff:ab:0:ff00:42:8329"), Is.True);

    [Test]
    public void WhenSeparatedBySemiColonAndOneOfItsSegmentIsGreaterThanFfffThenItIsNotValidIpv6() =>
        Assert.That(IsValid("2001:db8:ab:23:78:fffff:42:8329"), Is.False);

    [Test]
    public void WhenSeparatedBySemiColonAndHasMoreThanEightSegmentsThenItIsNotValidIpv6() =>
        Assert.That(IsValid("fe80:ab:1c17:5e3d:caea:b6a7:ff:67:13"), Is.False);

    [Test]
    public void WhenConsecutiveSemiColonRepeatedMoreThanTwiceThenItIsNotValidIpv6() =>
        Assert.That(IsValid(":::1"), Is.False);

    [Test]
    public void WhenHavingLessThanEightSegmentsButNoRepetitiveSemiColonThenItIsNotValidIpv6() =>
        Assert.That(IsValid("1:3"), Is.False);

    [Test]
    public void WhenHavingLessThanEightSegmentsButHasRepetitiveSemiColonThenItIsValidIpv6() =>
        Assert.That(IsValid("1::2"), Is.True);

    [Test]
    public void WhenSeparatedAndStartedBySemiColonThenItIsValidIpv6() => Assert.That(IsValid("::1"), Is.True);
}

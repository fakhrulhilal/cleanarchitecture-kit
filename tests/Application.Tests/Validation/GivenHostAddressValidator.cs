using DevKit.Application.Validations;
using FluentValidation;

namespace DevKit.Application.Tests.Validation;

[TestFixture]
public class GivenHostAddressValidator
{
    [Test]
    public void WhenValidIpAddressPassedThenItShouldBeValid() {
        var sut = new HostAddress.Validator();

        var result = sut.Validate(new HostAddress("::1"));

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void WhenValidDomainAddressPassedThenItShouldBeValid() {
        var sut = new HostAddress.Validator();

        var result = sut.Validate(new HostAddress("localhost"));

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void WhenItIsNotIpOrDomainAddressThenItShouldBeInvalid() {
        var sut = new HostAddress.Validator();

        var result = sut.Validate(new HostAddress("::1-"));

        Assert.That(result.IsValid, Is.False);
        var error = result.Errors[0];
        Assert.That(error.PropertyName, Is.EqualTo(nameof(HostAddress.Name)));
        Assert.That(error.ErrorMessage,
            Is.EqualTo($"'{nameof(HostAddress.Name)}' is not valid IP or domain address"));
    }

    private record HostAddress(string Name)
    {
        public class Validator : AbstractValidator<HostAddress>
        {
            public Validator() => RuleFor(p => p.Name).HostAddress();
        }
    }
}

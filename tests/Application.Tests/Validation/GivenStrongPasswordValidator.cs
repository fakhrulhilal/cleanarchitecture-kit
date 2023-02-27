using DevKit.Application.Validations;
using FluentValidation;

namespace DevKit.Application.Tests.Validation;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public class GivenStrongPasswordValidator
{
    [Test]
    public void WhenMinimumNotSetAndGivenValueIsLessThenEightThenItIsNotValid() {
        var sut = new Validator();

        var result = sut.Validate(new Account("P@ssw0r"));

        Assert.That(result.IsValid, Is.False);
        var error = result.Errors[0];
        Assert.That(error.PropertyName, Is.EqualTo(nameof(Account.Password)));
    }

    [Test]
    public void WhenMinimumSetAndGivenValueIsLessThanThatThenItIsNotValid() {
        var sut = new Validator(9);

        var result = sut.Validate(new Account("P@ssw0rd"));

        Assert.That(result.IsValid, Is.False);
        var error = result.Errors[0];
        Assert.That(error.PropertyName, Is.EqualTo(nameof(Account.Password)));
    }

    [Test]
    public void WhenDoesNotContainUpperCaseThenItIsNotValid() {
        var sut = new Validator();

        var result = sut.Validate(new Account("p@ssw0rd"));

        Assert.That(result.IsValid, Is.False);
        var error = result.Errors[0];
        Assert.That(error.PropertyName, Is.EqualTo(nameof(Account.Password)));
        Assert.That(error.ErrorMessage,
            Is.EqualTo($"'{nameof(Account.Password)}' must contain uppercase character."));
    }

    [Test]
    public void WhenDoesNotContainLowerCaseThenItIsNotValid() {
        var sut = new Validator();

        var result = sut.Validate(new Account("P@SSW0RD"));

        Assert.That(result.IsValid, Is.False);
        var error = result.Errors[0];
        Assert.That(error.PropertyName, Is.EqualTo(nameof(Account.Password)));
        Assert.That(error.ErrorMessage,
            Is.EqualTo($"'{nameof(Account.Password)}' must contain lowercase character."));
    }

    [Test]
    public void WhenDoesNotContainSpecialCharacterThenItIsNotValid() {
        var sut = new Validator();

        var result = sut.Validate(new Account("Passw0rd"));

        Assert.That(result.IsValid, Is.False);
        var error = result.Errors[0];
        Assert.That(error.PropertyName, Is.EqualTo(nameof(Account.Password)));
        Assert.That(error.ErrorMessage,
            Is.EqualTo($"'{nameof(Account.Password)}' must contain special character."));
    }

    [Test]
    public void WhenDoesNotContainNumberThenItIsNotValid() {
        var sut = new Validator();

        var result = sut.Validate(new Account("P@ssword"));

        Assert.That(result.IsValid, Is.False);
        var error = result.Errors[0];
        Assert.That(error.PropertyName, Is.EqualTo(nameof(Account.Password)));
        Assert.That(error.ErrorMessage,
            Is.EqualTo($"'{nameof(Account.Password)}' must contain number."));
    }

    [Test]
    public void
        WhenContainsUpperCaseAndLowerCaseAndNumberAndSpecialCharacterAndLongerOrEqualThanMinimumThenItIsValid() {
        var sut = new Validator();

        var result = sut.Validate(new Account("P@ssw0rd"));

        Assert.That(result.IsValid, Is.True);
    }

    internal record Account(string Password);

    private class Validator : AbstractValidator<Account>
    {
        public Validator(int? minimum = null) => RuleFor(p => p.Password).StrongPassword(minimum);
    }
}

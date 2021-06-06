using FluentValidation;
using FM.Application.Validations;
using NUnit.Framework;

namespace FM.Application.Tests.Common.Validation
{
    [TestFixture]
    [Parallelizable(ParallelScope.Children)]
    public class GivenPortNumberValidator
    {
        [Test]
        public void WhenLessThanZeroThenItIsInvalid()
        {
            var sut = new Validator();

            var result = sut.Validate(new Data(-1));

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo(nameof(Data.Port)));
        }

        [Test]
        public void WhenZeroThenItIsValid()
        {
            var sut = new Validator();

            var result = sut.Validate(new Data(0));

            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void WhenGreaterThan65535ThenItIsValid()
        {
            var sut = new Validator();

            var result = sut.Validate(new Data(65536));

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo(nameof(Data.Port)));
        }

        [Test]
        public void WhenBetweenZeroAnd65535ThenItIsValid()
        {
            var sut = new Validator();

            var result = sut.Validate(new Data(1));

            Assert.That(result.IsValid, Is.True);
        }

        internal record Data(int Port);
        private class Validator : AbstractValidator<Data>
        {
            public Validator()
            {
                RuleFor(p => p.Port).PortNumber();
            }
        }
    }
}

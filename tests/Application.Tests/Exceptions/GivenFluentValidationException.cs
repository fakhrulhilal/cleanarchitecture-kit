using System.Collections.Generic;
using FluentValidation.Results;
using FM.Domain.Exception;
using NUnit.Framework;
using FluentValidationException = FM.Application.Exceptions.FluentValidationException;

namespace FM.Application.Tests.Exceptions
{
    [TestFixture]
    public class GivenFluentValidationException
    {
        [Test]
        public void WhenBuiltFromDefaultConstructorThenErrorDictionaryWillBeEmpty()
        {
            var sut = new ValidationException().Errors;
            Assert.That(sut.Keys, Is.Empty);
        }

        [Test]
        public void WhenContainingSingleValidationFailureThenErrorDictionaryWillBeSingle()
        {
            var failures = new List<ValidationFailure>
            {
                new("Age", "must be over 18"),
            };

            var sut = new FluentValidationException(failures).Errors;

            Assert.That(sut.Keys, Is.EquivalentTo(new[] { "Age" }));
            Assert.That(sut["Age"], Is.EquivalentTo(new[] { "must be over 18" }));
        }

        [Test]
        public void WhenContainingMultipleValidationFailureThenItWillBeGroupedByPropertyName()
        {
            var failures = new List<ValidationFailure>
            {
                new("Age", "must be 18 or older"),
                new("Age", "must be 25 or younger"),
                new("Password", "must contain at least 8 characters"),
                new("Password", "must contain a digit"),
                new("Password", "must contain upper case letter"),
                new("Password", "must contain lower case letter"),
            };

            var sut = new FluentValidationException(failures).Errors;

            Assert.That(sut.Keys, Is.EquivalentTo(new[] { "Age", "Password" }));
            Assert.That(sut["Age"], Is.EquivalentTo(new[]
            {
                "must be 25 or younger",
                "must be 18 or older",
            }));
            Assert.That(sut["Password"], Is.EquivalentTo(new[]
            {
                "must contain lower case letter",
                "must contain upper case letter",
                "must contain at least 8 characters",
                "must contain a digit",
            }));
        }
    }
}

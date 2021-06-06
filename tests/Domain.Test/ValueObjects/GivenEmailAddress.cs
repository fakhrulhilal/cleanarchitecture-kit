using System;
using FM.Domain.ValueObjects;
using NUnit.Framework;

namespace FM.Domain.Tests.ValueObjects
{
    [TestFixture]
    [Parallelizable(ParallelScope.Children)]
    public class GivenEmailAddress
    {
        [Test]
        public void WhenCreatingFromZeroThenAllPropertiesAreEmpty()
        {
            var result = Email.Empty;

            Assert.That(result.Address, Is.Empty);
            Assert.That(result.Username, Is.Empty);
            Assert.That(result.Domain, Is.Empty);
        }

        [Test]
        public void WhenCreatingAndDisplayNameIsEmptyThenItWillThrowArgumentException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => Email.Create(" ", "me@localhost"));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo("displayName"));
        }

        [Test]
        public void WhenNullOrEmptyThenItWillThrowArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => Email.Parse(string.Empty));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.Not.Null.And.EqualTo("word"));
        }

        [Test]
        public void WhenSeparatedByAtCharacterOnlyThenItWillBeAddressWithoutDisplayName()
        {
            string address = "me@localhost";

            var result = Email.Parse(address);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Address, Is.EqualTo(address));
            Assert.That(result.DisplayName, Is.Not.Null.And.Empty);
            Assert.That(result.Username, Is.EqualTo("me"));
            Assert.That(result.Domain, Is.EqualTo("localhost"));
        }

        [Test]
        public void
            WhenDisplayNameSurroundedByDoubleQuoteAndAddressSurroundedByLessAndGreaterCharThenItWillBeParsedForBoth()
        {
            string address = "me@localhost";
            string displayName = "Full Name";

            var result = Email.Parse($"\"{displayName}\" <{address}>");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Address, Is.EqualTo(address));
            Assert.That(result.DisplayName, Is.EqualTo(displayName));
            Assert.That(result.Username, Is.EqualTo("me"));
            Assert.That(result.Domain, Is.EqualTo("localhost"));
        }

        [Test]
        public void WhenComparedByOtherHavingSameAddressThenItWillEqual()
        {
            string address = "me@localhost";
            var emailAddress1 = Email.Parse(address);
            var emailAddress2 = Email.Parse(address);

            Assert.That(emailAddress1 == emailAddress2, Is.True);
            Assert.That(emailAddress1.Equals(emailAddress2), Is.True);
        }

        [Test]
        public void WhenComparedByOtherHavingSameAddressButDifferentDisplayNameThenItWillBeEqual()
        {
            string address = "me@localhost";
            var emailAddress1 = Email.Parse($"\"My Full Name\" <{address}>");
            var emailAddress2 = Email.Parse(address);

            Assert.That(emailAddress1 == emailAddress2, Is.True);
            Assert.That(emailAddress1.Equals(emailAddress2), Is.True);
        }

        [Test]
        public void WhenSerializedIntoStringHavingAddressOnlyThenItWillReturnAsIs()
        {
            string address = "me@localhost";
            var emailAddress = Email.Parse(address);

            string result = emailAddress;

            Assert.That(result, Is.EqualTo(address));
        }

        [Test]
        public void WhenSerializedIntoStringContainingDisplayNameAndAddressThenDisplayNameWillBeSurroundedByDoubleQuoteAndAddressWillBeSurroundedByLessAndGreaterChar()
        {
            string expected = "\"My Full Name\" <me@localhost>";
            var emailAddress = Email.Parse(expected);

            string result = emailAddress;

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}

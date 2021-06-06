using NUnit.Framework;

namespace FM.Application.Tests.Common.Validation
{
    using static Validations.DomainAddressValidator;

    [TestFixture]
    [Parallelizable(ParallelScope.Children)]
    public class GivenDomainAddressValidator
    {
        [Test]
        public void WhenAllAreAlphaNumericThenItIsValid() => Assert.That(IsValid("localhost123"), Is.True);

        [Test]
        public void WhenNamesAreSeparatedByDotThenItIsValid() => Assert.That(IsValid("name.domain"), Is.True);

        [Test]
        public void WhenStartedByDotThenItIsNotValid() => Assert.That(IsValid(".domain"), Is.False);

        [Test]
        public void WhenEndedByDotThenItIsNotValid() => Assert.That(IsValid("name."), Is.False);

        [Test]
        public void WhenContainsDashOrUnderscoreThenItIsValid() => Assert.That(IsValid("another-domain_name"), Is.True);

        [Test]
        public void WhenStartedByDashCharacterThenItIsNotValid() => Assert.That(IsValid("-domain.name"), Is.False);

        [Test]
        public void WhenSomePartStartedByDashThenItIsNotValid() => Assert.That(IsValid("www.-domain.com"), Is.False);

        [Test]
        public void WhenSomePartEndedByDashThenItIsNotValid() => Assert.That(IsValid("www.domain-.com"), Is.False);

        [Test]
        public void WhenEndedByDashThenItIsNotValid() => Assert.That(IsValid("domain-"), Is.False);

        [Test]
        public void WhenContainsSpecialCharacterOtherThanDashOrUnderscoreThenItIsNotValid() =>
            Assert.That(IsValid("in%valid.domain"), Is.False);
    }
}

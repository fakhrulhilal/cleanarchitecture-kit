using System.Text.RegularExpressions;

namespace FM.Application.Validations
{
    /// <summary>
    /// Validate against valid domain name address.
    /// Reference: https://en.wikipedia.org/wiki/Domain_name
    /// </summary>
    internal static class DomainAddressValidator
    {
        private const string NamePattern = @"\w[\w\-]*(?<!\W)";
        private static readonly Regex _domainPattern = new($@"^({NamePattern}\.)*{NamePattern}$");

        internal static bool IsValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.Trim();
            return _domainPattern.IsMatch(value);
        }
    }
}

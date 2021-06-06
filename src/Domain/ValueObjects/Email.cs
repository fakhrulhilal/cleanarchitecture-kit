using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FM.Domain.Common;

namespace FM.Domain.ValueObjects
{
    public class Email : ValueObject
    {
        private static readonly string _addressPattern = @"(?<address>(?<username>[\w\.\-,]+)@(?<domain>([\w\-\.]+)*[\w+]))";
        private static readonly Regex _addressOnlyPattern = new($@"^{_addressPattern}$");
        private static readonly Regex _fullEmailPattern = new($@"^""(?<display_name>[\w\s\.\-,@]+)""\s+<{_addressPattern}>$");

        public string DisplayName { get; } = string.Empty;
        public string Address { get; }
        public string Username { get; }
        public string Domain { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(Address);

        private Email()
        {
            Address = Username = Domain = string.Empty;
        }

        private Email(string address, string username, string domain)
        {
            if (string.IsNullOrEmpty(address)) throw new ArgumentNullException(nameof(address));
            Address = address;
            Username = username;
            Domain = domain;
        }

        private Email(string address, string displayName, string username, string domain) : this(address, username, domain)
        {
            DisplayName = displayName;
        }

        public static Email Empty => new();

        public static Email Parse(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) throw new ArgumentNullException(nameof(word));
            var match = _fullEmailPattern.Match(word);
            if (match.Success)
                return new Email(match.Groups["address"].Value, match.Groups["display_name"].Value,
                    match.Groups["username"].Value, match.Groups["domain"].Value);

            match = _addressOnlyPattern.Match(word);
            if (match.Success)
                return new Email(match.Groups["address"].Value, match.Groups["username"].Value,
                    match.Groups["domain"].Value);

            throw new ArgumentException($"Invalid email address format: {word}", nameof(word));
        }

        public static Email Create(string displayName, string address)
        {
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentNullException(nameof(displayName));
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentNullException(nameof(address));

            var match = _addressOnlyPattern.Match(address);
            if (!match.Success)
                throw new ArgumentException($"Invalid email address format: {address}", nameof(address));
            return new Email(address, displayName, match.Groups["username"].Value, match.Groups["domain"].Value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Address;
        }

        public static implicit operator string(Email email) => email.ToString();
        public static implicit operator Email(string word) => Parse(word);

        public override string ToString() =>
            string.IsNullOrEmpty(DisplayName) ? Address : $"\"{DisplayName}\" <{Address}>";
    }
}

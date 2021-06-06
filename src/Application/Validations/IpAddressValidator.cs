using System;
using System.Text.RegularExpressions;

namespace FM.Application.Validations
{
    internal static class IpAddressValidator
    {
        private static readonly Regex _repetitiveSemiColon = new(@"(?<semicolon>\:{2,})");
        internal static bool IsValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.Trim();
            return IsValidIpv4(value) || IsValidIpv6(value);
        }

        private static bool IsValidIpv4(string value)
        {
            string[] segments = value.Split('.');
            const int max = 255, totalSegment = 4;
            if (segments.Length != totalSegment) return false;

            int sequence = 1;
            foreach (string segment in segments)
            {
                if (!int.TryParse(segment, out int number)) return false;
                if ((sequence == 1 || sequence == totalSegment) && number == 0) return false;
                if (number > max) return false;
                sequence++;
            }

            return true;
        }

        private static bool IsValidIpv6(string value)
        {
            int max = Convert.ToInt32("ffff", 16), totalSegment = 8;
            string[] segments = value.Split(':');
            var match = _repetitiveSemiColon.Match(value);
            bool hasRepetitiveSemiColon = match.Success && match.Groups["semicolon"].Value.Length == 2;
            if (segments.Length > totalSegment || (segments.Length < totalSegment && !hasRepetitiveSemiColon))
                return false;
            foreach (string segment in segments)
            {
                try
                {
                    if (string.IsNullOrEmpty(segment)) continue;
                    int hex = Convert.ToInt32(segment, 16);
                    if (hex > max) return false;
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

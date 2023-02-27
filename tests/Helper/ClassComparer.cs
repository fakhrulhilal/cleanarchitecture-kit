using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DevKit;

public static class ClassComparer
{
    public static PublicPropertyComparerBuilder PublicProperty => new();

    public class PublicPropertyComparerBuilder
    {
        private readonly PublicPropertyComparer _inner = new();

        public static implicit operator PublicPropertyComparer(
            PublicPropertyComparerBuilder publicPropertyComparerBuilder) =>
            publicPropertyComparerBuilder._inner;

        public IComparer Build() => _inner;

        public PublicPropertyComparerBuilder UseEnumAsString() {
            _inner.UseEnumAsString = true;
            return this;
        }

        public PublicPropertyComparerBuilder IgnoringThese(params string[] properties) {
            if (properties.Length == 0) throw new ArgumentNullException(nameof(properties));
            _inner.IgnoredProperties = properties;
            return this;
        }

        public PublicPropertyComparerBuilder IgnoreLineEnding() {
            _inner.IgnoreLineEnding = true;
            return this;
        }
    }

    public class PublicPropertyComparer : IComparer
    {
        private readonly Regex _lineEndingRegex = new(@"(\r\n|\r|\n)", RegexOptions.Multiline);
        internal string[] IgnoredProperties { private get; set; } = Array.Empty<string>();
        internal bool UseEnumAsString { private get; set; }
        internal bool IgnoreLineEnding { private get; set; }

        public int Compare(object? source, object? target) {
            if (source == null || target == null) return source == target ? 0 : 1;

            var generatedAttribute = typeof(CompilerGeneratedAttribute);
            var properties =
                source.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && (p.Name != "EqualityContract" ||
                                              (!p.GetMethod?.CustomAttributes.Any(a =>
                                                  a.AttributeType == generatedAttribute) ?? false)))
                    .ToArray();

            var targetType = target.GetType();
            if (!properties.Any()) return source == target ? 0 : 1;

            var ignoreList = new List<string>(IgnoredProperties);
            foreach (var prop in properties) {
                if (ignoreList.Contains(prop.Name)) continue;

                object? sourceValue = prop.GetValue(source, null);
                object? targetValue = targetType.GetProperty(prop.Name)?.GetValue(target, null);
                if (prop.PropertyType.IsEnum && UseEnumAsString) {
                    sourceValue = sourceValue?.ToString();
                    targetValue = targetValue?.ToString();
                }
                else if (sourceValue is string sourceString && targetValue is string targetString &&
                         IgnoreLineEnding) {
                    sourceValue = _lineEndingRegex.Replace(sourceString, string.Empty);
                    targetValue = _lineEndingRegex.Replace(targetString, string.Empty);
                }

                if (sourceValue is IEnumerable sourceCollection &&
                    targetValue is IEnumerable targetCollection) {
                    var targetEnumerator = targetCollection.GetEnumerator();
                    foreach (object sourceItem in sourceCollection) {
                        if (!targetEnumerator.MoveNext()) return 1;
                        if (IsNotEqual(sourceItem, targetEnumerator.Current)) return 1;
                    }

                    if (targetEnumerator.MoveNext()) return 1;
                }
                else if (IsNotEqual(sourceValue, targetValue)) return 1;
            }

            return 0;
        }

        private static bool IsNotEqual(object? sourceValue, object? targetValue) =>
            sourceValue != targetValue && (sourceValue == null || !sourceValue.Equals(targetValue));
    }
}

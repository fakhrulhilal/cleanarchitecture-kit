using System.Text.RegularExpressions;

namespace DevKit.Domain.Models;

public static class TypeExtensions
{
    private static readonly Regex _configName = new(@"(Config)$", RegexOptions.IgnoreCase);

    /// <summary>
    ///     Get class name along with parent class (for nested class)
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetClassName(this Type type) {
        if (!type.IsNested) return type.Name;
        bool isStaticClass = type.IsAbstract && type.IsSealed;
        if (isStaticClass) return type.Name;

        return type.DeclaringType != null ? $"{GetClassName(type.DeclaringType)}{type.Name}" : type.Name;
    }

    /// <summary>
    ///     Build default config key path.
    ///     It will build until root class while removing 'Config' suffix from class name.
    ///     For every nested class, it will be separated by semicolon (:).
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetConfigName(this Type type) {
        string key = _configName.Replace(type.Name, string.Empty);
        if (!type.IsNested) return key;
        bool isStaticClass = type.IsAbstract && type.IsSealed;
        if (isStaticClass || type.DeclaringType == null) return key;

        return $"{GetConfigName(type.DeclaringType)}:{key}";
    }

    public static bool HasParameterLessConstructor(this Type type) =>
        type.GetConstructors()
            .OrderBy(x => x.GetParameters().Length - x.GetParameters().Count(p => p.IsOptional))
            .FirstOrDefault() != null;
}

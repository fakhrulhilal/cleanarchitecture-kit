using FluentValidation;

namespace DevKit.Application.Validations;

public static class ValidationExtensions
{
    private const int DefaultMinimumPasswordLength = 8;

    /// <summary>
    ///     Rule for strong password: contains uppercase, lowercase, number, special character,
    ///     and has <paramref name="minimumLength" /> length
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rule"></param>
    /// <param name="minimumLength"></param>
    /// <returns></returns>
    public static IRuleBuilder<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule,
        int? minimumLength = null) => rule
        .NotEmpty()
        .MinimumLength(minimumLength ?? DefaultMinimumPasswordLength)
        .Matches("[A-Z]").WithMessage("'{PropertyName}' must contain uppercase character.")
        .Matches("[a-z]").WithMessage("'{PropertyName}' must contain lowercase character.")
        .Matches("[0-9]").WithMessage("'{PropertyName}' must contain number.")
        .Matches("[^a-zA-Z0-9]").WithMessage("'{PropertyName}' must contain special character.");

    /// <summary>
    ///     Network port number
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rule"></param>
    /// <returns></returns>
    public static IRuleBuilder<T, int> PortNumber<T>(this IRuleBuilder<T, int> rule) => rule
        .GreaterThanOrEqualTo(0).LessThan(65536);

    /// <summary>
    ///     Valid host address, either in form of IP (v4/v6) or domain hostname
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rule"></param>
    /// <returns></returns>
    public static IRuleBuilder<T, string> HostAddress<T>(this IRuleBuilder<T, string> rule) => rule
        .NotNull().NotEmpty()
        .Must(address => IpAddressValidator.IsValid(address) || DomainAddressValidator.IsValid(address))
        .WithMessage("'{PropertyName}' is not valid IP or domain address");
}

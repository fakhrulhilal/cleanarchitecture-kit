namespace DevKit.Application.Ports;

/// <summary>
///     Get current logging in user info
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    ///     User identifier
    /// </summary>
    string UserId { get; }
}

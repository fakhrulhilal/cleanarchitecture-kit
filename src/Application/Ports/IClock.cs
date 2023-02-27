namespace DevKit.Application.Ports;

/// <summary>
///     System clock
/// </summary>
public interface IClock
{
    /// <summary>
    ///     UTC now
    /// </summary>
    DateTime Now => DateTime.UtcNow;
}

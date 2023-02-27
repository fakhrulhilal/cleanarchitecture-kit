namespace DevKit.Domain.Models;

/// <summary>
///     Resiliency config
/// </summary>
public class RetryConfig
{
    /// <summary>
    ///     Maximum attempt
    /// </summary>
    public int Max { get; set; }

    /// <summary>
    ///     Delay between attempt
    /// </summary>
    public TimeSpan Delay { get; set; }

    public struct Default
    {
        /// <summary>
        ///     Default delay between attempt in seconds
        /// </summary>
        public const int Delay = 5;

        public const int Max = 5;
    }
}

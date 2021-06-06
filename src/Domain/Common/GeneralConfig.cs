namespace FM.Domain.Common
{
    public class GeneralConfig
    {
        /// <summary>
        /// Maximum time for long running task in ms, otherwise, it will be logged as warning
        /// </summary>
        public int MaxLongRunningTask { get; set; } = 500;
    }
}

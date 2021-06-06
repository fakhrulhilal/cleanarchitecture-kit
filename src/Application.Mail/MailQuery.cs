using System;

namespace FM.Application.Mail
{
    /// <summary>
    /// Filter for fetching mail message
    /// </summary>
    public class MailQuery
    {
        public int? Limit { get; set; }

        /// <summary>
        /// Fetch only unread message
        /// </summary>
        public bool? UnreadOnly { get; set; }

        /// <summary>
        /// Fetch only message received later than the date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Fetch only message received before than the date
        /// </summary>
        public DateTime? EndDate { get; set; }

        public bool AutoDelete { get; set; }
    }
}

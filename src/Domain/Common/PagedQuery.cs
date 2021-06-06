namespace FM.Domain.Common
{
    public abstract class PagedQuery
    {
        public const int DefaultPagingSize = 10;
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }
}

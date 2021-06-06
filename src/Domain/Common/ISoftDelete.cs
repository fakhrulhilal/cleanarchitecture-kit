namespace FM.Domain.Common
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}

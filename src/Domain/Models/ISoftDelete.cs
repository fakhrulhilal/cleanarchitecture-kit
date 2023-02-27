namespace DevKit.Domain.Models;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}

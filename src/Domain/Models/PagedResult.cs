namespace DevKit.Domain.Models;

/// <summary>
///     Paged items result
/// </summary>
/// <typeparam name="T"></typeparam>
public class PagedResult<T>
{
    /// <summary>
    ///     Instantiate new paged result
    /// </summary>
    /// <param name="items">Actual items</param>
    /// <param name="totalItems">Total un-paged items</param>
    /// <param name="pageNumber">Current page position</param>
    /// <param name="pageSize">Total items per page</param>
    public PagedResult(T[] items, int totalItems, int pageNumber, int pageSize) {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        TotalItems = totalItems;
        Items = items;
        HasPreviousPage = pageNumber > 1;
        HasNextPage = PageNumber < TotalPages;
    }

    /// <summary>
    ///     Actual items
    /// </summary>
    public T[] Items { get; }

    /// <summary>
    ///     Current page position, starting from 1
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    ///     Total pages
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    ///     Total un-paged items
    /// </summary>
    public int TotalItems { get; }

    /// <summary>
    ///     Determine whether previous page is available
    /// </summary>
    public bool HasPreviousPage { get; }

    /// <summary>
    ///     Determine whether next page is available
    /// </summary>
    public bool HasNextPage { get; }
}

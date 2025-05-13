using Microsoft.EntityFrameworkCore;

namespace MvcMovie.Helpers;

public class PaginatedList<T>
{
    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        PageSize = pageSize;
        Items = items;
    }
    public int PageIndex { get; private set; }
    public int PageSize { get; private set; }
    public bool PaginationEnabled { get; private set; }

    public int TotalPages { get; private set; }
    public List<T> Items { get; private set; }

    public bool HasPreviousPage => (PageIndex > 1);
    public bool HasNextPage => (PageIndex < TotalPages);



    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = source.Count();

        if(pageIndex < 1)
        {
            pageIndex = 1;
        }
        if (pageSize < 1)
        {
            pageSize = 10;
        }

        if(pageIndex > count / pageSize)
        {
            pageIndex = (int)Math.Ceiling(count / (double)pageSize);
        }
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}

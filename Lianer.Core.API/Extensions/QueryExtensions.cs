public static class QueryExtensions
{
    public static IQueryable<T> Paginate<T>
    (
        this IQueryable<T> query, int currentPage, int pageSize
    )
    {
        return query.Skip((currentPage-1)*pageSize).Take(pageSize);
    }


}

using Lianer.Core.API.Projection;

public static class QueryExtensions
{
    public static IQueryable<T> Paginate<T>
    (
        this IQueryable<T> query, int currentPage, int pageSize
    )
    {
        return query.Skip((currentPage-1)*pageSize).Take(pageSize);
    }

    public static IQueryable<Dto> ProjectTo<Entity, Dto>
    (this IQueryable<Entity> query)
    where Entity: class
    where Dto: class, IProjection<Entity,Dto>
    {
        return query.Select(Dto.Selector);
    }
    
}

public interface ICrud<TEntity>
{
    Task<TEntity> GetById(Guid Id, CancellationToken ct);
    Task<TEntity> Create(TEntity entity,CancellationToken ct);
    Task<TEntity> Update(TEntity entity,CancellationToken ct);
    Task Delete(TEntity entity,CancellationToken ct);

}
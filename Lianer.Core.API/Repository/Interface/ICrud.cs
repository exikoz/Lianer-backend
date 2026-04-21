public interface ICrud<TEntity>
{
    Task<TEntity> Create(TEntity entity,CancellationToken ct);
    Task<TEntity> Update(TEntity entity,CancellationToken ct);
    Task Delete(Guid Id,CancellationToken ct);
    Task<TEntity?> GetById(Guid id, CancellationToken ct);

}
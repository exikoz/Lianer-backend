using Lianer.Core.API.Data;
using Lianer.Core.API.Models;
using Microsoft.EntityFrameworkCore;

public abstract class ACrud<TEntity>(AppDbContext context) : ICrud<TEntity>
where TEntity : class

{

    protected readonly AppDbContext _context = context;
    protected readonly DbSet<TEntity> _set = context.Set<TEntity>();

    public virtual async Task<TEntity> Create(TEntity entity, CancellationToken ct)
    {
        await _set.AddAsync(entity,ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task Delete(Guid id, CancellationToken ct)
    {
        var entity = await _set.FindAsync([id], ct);
        _set.Remove(entity!);
        await _context.SaveChangesAsync(ct);
    }

    public virtual async Task<TEntity?> GetById(Guid id, CancellationToken ct = default)
    {
        return await _set.FindAsync([id], ct);
    }

    public virtual async Task<TEntity> Update(TEntity entity, CancellationToken ct)
    {
        _set.Update(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }
  
}

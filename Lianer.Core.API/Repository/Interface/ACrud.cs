using Lianer.Core.API.Data;
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

    public virtual async Task Delete(TEntity entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public virtual async Task<TEntity> GetById(Guid Id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public virtual async Task<TEntity> Update(TEntity entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}



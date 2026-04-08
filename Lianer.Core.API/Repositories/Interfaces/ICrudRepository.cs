
/// <summary>
/// 
/// </summary>
/// <typeparam name="Entity">Entity</typeparam>
public interface ICrudRepository<Entity>
{
    Task<Entity> GetById(Guid id);
    Task<List<Entity>> GetAllById(Guid id);
    Task<List<Entity>> GetAllByQuery(string query);
    Task<Entity> Create(Entity request);
    Task<Entity> Update(Entity request);
    Task<bool> Delete(Guid id);

}
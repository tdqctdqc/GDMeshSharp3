
public interface ICreateKey : IWriteKey
{
    void Remove(Entity e);
    void Create<TEntity>(TEntity t) where TEntity : Entity;
    
}

public interface ICreateKey : IWriteKey
{
    void Create<TEntity>(TEntity t) where TEntity : Entity;
}

public class LogicWriteKey : StrongWriteKey, ICreateWriteKey
{
    public LogicResults Results { get; private set; }
    public LogicWriteKey(Data data, LogicResults results, ISession session) : base(data, session)
    {
        Results = results;
    }

    public void Create<TEntity>(TEntity t) where TEntity : Entity
    {
        var update = EntityCreationUpdate.Create(t, this);
        Results.Messages.Add(update);
    }

    public void Remove<TEntity>(TEntity t) where TEntity : Entity
    {
        var update = EntityDeletionUpdate.Create(t.Id, this);
        Results.Messages.Add(update);
    }
}
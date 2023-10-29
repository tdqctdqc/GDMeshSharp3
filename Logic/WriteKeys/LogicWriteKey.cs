
using System;

public class LogicWriteKey : StrongWriteKey, ICreateWriteKey
{
    private Action<Message> _sendMessage;
    public LogicWriteKey(Action<Message> sendMessage, Data data) : base(data)
    {
        _sendMessage = sendMessage;
    }

    public void Create<TEntity>(TEntity t) where TEntity : Entity
    {
        var update = EntityCreationUpdate<TEntity>.Create(t, this);
        _sendMessage(update);
    }

    public void Remove<TEntity>(TEntity t) where TEntity : Entity
    {
        var update = EntityDeletionUpdate.Create(t.Id, this);
    }
}
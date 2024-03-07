
using System;

public class LogicWriteKey : StrongWriteKey, ICreateWriteKey
{
    private Action<Message> _sendMessage;
    public LogicWriteKey(Action<Message> sendMessage, ISession session) : base(session)
    {
        _sendMessage = sendMessage;
    }

    public void Create<TEntity>(TEntity t) where TEntity : Entity
    {
        var update = EntityCreationUpdate<TEntity>.Create(t, this);
        _sendMessage(update);
    }

    public void SendMessage(Message m)
    {
        _sendMessage(m);
    }
}
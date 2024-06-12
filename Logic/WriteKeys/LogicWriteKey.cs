
using System;

public class LogicWriteKey : StrongWriteKey, ICreateWriteKey
{
    private Action<Message> _sendMessage;
    private Action<Procedure, Guid> _sendMessageToClient;
    public LogicWriteKey(Action<Message> sendMessage, 
        Action<Procedure, Guid> sendMessageToClient,
        ISession session) : base(session)
    {
        _sendMessage = sendMessage;
        _sendMessageToClient = sendMessageToClient;
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
    
    public void SendMessageToClient(Procedure p, Guid client)
    {
        if(client == Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid)
        {
            p.Enact(new ProcedureWriteKey(Session));
        }
        else
        {
            _sendMessageToClient(p, client);
        }
    }
}
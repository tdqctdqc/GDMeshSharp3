
using System;

public class LogicKey : Key, ICreateKey
{
    private HostLogic _logic;
    private HostServer _server;
    public LogicKey(HostLogic logic, 
        HostServer server,
        ISession session) : base(session)
    {
        _logic = logic;
        _server = server;
    }

    public void Remove(Entity e)
    {
        var proc = EntityDeletionUpdate.Create(e.Id, this);
        SendMessage(proc);
    }

    public void Create<TEntity>(TEntity t) where TEntity : Entity
    {
        var update = EntityCreationUpdate<TEntity>.Create(t, this);
        SendMessage(update);
    }

    public void SendMessage(Message m)
    {
        _logic.HandleMessage(m);
    }
    
    public void SendMessageToClient(Procedure p, Guid client)
    {
        if(client == Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid)
        {
            p.Enact(new ProcedureKey(Session));
        }
        else
        {
            _server.SendMessageToClient(p, client);
        }
    }

    public override bool HasRemotes()
    {
        throw new NotImplementedException();
    }
}
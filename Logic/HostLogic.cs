using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class HostLogic : ILogic
{
    public ConcurrentQueue<Command> CommandQueue { get; }
    private StateMachine _stateMachine;
    private TurnState _start, _middle, _end;
    public OrderHolder OrderHolder { get; private set; }
    private HostServer _server; 
    private HostWriteKey _hKey;
    public ProcedureWriteKey PKey { get; private set; }
    private Data _data;
    private static object _lock = new object();
    public HostLogic(Data data)
    {
        CommandQueue = new ConcurrentQueue<Command>();
        data.Requests.QueueCommand.Subscribe(CommandQueue.Enqueue);
        var syncKey = new LogicWriteKey(HandleMessage, data);
        var concKey = new LogicWriteKey(m =>
        {
            lock (_lock)
            {
                HandleMessage(m);
            }
        }, data);
        OrderHolder = new OrderHolder(concKey);

        _start = new TurnStartState(syncKey, OrderHolder);
        _middle = new TurnMiddleState(concKey, OrderHolder);
        _end = new TurnEndState(syncKey, OrderHolder);
        _start.SetNextState(_middle);
        _middle.SetNextState(_end);
        _end.SetNextState(_start);
    }
    public void SetDependencies(HostServer server, GameSession session, Data data)
    {
        _data = data;
        _server = server;
        _hKey = new HostWriteKey(this, data);
        PKey = new ProcedureWriteKey(data);
    }
    public void Process(float delta)
    {
        DoCommands();
        _stateMachine?.Process();
    }
    public void Start()
    {
        var regimes = _data.GetAll<Regime>().ToList();
        var players = _data.GetAll<Player>().ToList();
        if (players.Count == 0) throw new Exception();
        if (players.Count > regimes.Count) throw new Exception();
        for (var i = 0; i < players.Count; i++)
        {
            var m = new ChooseRegimeCommand(regimes[i].MakeRef(),
                players[i].PlayerGuid);
            HandleMessage(m);
        }

        _stateMachine = new StateMachine(_start);
    }

    private bool _handling = false;
    private void HandleMessage(Message m)
    {
        m.Enact(PKey);
        _server.ReceiveMessage(m, _hKey);
    }
    private void DoCommands()
    {
        while (CommandQueue.TryDequeue(out var command))
        {
            if (_handling == true) throw new Exception();
            if(command.Valid(_data)) command.Enact(PKey);
        }
        _server.PushPackets(_hKey);
    }
}

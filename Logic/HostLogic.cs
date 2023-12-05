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
    private ISession _session;
    private StateMachine _stateMachine;
    private TurnState _start, _middle, _end;
    public OrderHolder OrderHolder { get; private set; }
    private HostServer _server; 
    private HostWriteKey _hKey;
    public ProcedureWriteKey PKey { get; private set; }
    private Data _data => _session.Data;
    private readonly object _lock = new object();
    public HostLogic(ISession session)
    {
        _session = session;
        CommandQueue = new ConcurrentQueue<Command>();
        var syncKey = new LogicWriteKey(HandleMessage, session);
        _hKey = new HostWriteKey(this, session);
        PKey = new ProcedureWriteKey(_session);
        
        OrderHolder = new OrderHolder(syncKey);
        
        _start = new TurnStartState(syncKey, OrderHolder);
        _middle = new TurnMiddleState(syncKey, OrderHolder);
        _end = new TurnEndState(syncKey, OrderHolder);
        _start.SetNextState(_middle);
        _middle.SetNextState(_end);
        _end.SetNextState(_start);
    }
    public void SetDependencies(HostServer server)
    {
        _server = server;
    }
    public void Process(float delta)
    {
        DoCommands();
        _stateMachine?.Process();
    }

    public void SubmitPlayerOrders(Player player, RegimeTurnOrders orders)
    {
        OrderHolder.SubmitPlayerTurnOrders(player, orders, _data);
    }

    public void Start()
    {
        SetPlayerRegimes();
        SetInitialRivals();

        _stateMachine = new StateMachine(_start);
    }

    private void SetInitialRivals()
    {
        var regimes = _data.GetAll<Regime>().ToList();
        foreach (var regime in regimes)
        {
            if (Random.Shared.NextSingle() < .75f) continue;
            var neighbors = regime.GetNeighborAlliances(_data);
            if (neighbors.Count() == 0) continue;
            HandleMessage(new DeclareRivalProcedure(regime.GetAlliance(_data).Id, neighbors.First().Id));
        }
    }

    private void SetPlayerRegimes()
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
    }

    private void HandleMessage(Message m)
    {
        lock (_lock)
        {
            m.Enact(PKey);
        }
        _server.ReceiveMessage(m, _hKey);
    }
    private void DoCommands()
    {
        lock (_lock)
        {
            while (CommandQueue.TryDequeue(out var command))
            {
                if(command.Valid(_data)) command.Enact(PKey);
            }
        }
        _server.PushPackets(_hKey);
    }

    
}

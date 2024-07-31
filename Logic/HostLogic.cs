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
    private TurnStateMachine _turnStateMachine;
    private TurnState _start, _middle, _end;
    public bool Calculating => _turnStateMachine.Current != _middle;
    public OrderHolder OrderHolder { get; private set; }
    private HostServer _server; 
    private HostKey _hKey;
    public ProcedureKey PKey { get; private set; }
    private LogicKey _logicKey;
    private Data _data => _session.Data;
    private readonly object _lock = new object();
    public HostLogic(ISession session)
    {
        _session = session;
        CommandQueue = new ConcurrentQueue<Command>();
        _logicKey = new LogicKey(this,
            _server,
            session);
        _hKey = new HostKey(this, session);
        PKey = new ProcedureKey(_session);
        OrderHolder = new OrderHolder(_logicKey);
        
        _start = new TurnStartState(_logicKey, OrderHolder);
        _middle = new TurnMiddleState(_logicKey, OrderHolder);
        _end = new TurnEndState(_logicKey, OrderHolder);
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
        _turnStateMachine?.Process();
    }

    public void SubmitPlayerOrders(Player player, RegimeTurnOrders orders)
    {
        OrderHolder.SubmitPlayerTurnOrders(player, orders, _data);
    }

    public void Start()
    {
        SetPlayerRegimes();
        SetInitialRivals();
        _turnStateMachine = new TurnStateMachine(_start);
    }

    private void SetInitialRivals()
    {
        var regimes = _data.GetAll<Regime>().ToList();
        foreach (var regime in regimes)
        {
            if (Random.Shared.NextSingle() < .75f) continue;
            var alliance = regime.GetAlliance(_data);
            var neighbors = alliance.GetNeighborAlliances(_data);
            if (neighbors.Any() == false) continue;
            HandleMessage(new DeclareRivalProcedure(regime.GetAlliance(_data).Id, 
                neighbors.First().Id));
        }
    }

    private void SetPlayerRegimes()
    {
        var regimes = _data.GetAll<Regime>()
            .Where(r => r.IsMajor)
            .ToList();
        var players = _data.GetAll<Player>().ToList();
        if (players.Count == 0) throw new Exception();
        if (players.Count > regimes.Count) throw new Exception();
        for (var i = 0; i < players.Count; i++)
        {
            var m = new SetPlayerRegimeProcedure(regimes[i].MakeRef(),
                players[i].PlayerGuid);
            HandleMessage(m);
        }
    }

    public void HandleMessage(Message m)
    {
        lock (_lock)
        {
            if (m is Update u)
            {
                u.Enact(PKey);
                _server.ReceiveMessage(m, _hKey);
                return;
            }

            if (m is Procedure p)
            {
                if (p.Valid(_data, out string error))
                {
                    p.Enact(PKey);
                    _server.ReceiveMessage(m, _hKey);
                }
                else
                {
                    GD.Print($"{p.GetType()} error {error}");
                }

                return;
            }

            if (m is Command c)
            {
                CommandQueue.Enqueue(c);
                return;
            }

            if (m is HostProcedure h)
            {
                if (h.Valid(_data, out string error))
                {
                    h.Enact(_logicKey);
                }
                else
                {
                    GD.Print($"{h.GetType()} error {error}");
                }

                return;
            }

            throw new Exception($"message of type {m.GetType()} not handled");
        }
    }
    private void DoCommands()
    {
        lock (_lock)
        {
            while (CommandQueue.TryDequeue(out var command))
            {
                if (command.Valid(_data, out string error))
                {
                    command.Enact(_logicKey);
                }
            }
        }
        _server.PushPackets(_hKey);
    }

    
}

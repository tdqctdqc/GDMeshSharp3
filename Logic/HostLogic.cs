using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public HostLogic(Data data)
    {
        OrderHolder = new OrderHolder();
        CommandQueue = new ConcurrentQueue<Command>();
        data.Requests.QueueCommand.Subscribe(CommandQueue.Enqueue);
        data.Requests.SubmitPlayerOrders
            .Subscribe(x => OrderHolder.SubmitPlayerTurnOrders(x.Item1, x.Item2, _data));
        
        data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank
            .Subscribe(
                () =>
                {
                    OrderHolder.CalcAiTurnOrders(_data);
                }
            );
        _start = new TurnStartState(data);
        _middle = new TurnMiddleState(data, OrderHolder);
        _end = new TurnEndState(data);
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
        _stateMachine.Process();
    }
    public void Start()
    {
        _stateMachine = new StateMachine(_start);
    }
    
    private void EnactResults(LogicResults logicResult)
    {
        for (var i = 0; i < logicResult.Messages.Count; i++)
        {
            logicResult.Messages[i].Enact(PKey);
        }
        _server.ReceiveLogicResult(logicResult, _hKey);
        _server.PushPackets(_hKey);
    }
    private void DoCommands()
    {
        while (CommandQueue.TryDequeue(out var command))
        {
            if(command.Valid(_data)) command.Enact(PKey);
        }
        _server.PushPackets(_hKey);
    }

    
}

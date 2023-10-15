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
    public OrderHolder OrderHolder { get; private set; }
    private HostServer _server; 
    private HostWriteKey _hKey;
    public ProcedureWriteKey PKey { get; private set; }
    private Data _data;
    private LogicModule[] _majorTurnStartModules, _majorTurnEndModules, 
        _minorTurnStartModules, _minorTurnEndModules;
    private TurnCalculator _turnStartCalculator, _turnEndCalculator;
    private bool _turnStartDone = false;
    public HostLogic(Data data)
    {
        OrderHolder = new OrderHolder();
        CommandQueue = new ConcurrentQueue<Command>();
        data.Requests.QueueCommand.Subscribe(CommandQueue.Enqueue);
        data.Requests.SubmitPlayerOrders
            .Subscribe(x => OrderHolder.SubmitPlayerTurnOrders(x.Item1, x.Item2, _data));
        _majorTurnStartModules = new LogicModule[]
        {
        };
        _majorTurnEndModules = new LogicModule[]
        {
            new DefaultLogicModule(() => new PrepareNewHistoriesProcedure()),
            new ProduceConstructModule(),
            new ConstructBuildingsModule(),
            new FoodAndPopGrowthModule(),
            new FinanceModule(),
            new TradeModule(),
            new ProposalsModule(),
            new FormUnitsModule()
        };
        _minorTurnStartModules = new LogicModule[] { };
        _minorTurnEndModules = new LogicModule[] { };
        _turnStartCalculator = new TurnCalculator(EnactResults, data);
        _turnEndCalculator = new TurnCalculator(EnactResults, data);
        
        data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank
            .Subscribe(() => OrderHolder.CalcAiTurnOrders(_data));
    }
    
    public void FirstTurn()
    {
        ProcessTurnBeginning();
    }
    public void Process(float delta)
    {
        if (_turnStartDone)
        {
            ProcessTurnEnd();
        }
        else
        {
            ProcessTurnBeginning();
        }
    }

    private bool ProcessTurnEnd()
    {
        if (_turnEndCalculator.State == TurnCalculator.TurnCalcState.Waiting)
        {
            var majorTurn = _data.BaseDomain.GameClock.MajorTurn(_data);
            DoCommands();
            if(OrderHolder.CheckReadyForFrame(_data, majorTurn))
            {
                List<LogicModule> modules;
                if (majorTurn)
                {
                    modules = _majorTurnEndModules.ToList();
                }
                else
                {
                    modules = _minorTurnEndModules.ToList();
                }
                _turnEndCalculator.Calculate(modules, 
                    OrderHolder, _data);
            }
        }
        else if (_turnEndCalculator.State == TurnCalculator.TurnCalcState.Calculating)
        {
            _turnEndCalculator.CheckOnCalculation();
        }
        else if (_turnEndCalculator.State == TurnCalculator.TurnCalcState.Finished)
        {
            //todo ticking for remote as well?
            OrderHolder.Clear();
            var tick = new TickProcedure();
            var res = new LogicResults();
            res.Messages.Add(tick);
            EnactResults(res);
            _turnEndCalculator.MarkDone();
            _turnStartDone = false;
            DoCommands();
            ProcessTurnBeginning();
            return true;
        }
        return false;
    }

    private bool ProcessTurnBeginning()
    {
        if (_turnStartCalculator.State == TurnCalculator.TurnCalcState.Waiting)
        {
            DoCommands();
            List<LogicModule> modules;
            if (_data.BaseDomain.GameClock.MajorTurn(_data))
            {
                modules = _majorTurnStartModules.ToList();
            }
            else
            {
                modules = _minorTurnStartModules.ToList();
            }
            _turnStartCalculator.Calculate(modules, 
                OrderHolder, _data);
        }
        else if (_turnStartCalculator.State == TurnCalculator.TurnCalcState.Calculating)
        {
            _turnStartCalculator.CheckOnCalculation();
        }
        else if (_turnStartCalculator.State == TurnCalculator.TurnCalcState.Finished)
        {
            //todo ticking for remote as well?
            OrderHolder.Clear();
            _turnStartCalculator.MarkDone();
            _turnStartDone = true;
            var proc = new FinishedTurnStartCalcProc();
            var res = new LogicResults();
            res.Messages.Add(proc);
            EnactResults(res);
            DoCommands();
            OrderHolder.CalcAiTurnOrders(_data);
            return true;
        }
        return false;
    }
    public void SetDependencies(HostServer server, GameSession session, Data data)
    {
        _data = data;
        _server = server;
        _hKey = new HostWriteKey(this, data);
        PKey = new ProcedureWriteKey(data);
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
        var queuedProcs = new List<Procedure>();
        var logicResult = new LogicResults(new List<Message>(), new List<Func<HostWriteKey, Entity>>());
        var commandCount = CommandQueue.Count;
        for (var i = 0; i < commandCount; i++)
        {
            if (CommandQueue.TryDequeue(out var command))
            {
                if(command.Valid(_data)) command.Enact(PKey);
            }
        }
        for (var i = 0; i < logicResult.Messages.Count; i++)
        {
            logicResult.Messages[i].Enact(PKey);
        }
        _server.ReceiveLogicResult(logicResult, _hKey);
        _server.PushPackets(_hKey);
    }
}

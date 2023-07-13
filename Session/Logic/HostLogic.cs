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
    private EntityValueCache<Regime, RegimeAi> _regimeAis;
    private ConcurrentDictionary<Player, TurnOrders> _playerTurnOrders;
    private ConcurrentDictionary<Regime, Task<TurnOrders>> _aiTurnOrders;
    private HostServer _server; 
    private HostWriteKey _hKey;
    private ProcedureWriteKey _pKey;
    private Data _data;
    private Task<bool> _calculatingAiOrders;
    private LogicModule[] _majorTurnStartModules, _majorTurnEndModules, _minorTurnStartModules, _minorTurnEndModules;
    private TurnCalculator _turnStartCalculator, _turnEndCalculator;
    private bool _turnStartDone = false;
    public HostLogic(Data data)
    {
        _playerTurnOrders = new ConcurrentDictionary<Player, TurnOrders>();
        _aiTurnOrders = new ConcurrentDictionary<Regime, Task<TurnOrders>>();
        _regimeAis = EntityValueCache<Regime, RegimeAi>
            .ConstructConstant(data, r => new RegimeAi(r, data));
        CommandQueue = new ConcurrentQueue<Command>();
        _majorTurnStartModules = new LogicModule[]
        {
            new ProduceConstructModule()
        };
        _majorTurnEndModules = new LogicModule[]
        {
            new ConstructBuildingsModule(),
            new FoodAndPopGrowthModule(),
            new FinanceModule(),
            new TradeModule()
        };
        _minorTurnStartModules = new LogicModule[] { };
        _minorTurnEndModules = new LogicModule[] { };
        _turnStartCalculator = new TurnCalculator(EnactResults, data);
        _turnEndCalculator = new TurnCalculator(EnactResults, data);
        
        data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank.Subscribe(CalcAiTurnOrders);
    }

    public void Start()
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
            DoCommands();
            if(CheckReadyForFrame())
            {
                List<LogicModule> modules;
                if (_data.BaseDomain.GameClock.MajorTurn(_data))
                {
                    modules = _majorTurnEndModules.ToList();
                }
                else
                {
                    modules = _minorTurnEndModules.ToList();
                }
                _turnEndCalculator.Calculate(modules, 
                    _playerTurnOrders, _aiTurnOrders, _data);
            }
        }
        else if (_turnEndCalculator.State == TurnCalculator.TurnCalcState.Calculating)
        {
            _turnEndCalculator.CheckOnCalculation();
        }
        else if (_turnEndCalculator.State == TurnCalculator.TurnCalcState.Finished)
        {
            //todo ticking for remote as well?
            _playerTurnOrders.Clear();
            _aiTurnOrders.Clear();
            var tick = new TickProcedure();
            var res = new LogicResults();
            res.Procedures.Add(tick);
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
                _playerTurnOrders, _aiTurnOrders, _data);
        }
        else if (_turnStartCalculator.State == TurnCalculator.TurnCalcState.Calculating)
        {

            _turnStartCalculator.CheckOnCalculation();
        }
        else if (_turnStartCalculator.State == TurnCalculator.TurnCalcState.Finished)
        {
            //todo ticking for remote as well?
            _playerTurnOrders.Clear();
            _aiTurnOrders.Clear();
            _turnStartCalculator.MarkDone();
            _turnStartDone = true;
            var proc = new FinishedTurnStartCalcProc();
            var res = new LogicResults();
            res.Procedures.Add(proc);
            EnactResults(res);
            
            DoCommands();
            CalcAiTurnOrders();
            return true;
        }
        return false;
    }
    public void SetDependencies(HostServer server, GameSession session, Data data)
    {
        _data = data;
        _server = server;
        _hKey = new HostWriteKey(server, this, data, session);
        _pKey = new ProcedureWriteKey(data, session);
        _regimeAis = data.HostLogicData.AIs;
    }

    public void SubmitPlayerTurnOrders(Player player, TurnOrders orders)
    {
        if (orders.Tick != _data.BaseDomain.GameClock.Tick) throw new Exception();
        var added = _playerTurnOrders.TryAdd(player, orders);
        if (added == false) throw new Exception();
    }

    private bool CheckReadyForFrame()
    {
        var players = _data.BaseDomain.Players.Entities;
        var aiRegimes = _data.Society.Regimes.Entities.Where(r => r.IsPlayerRegime(_data) == false);
        foreach (var kvp in _aiTurnOrders)
        {
            if (kvp.Value.IsFaulted)
            {
                throw kvp.Value.Exception;
            }
        }

        var allPlayersHaveRegime = players.All(p => p.Regime.Empty() == false);
        
        var allPlayersSubmitted = players.All(p => _playerTurnOrders.ContainsKey(p));

        var allAisHaveEntry = aiRegimes.All(p => _aiTurnOrders.ContainsKey(p));

        var allAisCompleted = _aiTurnOrders.All(kvp => kvp.Value.IsCompleted);

        return allPlayersHaveRegime && allPlayersSubmitted && allAisHaveEntry && allAisCompleted;
    }
    private void CalcAiTurnOrders()
    {
        if (_data.BaseDomain.GameClock.MajorTurn(_data))
        {
            inner(r => _regimeAis[r].GetMajorTurnOrders(_data));
        }
        else
        {
            inner(r => _regimeAis[r].GetMinorTurnOrders(_data));
        }

        void inner(Func<Regime, TurnOrders> getOrders)
        {
            var aiRegimes = _data.Society.Regimes.Entities
                .Where(r => r.IsPlayerRegime(_data) == false);
            foreach (var aiRegime in aiRegimes)
            {
                if (_aiTurnOrders.ContainsKey(aiRegime) == false)
                {
                    var task = Task.Run(() =>
                    {
                        return (TurnOrders) getOrders(aiRegime);
                    });
                    _aiTurnOrders.TryAdd(aiRegime, task);
                }
            }
        }
    }
    
    private void EnactResults(LogicResults logicResult)
    {
        for (var i = 0; i < logicResult.Procedures.Count; i++)
        {
            logicResult.Procedures[i].Enact(_pKey);
        }

        for (int i = 0; i < logicResult.CreateEntities.Count; i++)
        {
            logicResult.CreateEntities[i].Invoke(_hKey);
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
                if(command.Valid(_data)) command.Enact(_hKey, logicResult.Procedures.Add);
            }
        }
        for (var i = 0; i < logicResult.Procedures.Count; i++)
        {
            logicResult.Procedures[i].Enact(_pKey);
        }
        _server.ReceiveLogicResult(logicResult, _hKey);
        _server.PushPackets(_hKey);
    }
}

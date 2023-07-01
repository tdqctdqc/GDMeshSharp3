using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TurnCalculator
{
    public bool Calculating { get; private set; }
    public TurnCalcState State { get; private set; }
    private List<LogicModule> _modules;
    private Data _data;
    private Task<LogicResults> _currTask;
    private Action<LogicResults> _enact;
    public enum TurnCalcState
    {
        Waiting,
        Calculating,
        Finished
    }

    public TurnCalculator(Action<LogicResults> enact, Data data)
    {
        _data = data;
        _enact = enact;
        State = TurnCalcState.Waiting;
    }
    public void StartCalculateTurn(List<LogicModule> modules,
        IDictionary<Player, TurnOrders> playerTurnOrders,
        IDictionary<Regime, Task<TurnOrders>> aiTurnOrders, Data d)
    {
        State = TurnCalcState.Calculating;
        _modules = modules;
        _currTask = Task.Run(() => HandleTurnOrders(playerTurnOrders, aiTurnOrders, d));
    }
    public void CheckOnCalculation()
    {
        if (_currTask.IsCompleted)
        {
            if (_currTask.IsFaulted)
            {
                throw _currTask.Exception;
            }
            _enact(_currTask.Result);
            if (_modules.Count > 0)
            {
                var module = _modules[0];
                _modules.RemoveAt(0);
                _currTask = Task.Run(() => module.Calculate(_data));
            }
            else
            {
                State = TurnCalcState.Finished;
                _currTask = null;
            }
        }
    }

    public void MarkDone()
    {
        State = TurnCalcState.Waiting;
    }
    private LogicResults HandleTurnOrders(IDictionary<Player, TurnOrders> playerTurnOrders,
        IDictionary<Regime, Task<TurnOrders>> aiTurnOrders, Data d)
    {
        var turnOrdersResult = new LogicResults();
        
        foreach (var kvp in playerTurnOrders)
        {
            if (kvp.Key.Regime.Entity().IsPlayerRegime(_data) == false) continue;
            var orders = kvp.Value;
            orders.WriteToResult(turnOrdersResult, d);
        }
        foreach (var kvp in aiTurnOrders)
        {
            if (kvp.Key.IsPlayerRegime(_data)) continue;
            var orders = kvp.Value.Result;
            orders.WriteToResult(turnOrdersResult, d);
        }

        return turnOrdersResult;
    }
}

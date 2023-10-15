using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class TurnCalculator
{
    public bool Calculating { get; private set; }
    public TurnCalcState State { get; private set; }
    private List<LogicModule> _modules;
    private List<TurnOrders> _orders;
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
    public void Calculate(List<LogicModule> modules,
        OrderHolder orderHolder, Data d)
    {
        State = TurnCalcState.Calculating;
        _modules = modules;
        _orders = GetOrdersList(orderHolder.PlayerTurnOrders, 
            orderHolder.AiTurnOrders);
        StartNextModule();
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
                StartNextModule();
            }
            else
            {
                State = TurnCalcState.Finished;
                _currTask = null;
            }
        }
    }

    private void StartNextModule()
    {
        if (_modules.Count == 0)
        {
            State = TurnCalcState.Finished;
            _currTask = null;
            return;
        }
        var module = _modules[0];
        _modules.RemoveAt(0);
        _currTask = Task.Run(() => module.Calculate(_orders, _data));
    }
    public void MarkDone()
    {
        State = TurnCalcState.Waiting;
    }

    private List<TurnOrders> GetOrdersList(IDictionary<Player, TurnOrders> playerTurnOrders,
        IDictionary<Regime, Task<TurnOrders>> aiTurnOrders)
    {
        var res = new List<TurnOrders>();
        foreach (var kvp in playerTurnOrders)
        {
            if (kvp.Key.Regime.Entity(_data).IsPlayerRegime(_data) == false) continue;
            var orders = kvp.Value;
            res.Add(orders);
        }
        foreach (var kvp in aiTurnOrders)
        {
            if (kvp.Key.IsPlayerRegime(_data)) continue;
            var orders = kvp.Value.Result;
            res.Add(orders);
        }
        return res;
    }
    
}

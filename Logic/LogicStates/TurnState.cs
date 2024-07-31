
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public abstract class TurnState
{
    public TurnState NextState { get; private set; }
    protected LogicModule[] _majorModules, _minorModules;
    protected OrderHolder _orders;
    protected LogicKey _key;
    public bool Calculating { get; protected set; }

    public TurnState(LogicKey key, 
        OrderHolder orders)
    {
        _key = key;
        _orders = orders;
    }

    public void SetNextState(TurnState next)
    {
        NextState = next;
    }
    public virtual void Calculate()
    {
        Calculating = true;
        if (_key.Data.BaseDomain.GameClock.MajorTurn(_key.Data))
        {
            CalculateMajor();
        }
        else
        {
            CalculateMinor();
        }

        Calculating = false;
    }
    private void CalculateMajor()
    {
        var sw = new Stopwatch();
        foreach (var module in _majorModules)
        {
            sw.Reset();
            sw.Start();
            module.Calculate(_orders.GetOrdersList(_key.Data), _key);
            sw.Stop();
            _key.Data.Logger.Log($" {module.GetType().Name} time {sw.Elapsed.TotalMilliseconds}",
                LogType.Logic);   
        }
    }
    private void CalculateMinor()
    {
        var sw = new Stopwatch();
        foreach (var module in _minorModules)
        {
            sw.Reset();
            sw.Start();
            module.Calculate(_orders.GetOrdersList(_key.Data), _key);
            sw.Stop();
            _key.Data.Logger.Log($" {module.GetType().Name} time {sw.Elapsed.TotalMilliseconds}",
                LogType.Logic);
        }
    }

    public abstract bool ReadyForNext();
}
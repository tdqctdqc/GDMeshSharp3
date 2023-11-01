
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public abstract class TurnState : State
{
    private Task _calculation;
    private TurnState _nextState;
    protected LogicModule[] _majorModules, _minorModules;
    private OrderHolder _orders;
    private LogicWriteKey _key;

    public TurnState(LogicWriteKey key, 
        OrderHolder orders)
    {
        _key = key;
        _orders = orders;
    }

    public void SetNextState(TurnState next)
    {
        _nextState = next;
    }
    public override void Enter()
    {
        Game.I.Logger.Log("Entering state "  + GetType().Name, LogType.Logic);
        if (_calculation != null) throw new Exception();
        _calculation = Task.Run(Calculate);
    }
    private void Calculate()
    {
        if (_key.Data.BaseDomain.GameClock.MajorTurn(_key.Data))
        {
            CalculateMajor();
        }
        else
        {
            CalculateMinor();
        }
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
            Game.I.Logger.Log($" {module.GetType().Name} time {sw.Elapsed.TotalMilliseconds}",
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
            Game.I.Logger.Log($" {module.GetType().Name} time {sw.Elapsed.TotalMilliseconds}",
                LogType.Logic);
        }
    }
    public override State Check()
    {
        if (_calculation.IsFaulted)
        {
            throw _calculation.Exception;
        }
        if (_calculation.IsCompleted)
        {
            _calculation = null;
            return _nextState;
        }
        
        return this;
    }
}
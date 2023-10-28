
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class TurnState : State
{
    private Data _data;
    private Task _calculation;
    private TurnState _nextState;
    protected LogicModule[] _majorModules, _minorModules;
    private OrderHolder _orders;

    public TurnState(Data data)
    {
        _data = data;
    }

    public void SetNextState(TurnState next)
    {
        _nextState = next;
    }
    public override void Enter()
    {
        _calculation = Task.Run(Calculate);
    }

    private void Calculate()
    {
        if (_data.BaseDomain.GameClock.MajorTurn(_data))
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
        foreach (var module in _majorModules)
        {
            module.Calculate(_orders.GetOrdersList(_data), _data);
        }
    }

    private void CalculateMinor()
    {
        foreach (var module in _minorModules)
        {
            module.Calculate(_orders.GetOrdersList(_data), _data);
        }
    }
    public override State Check()
    {
        if (_calculation.IsFaulted)
        {
            throw _calculation.Exception.InnerException;
        }
        if (_calculation.IsCompleted)
        {
            return _nextState;
        }

        return this;
    }
}
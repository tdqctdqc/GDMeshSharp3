
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public abstract class TurnState : State
{
    private Data _data;
    private Task _calculation;
    private TurnState _nextState;
    protected LogicModule[] _majorModules, _minorModules;
    private OrderHolder _orders;
    private Action<Message> _sendMessage;

    public TurnState(Data data, Action<Message> sendMessage,
        OrderHolder orders)
    {
        _data = data;
        _sendMessage = sendMessage;
        _orders = orders;
    }

    public void SetNextState(TurnState next)
    {
        _nextState = next;
    }
    public override void Enter()
    {
        GD.Print("entering state " + GetType().Name);
        if (_calculation != null) throw new Exception();
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
            module.Calculate(_orders.GetOrdersList(_data), _data,
                _sendMessage);
        }
    }

    private void CalculateMinor()
    {
        foreach (var module in _minorModules)
        {
            module.Calculate(_orders.GetOrdersList(_data), _data,
                _sendMessage);
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
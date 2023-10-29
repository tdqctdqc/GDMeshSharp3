using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class LogicModule
{
    public abstract void Calculate(List<RegimeTurnOrders> orders, 
        Data data, Action<Message> queueMessage);
}
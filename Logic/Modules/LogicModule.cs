using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class LogicModule
{
    public abstract LogicResults Calculate(List<TurnOrders> orders, 
        Data data);
}
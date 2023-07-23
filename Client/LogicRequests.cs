using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LogicRequests
{
    public RefAction<Command> QueueCommand { get; private set; }
    public RefAction<(Player, TurnOrders)> SubmitPlayerOrders { get; private set; }
    public LogicRequests()
    {
        QueueCommand = new RefAction<Command>();
        SubmitPlayerOrders = new RefAction<(Player, TurnOrders)>();
    }
}

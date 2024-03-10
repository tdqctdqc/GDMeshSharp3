using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface ILogic
{
    bool Calculating { get; }
    void Process(float delta);
    void SubmitPlayerOrders(Player player, RegimeTurnOrders orders);
}
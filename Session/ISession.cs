using Godot;
using System;

public interface ISession
{
    // RefFulfiller RefFulfiller { get; }
    Client Client { get; }
    IServer Server { get; }
    ILogic Logic { get; }
    Data Data { get; }
    void QueueFree();
}

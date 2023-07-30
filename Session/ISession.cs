using Godot;
using System;

public interface ISession
{
    // RefFulfiller RefFulfiller { get; }
    Client Client { get; }
    IServer Server { get; }
    void QueueFree();
}

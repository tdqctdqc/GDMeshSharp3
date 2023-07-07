using Godot;
using System;

public interface ISession
{
    // RefFulfiller RefFulfiller { get; }
    IClient Client { get; }
    IServer Server { get; }
    void QueueFree();
    void Setup();
}

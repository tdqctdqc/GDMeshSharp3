using Godot;
using System;
using System.Collections.Concurrent;

public interface IClient
{
    void HandleInput(InputEvent e, float delta);
    void Process(float delta);
    ClientSettings Settings { get; }
    UiRequests UiRequests { get; }
    ConcurrentQueue<Action> QueuedUpdates { get; }
}

using Godot;
using System;

public interface IClient
{
    void HandleInput(InputEvent e, float delta);
    void Process(float delta);
    ICameraController Cam { get; }
    ClientSettings Settings { get; }
    ClientWriteKey WriteKey { get; }
    UiRequests UiRequests { get; }
}

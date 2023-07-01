using Godot;
using System;

public interface IServer
{
    void QueueCommandLocal(Command c);
}

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IClientComponent
{
    Node Node { get; }
    Action Disconnect { get; set; }
    void Process(float delta);
}

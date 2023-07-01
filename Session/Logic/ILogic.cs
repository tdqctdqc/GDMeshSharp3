using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface ILogic
{
    bool Process(float delta);
}
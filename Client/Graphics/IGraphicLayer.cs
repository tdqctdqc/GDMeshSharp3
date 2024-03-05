using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public interface IGraphicLayer
{
    int Z { get; }
    MultiSettings GetSettings();
    string Name { get; }
}
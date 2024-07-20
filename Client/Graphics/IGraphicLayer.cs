using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public interface IGraphicLayer : INamed
{
    int Z { get; }
    MultiSettings GetSettings();
}
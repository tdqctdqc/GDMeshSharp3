using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public interface IGraphicLayer
{
    int Z { get; }
    List<ISettingsOption> Settings { get; }
    Control GetControl();
    string Name { get; }
    bool Visible { get; set; }
}
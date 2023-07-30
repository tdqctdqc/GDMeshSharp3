using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class UiRequests 
{
    public RefAction<PolyTriPosition> MouseOver { get; private set; }
    public RefAction<string> ToggleMapGraphicsLayer { get; private set; }

    public UiRequests()
    {
        MouseOver = new RefAction<PolyTriPosition>();
        ToggleMapGraphicsLayer = new RefAction<string>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapGraphicsOptions : VBoxContainer
{
    public MapGraphicsOptions()
    {
        
    }

    public void Setup(MapGraphics mg)
    {
        this.ClearChildren();
        foreach (var graphicLayer in mg.GraphicLayerHolder.Layers)
        {
            AddChild(graphicLayer.GetControl());
        }
    }
}

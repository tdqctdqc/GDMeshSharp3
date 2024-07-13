using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepJob : IModel, IIconed
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public Icon Icon { get; private set; }
    public float Income { get; private set; }
    
    public PeepJob()
    {
    }

    public void CreateIcon()
    {
        Icon = Icon.Create(Name, new Vector2I(1, 2));
    }
}
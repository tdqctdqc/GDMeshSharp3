using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepJob : IModel
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public Icon Icon { get; }
    public float Income { get; private set; }
    
    public PeepJob(string name, float income)
    {
        Name = name;
        Icon = Icon.Create(Name, new Vector2I(1, 2));
        Income = income;
    }
}
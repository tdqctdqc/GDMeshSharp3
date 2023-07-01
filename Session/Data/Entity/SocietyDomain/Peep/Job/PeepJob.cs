using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepJob : IModel
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public Icon JobIcon { get; } 
    public PeepJob(string name)
    {
        Name = name;
        JobIcon = Icon.Create(Name, Icon.AspectRatio._1x2, 50f);
    }
}
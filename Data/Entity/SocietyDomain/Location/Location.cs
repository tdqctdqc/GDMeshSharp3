using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public abstract class Location : Entity
{
    public Location(int id) : base(id)
    {
    }
    
}
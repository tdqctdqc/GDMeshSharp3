using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Flow : IModel
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public Icon Icon { get; private set; }
    
    
    protected Flow(string name)
    {
        Name = name;
        Icon = Icon.Create(GetType().Name, Icon.AspectRatio._1x1, 25f);
    }
    public abstract float GetNonBuildingSupply(Regime r, Data d);
    public abstract float GetConsumption(Regime r, Data d);
}


using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class SettlementBuilding : IModel, IMakeable, IIconed
{
    public int Id { get; private set; }
    public string Name { get; }
    public LaborComponent Labor { get; private set; }
    public MakeableAttribute Makeable { get; private set; }
    

    public Icon Icon { get; }
    public SettlementBuilding( 
        string name, 
        LaborComponent labor, 
        MakeableAttribute makeable)
    {
        Name = name;
        Icon = Icon.Create(Name, Vector2I.One);
        Makeable = makeable;
        Labor = labor;
    }
    
    public abstract bool CanBuildInCell(Cell t, Data data);
    public abstract bool CanBuildInPoly(MapPolygon p, Data data);
    
}

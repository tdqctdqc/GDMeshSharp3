
using System.Collections.Generic;
using Godot;

public class DeployOnLineOrder : UnitOrder
{
    public List<Vector2> Points { get; private set; }
    public override void Handle(UnitGroup g, Data d, HandleUnitOrdersProcedure proc)
    {
        
    }
}

using System.Collections.Generic;
using Godot;

public class DeployOnLineOrder : UnitOrder
{
    public List<int> WaypointIds { get; private set; }

    public DeployOnLineOrder(List<int> waypointIds)
    {
        WaypointIds = waypointIds;
    }

    public override void Handle(UnitGroup g, Data d, HandleUnitOrdersProcedure proc)
    {
        // GD.Print("havent implemented " + GetType().Name);
    }
}
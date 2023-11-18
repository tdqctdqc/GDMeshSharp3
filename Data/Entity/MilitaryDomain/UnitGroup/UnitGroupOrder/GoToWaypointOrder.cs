
using Godot;

public class GoToWaypointOrder : UnitOrder
{
    public int DestinationWaypointId { get; private set; }

    public GoToWaypointOrder(int destinationWaypointId)
    {
        DestinationWaypointId = destinationWaypointId;
    }

    public override void Handle(UnitGroup g, Data d, HandleUnitOrdersProcedure proc)
    {
        var context = d.Context;
        var destWp = d.Planet.NavWaypoints.Waypoints[DestinationWaypointId];
        foreach (var unit in g.Units.Items(d))
        {
            var currWp = context.UnitWaypoints[unit];
            var pos = unit.Position;
            if (currWp == destWp)
            {
            }
            else
            {
                var path = context.GetWaypointPath(currWp, destWp, d);
            }

            proc.NewUnitPosesById[unit.Id] = destWp.Pos;
        }

        // GD.Print("havent implemented go to yet");
    }

    public override void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data data)
    {
        return;
    }

    private void MoveToWaypointCenter(Unit u, Waypoint wp, ref Vector2 pos)
    {
        
    }
}
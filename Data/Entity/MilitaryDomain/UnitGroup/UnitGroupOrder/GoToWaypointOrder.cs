
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
        GD.Print("havent implemented go to yet");
    }
}
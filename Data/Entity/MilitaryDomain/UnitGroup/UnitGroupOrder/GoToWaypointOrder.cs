
using Godot;

public class GoToWaypointOrder : UnitOrder
{
    public int DestinationWaypointId { get; private set; }
    public override void Handle(UnitGroup g, Data d, HandleUnitOrdersProcedure proc)
    {
        GD.Print("havent implemented go to yet");
    }
}
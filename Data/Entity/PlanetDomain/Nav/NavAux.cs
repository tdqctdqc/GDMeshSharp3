
using Godot;

public class NavAux
{
    public Grid<Waypoint> WaypointGrid { get; private set; }
    public NavAux(Data data)
    {
        data.Notices.FinishedStateSync.Subscribe(() => MakeWaypointGrid(data));
        data.Notices.MadeWaypoints.Subscribe(() => MakeWaypointGrid(data));
    }

    private void MakeWaypointGrid(Data data)
    {
        var gridCellSize = 100f;
        var numPartitions = Mathf.CeilToInt(data.Planet.Info.Dimensions.X / gridCellSize);
        WaypointGrid = new Grid<Waypoint>(numPartitions, data.Planet.Info.Dimensions, wp => wp.Pos,
            data);
        foreach (var wp in data.Planet.Nav.Waypoints.Values)
        {
            WaypointGrid.AddElement(wp);
        }
    }
}
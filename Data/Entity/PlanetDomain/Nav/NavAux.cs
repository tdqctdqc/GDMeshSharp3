
using Godot;

public class NavAux
{
    public CylinderGrid<Waypoint> WaypointGrid { get; private set; }
    public NavAux(Data data)
    {
        data.Notices.FinishedStateSync.Subscribe(() => MakeWaypointGrid(data));
        data.Notices.MadeWaypoints.Subscribe(() => MakeWaypointGrid(data));
    }

    private void MakeWaypointGrid(Data data)
    {
        var gridCellSize = 100f;
        var numPartitions = Mathf.CeilToInt(data.Planet.Info.Dimensions.X / gridCellSize);
        WaypointGrid = new CylinderGrid<Waypoint>(
            data.Planet.Info.Dimensions,
            100f, wp => wp.Pos);
        foreach (var wp in data.Planet.Nav.Waypoints.Values)
        {
            WaypointGrid.Add(wp);
        }
    }
}
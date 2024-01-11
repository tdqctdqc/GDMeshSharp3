
using Godot;

public class MilitaryDomain
{
    public UnitAux UnitAux { get; private set; }
    public FrontAux FrontAux { get; private set; }
    private SingletonAux<TacticalWaypoints> _tacticalWaypoints;
    public TacticalWaypoints TacticalWaypoints => _tacticalWaypoints.Value;
    public CylinderGrid<Waypoint> WaypointGrid { get; private set; }

    public MilitaryDomain()
    {
    }

    public void Setup(Data data)
    {
        UnitAux = new UnitAux(data);
        FrontAux = new FrontAux(data);
        _tacticalWaypoints = new SingletonAux<TacticalWaypoints>(data);
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
        foreach (var wp in TacticalWaypoints.Waypoints.Values)
        {
            WaypointGrid.Add(wp);
        }
    }

    public static Waypoint GetWaypoint(int id, Data d)
    {
        return d.Military.TacticalWaypoints.Waypoints[id];
    }
}
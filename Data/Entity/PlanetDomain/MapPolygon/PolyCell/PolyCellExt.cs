
public static class PolyCellExt
{
    public static RoadModel GetRoadWith(this PolyCell p1, PolyCell p2, Data d)
    {
        return d.Infrastructure.RoadNetwork.Get(p1, p2, d);
    }

    public static bool RivalControlled(this PolyCell p, Alliance a, Data d)
    {
        if (p.Controller.Empty()) return false;
        var controllerAlliance = p.Controller.Entity(d).GetAlliance(d);
        return a.Rivals.RefIds.Contains(controllerAlliance.Id);
    }
    public static bool Controlled(this PolyCell p, Alliance a, Data d)
    {
        return a.Members.RefIds.Contains(p.Controller.RefId);
    }

    public static MapChunk GetChunk(this PolyCell p, Data d)
    {
        return d.Planet.PolygonAux.ChunksByCell[p];
    }
}
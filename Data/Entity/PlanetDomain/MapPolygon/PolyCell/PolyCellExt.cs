
public static class PolyCellExt
{
    public static RoadModel GetRoadWith(this PolyCell p1, PolyCell p2, Data d)
    {
        return d.Infrastructure.RoadNetwork.Get(p1, p2, d);
    }

    public static bool RivalControlled(this PolyCell p, Alliance a, Data d)
    {
        return a.Rivals.RefIds.Contains(p.Controller.RefId);
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
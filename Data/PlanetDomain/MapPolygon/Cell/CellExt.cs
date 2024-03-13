
using System.Collections.Generic;

public static class CellExt
{
    public static RoadModel GetRoadWith(this Cell p1, Cell p2, Data d)
    {
        return d.Infrastructure.RoadNetwork.Get(p1, p2, d);
    }

    public static bool RivalControlled(this Cell p, Alliance a, Data d)
    {
        if (p.Controller.IsEmpty()) return false;
        var controllerAlliance = p.Controller.Get(d).GetAlliance(d);
        return a.IsRivals(controllerAlliance, d);
    }
    public static bool Controlled(this Cell p, Alliance a, Data d)
    {
        return a.Members.RefIds.Contains(p.Controller.RefId);
    }

    public static MapChunk GetChunk(this Cell p, Data d)
    {
        return d.Planet.MapAux.ChunksByCell[p];
    }

    public static IEnumerable<Unit> GetUnits(this Cell cell, Data d)
    {
        return d.Military.UnitAux.UnitsByCell[cell];
    }

    public static bool HasBuilding(this Cell c, Data d)
    {
        return d.Infrastructure.BuildingAux.ByCell.Contains(c);
    }
    public static MapBuilding GetBuilding(this Cell c, Data d)
    {
        return d.Infrastructure.BuildingAux.ByCell[c];
    }

    public static bool HasPeep(this Cell c, Data d)
    {
        return d.Society.PolyPeepAux.ByCell.Contains(c);
    }
    public static Peep GetPeep(this Cell c, Data d)
    {
        return d.Society.PolyPeepAux.ByCell[c];
    }
    
    public static bool HasResourceDeposit(this Cell c, Data d)
    {
        return d.Planet.ResourceDepositAux.ByCell.Contains(c);
    }
    public static ResourceDeposit GetResourceDeposit(this Cell c, Data d)
    {
        return d.Planet.ResourceDepositAux.ByCell[c];
    }
    
    public static bool HasSettlement(this Cell c, Data d)
    {
        return d.Infrastructure.SettlementAux.ByCell.Contains(c);
    }
    public static Settlement GetSettlement(this Cell c, Data d)
    {
        return d.Infrastructure.SettlementAux.ByCell[c];
    }
}
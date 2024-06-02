
using System;
using System.Collections.Generic;
using System.Linq;
public static class CellExt
{
    public static RoadModel GetRoadWith(this Cell p1, Cell p2, Data d)
    {
        return d.Infrastructure.RoadNetwork.Get(p1, p2, d);
    }
    public static bool FriendlyControlled(this Cell p, Alliance a, Data d)
    {
        if (p.Controller.IsEmpty()) return false;
        var controllerAlliance = p.Controller.Get(d).GetAlliance(d);
        return a == controllerAlliance;
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

    public static MapChunk GetChunk(this Cell c, Data d)
    {
        if (c is IPolyCell p)
        {
            return p.Polygon.Get(d).GetChunk(d);
        }

        if (c is IEdgeCell e)
        {
            return e.Edge.Get(d).HighPoly.Get(d).GetChunk(d);
        }

        throw new Exception();
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

    public static float GetLaborDemand(this LandCell c, Data d)
    {
        var laborDemand = 0f;
        if (c.FoodProd.Nums.Contents.Count > 0)
        {
            var foodProdLabor = c.FoodProd.Nums
                .GetEnumerableModel(d)
                .Sum(kvp => kvp.Key.Labor.TotalLabor() * kvp.Value);
            laborDemand += foodProdLabor;
        }

        if (c.GetResourceDeposit(d) is ResourceDeposit r
            && r.Extraction.Get(d) is ResourceExtractionBuilding b)
        {
            laborDemand += b.BaseLabor;
        }

        if (c.GetSettlement(d) is Settlement s
            && s.Buildings.Contents.Count > 0)
        {
            laborDemand += s.Buildings.GetEnumerableModel(d)
                .Sum(kvp =>
                {
                    if (kvp.Key.GetComponent<LaborComponent>() is LaborComponent l)
                    {
                        return l.TotalLabor();
                    }

                    return 0f;
                });
        }

        return laborDemand;
    }

    public static IEnumerable<Army> GetOccupyingArmies(this Cell c, Data d)
    {
        return d.Military.UnitAux.ArmiesByOccupancy[c];
    }
    public static IEnumerable<Army> GetArmiesWithHomeHere(this Cell c, Data d)
    {
        return d.Military.UnitAux.ArmiesByHomeCell[c];
    }
}
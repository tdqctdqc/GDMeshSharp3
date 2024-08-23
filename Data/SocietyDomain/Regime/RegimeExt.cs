
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;

public static class RegimeExt
{
    public static bool IsRivals(this Regime r1, Regime r2, Data d)
    {
        return d.Society.DiploGraph.HasRelation(r1, r2, DiploRelation.Rivals);
    }
    public static bool IsAtWar(this Regime a, Regime b, Data d)
    {
        return d.Society.DiploGraph.HasRelation(a, b, DiploRelation.War);
    }
    public static IEnumerable<Regime> GetRivals(this Regime a, Data d)
    {
        return d.Society.DiploGraph.GetRelations(a, DiploRelation.Rivals, d);
    }
    public static IEnumerable<Regime> GetAtWar(this Regime a, Data d)
    {
        return d.Society.DiploGraph.GetRelations(a, DiploRelation.War, d);
    }
    public static void CreateRelation(this Regime a, Regime b, DiploRelation e, IWriteKey key)
    {
        key.GetData().Society.DiploGraph.AddEdge(a, b, e, key);
    }
    
    public static IEnumerable<Regime> GetNeighborRegimes(this Regime regime, Data data)
    {
        return regime.GetCells(data)
            .SelectMany(p => p.GetNeighbors(data).Where(e => e.Controller.Fulfilled()))
            .Select(p => p.Controller.Get(data))
            .Distinct()
            .Where(a => a != regime);
    }
    
    public static Dictionary<Troop, float> GetAllTroopAmounts(this Regime r, Data d)
    {
        var troops = r.Stock.Stock.GetEnumModel(d)
            .Select(kvp => kvp.Key)
            .OfType<Troop>()
            .ToDictionary(t => t, t => r.Stock.Stock.Get(t));
        foreach (var unit in r.GetUnits(d))
        {
            foreach (var (key, value) in unit.Troops.GetEnumModel(d))
            {
                troops.AddOrSum(key, value);
            }
        };
        return troops;
    }
    public static RegimeAi GetAi(this Regime r, Data d)
    {
        return d.HostLogicData.RegimeAis[r];
    }
    public static IEnumerable<Settlement> GetSettlements(this Regime r, Data d)
    {
        return r.GetCells(d).Where(c => c.HasSettlement(d))
            .Select(c => c.GetSettlement(d));
    }
    public static Color GetUnitColor(this Regime r)
    {
        return r.PrimaryColor
            .Interpolate(Colors.Black, .25f);
    }
    public static Color GetMapColor(this Regime r)
    {
        return r.PrimaryColor
            .Interpolate(Colors.Gray, .25f);
    }
    public static IEnumerable<Unit> GetUnits(this Regime r, Data d)
    {
        return d.Military.UnitAux.UnitByRegime[r] ?? ImmutableArray<Unit>.Empty;
    }
    public static IEnumerable<Army> GetArmies(this Regime r, Data d)
    {
        return d.GetAll<Army>().Where(a => a.Regime.RefId == r.Id).ToImmutableArray();
    }
    public static IEnumerable<UnitTemplate> GetUnitTemplates(this Regime r, Data d)
    {
        return d.Military.UnitAux.UnitTemplates[r];
    }
    public static bool IsPlayerRegime(this Regime r, Data data)
    {
        return data.BaseDomain.PlayerAux.ByRegime.Contains(r);
    }
    public static bool IsLocalPlayerRegime(this Regime r, Data data)
    {
        return data.BaseDomain.PlayerAux.LocalPlayer.Regime.Get(data) == r;
    }
    public static Player GetPlayer(this Regime r, Data data)
    {
        return data.BaseDomain.PlayerAux.ByRegime[r];
    }

    public static Dictionary<LaborComponent, float> GetProds(this Regime r, Data d)
    {
        var cells = r.GetCells(d);
        var res = new Dictionary<LaborComponent, float>();
        
        foreach (var c in cells)
        {
            var foodProd = c.FoodProd.Nums.GetEnumModel(d);
            foreach (var (key, value) in foodProd)
            {
                res.AddOrSum(key.Labor, value);
            }

            if (c.GetResourceDeposit(d) is ResourceDeposit rd
                && rd.Extraction.Fulfilled())
            {
                res.AddOrSum(c.GetResourceDeposit(d).Extraction.Get(d).Labor, 1f);
            }

            if (c.HasSettlement(d))
            {
                var s = c.GetSettlement(d);
                foreach (var (key, value) in s.Buildings.GetEnumModel(d))
                {
                    res.AddOrSum(key.Labor, value);
                }
            }
        }

        return res;
    }
    public static IEnumerable<LandCell> GetCells(this Regime r, Data d)
    {
        return d.Planet.MapAux.CellHolder.Cells.Values
            .OfType<LandCell>()
            .Where(c => c.Controller.RefId == r.Id);
    }
    public static IEnumerable<Peep> GetPeeps(this Regime r, Data data)
    {
        return r.GetCells(data)
            .Where(p => p.HasPeep(data))
            .Select(p => p.GetPeep(data));
    }

    public static float GetPopulation(this Regime r, Data data)
    {
        return r.GetPeeps(data).Sum(p => p.Size);
    }

    public static float GetPowerScore(this Regime r, Data data)
    {
        var fromPop = r.GetPopulation(data);
        var fromIndustry = r.Stock.Stock.Get(data.Models.Items.IndustrialPower);
        return fromPop + fromIndustry;
    }

    
}

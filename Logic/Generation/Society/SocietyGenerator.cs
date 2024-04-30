using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;
using System.Linq;
using System.Threading.Tasks;
using Priority_Queue;

public class SocietyGenerator : Generator
{
    private GenWriteKey _key;
    private GenData _data;
    public override GenReport Generate(GenWriteKey key)
    {
        var report = new GenReport("Society");
        _key = key;
        _data = key.GenData;
        foreach (var c in _data.Planet.MapAux.CellHolder.Cells.Values.OfType<LandCell>())
        {
            Peep.Create(c, key);
        }
        foreach (var r in _data.GetAll<Regime>())
        {
            GenerateForRegime(r);
        }
        NameSettlements();
        Deforest();
        CreateUnits(key);
        return report;
    }
    
    private void GenerateForRegime(Regime r)
    {
        var popSurplus = GenerateFoodProducers(r);
        if (popSurplus <= 0) return;
        
        float score(LandCell p)
        {
            var s = (p.GetPeep(_data).Size + CellHabitability(p));
            return s;
        }
        
        popSurplus = GenerateExtractionBuildings(popSurplus, r);
        
        CreateSettlements(r, popSurplus);
    }

    private float GenerateFoodProducers(Regime r)
    {
        var developmentScale = _data.GenMultiSettings.SocietySettings.DevelopmentScale.Value;
        var foodConsPerPeep = _data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var territory = r.GetCells(_data).OfType<LandCell>();
        var foodSurplus = new ConcurrentBag<float>();
        
        foreach (var foodProdTechnique in _data.Models.GetModels<FoodProdTechnique>().Values)
        {
            makeFoodProdTechnique(foodProdTechnique);
        }
        
        void makeFoodProdTechnique(FoodProdTechnique technique)
        {
            var buildingSurplus = technique.BaseProd - technique.BaseLabor * foodConsPerPeep;
            Parallel.ForEach(territory, p =>
            {
                var numBuilding = technique.NumForCell(p, _data) * developmentScale;
                if (numBuilding == 0) return;
                foodSurplus.Add(buildingSurplus * numBuilding);
                p.GetPeep(_key.Data)
                    .GrowSize(Mathf.CeilToInt(technique.BaseLabor * numBuilding), _key);
                p.FoodProd.Add(technique, numBuilding);
            });
        }
        
        return foodSurplus.Sum() / foodConsPerPeep;
    }

    private float PolyHabitability(MapPolygon poly)
    {
        return poly.GetCells(_data).OfType<LandCell>()
            .Sum(CellHabitability) / poly.GetCells(_data).Count;
    }
    
    private float CellHabitability(LandCell cell)
    {
        var score = 2f * (cell.Vegetation.Get(_data).MinMoisture
                          + (1f - cell.Landform.Get(_data).MinRoughness * .5f));
        if (cell.GetNeighbors(_key.Data)
            .Any(e => e is LandCell l 
                && l.Polygon.RefId != cell.Polygon.RefId
                && l.Polygon.Get(_data).GetEdge(cell.Polygon.Get(_data), _data).IsRiver()))
        {
            score *= 1.5f;
        }
        if (cell.GetNeighbors(_key.Data)
            .Any(n => n is SeaCell))
        {
            score *= 1.5f;
        }
        return score;
    }
    private float GenerateExtractionBuildings(float popSurplus, Regime r)
    {
        var developmentScale = _data.GenMultiSettings.SocietySettings.DevelopmentScale.Value;
        var cells = r.GetCells(_data);
        var extractionBuildings = _data.Models.ResourceExtractions.GetList();
        foreach (var cell in cells)
        {
            if (_data.Planet.ResourceDepositAux.ByCell[cell] is ResourceDeposit rd)
            {
                var rand = Game.I.Random.Randf();
                if (rand > developmentScale) continue;
                var item = rd.Item.Get(_data);
                if (extractionBuildings.FirstOrDefault(b => b.Resource == item)
                    is ResourceExtractionBuilding xb)
                {
                    rd.SetExtraction(xb.MakeRef());
                    cell.GetPeep(_data).GrowSize(xb.BaseLabor, _key);
                    popSurplus -= xb.BaseLabor;
                }
            }
        }
        return popSurplus;
    }

    private void CreateSettlements(Regime r, float popSurplus)
    {
        var minSize = _data.Models.Settlements.TiersBySize.First().MinSize;
        var rPolysByHabitability = _data.GetAll<MapPolygon>()
            .Where(p => p.GetCells(_data).First() is LandCell landCell
                        && landCell.Controller.RefId == r.Id)
            .OrderByDescending(PolyHabitability).ToArray();

        var baseNum = Mathf.CeilToInt(
            Mathf.Min(2f * popSurplus / (minSize), 
                rPolysByHabitability.Length)
            );
        var decayMult = .5f;
        var weights = new Dictionary<MapPolygon, int>();

        var num = baseNum;
        while (num > 0)
        {
            for (var i = 0; i < num; i++)
            {
                var poly = rPolysByHabitability[i];
                weights.AddOrSum(poly, 1);
            }

            num = Mathf.FloorToInt(num * decayMult);
        }

        var totalWeight = weights.Values.Sum();
        var popPerWeight = popSurplus / totalWeight;
        
        foreach (var (poly, weight) in weights)
        {
            var urban = poly.GetCells(_data).OfType<LandCell>()
                .MaxBy(CellHabitability);
            var pop = Mathf.CeilToInt(weight * popPerWeight);
            if (pop < minSize)
            {
                GD.Print("skipped");
                continue;
            }
            urban.SetLandform(_data.Models.Landforms.Urban, _key);
            urban.SetVegetation(_data.Models.Vegetations.Barren, _key);
            urban.GetPeep(_data).GrowSize(pop, _key);
            Settlement.Create("", urban, pop, _key);
        }
    }
    
    private void Deforest()
    {
        var polys = _data.GetAll<MapPolygon>();
        var forest = _data.Models.Vegetations.Forest;
        var jungle = _data.Models.Vegetations.Jungle;
        var grassland = _data.Models.Vegetations.Grassland;
        var tundra = _data.Models.Vegetations.Tundra;
        foreach (var poly in polys)
        {
            if (poly.IsWater()) continue;
            var beneath = grassland;
            if (poly.DistFromEquatorRatio(_data) >= tundra.MinDistFromEquatorRatio)
            {
                beneath = tundra;
            }
            
            var choppableCells = poly.GetCells(_data)
                .Where(t => t.GetVegetation(_data) == forest
                    || t.GetVegetation(_data) == jungle);
            float deforestStr = 0f;
            if (poly.GetCells(_data).Any(c => c.HasSettlement(_data)))
            {
                deforestStr = .25f;
            }
            else continue;
            foreach (var cell in choppableCells)
            {
                var sample = Game.I.Random.Randf();
                if (sample < deforestStr)
                {
                    cell.SetVegetation(beneath, _key);
                }
            }
        }
        
    }
    
    private void NameSettlements()
    {
        var taken = new HashSet<string>();
        foreach (var r in _data.GetAll<Regime>())
        {
            var settlements = r.GetCells(_data)
                .Where(p => p.HasSettlement(_data))
                .Select(p => p.GetSettlement(_data));
            var names = r.Culture.Get(_data)
                .SettlementNames.Where(n => taken.Contains(n) == false)
                .ToList();
            if (settlements.Count() > names.Count) continue;
            int iter = 0;
            foreach (var settlement in settlements)
            {
                taken.Add(names[iter]);
                settlement.SetName(names[iter], _key);
                iter++;
            }
        }
    }
    
    private static void CreateUnits(GenWriteKey key)
    {
        foreach (var regime in key.Data.GetAll<Regime>())
        {
            var template = regime.GetUnitTemplates(key.Data)
                .First();

            var score = Mathf.CeilToInt(Mathf.Sqrt(regime.GetCells(key.Data).Count()));
            var numUnits = score * 2;

            var cells = regime
                .GetCells(key.Data)
                .Where(p => p.HasPeep(key.Data));
            
            var numCells = cells.Count();
            var numToDistributeIn = numCells / 3;
            numCells = Mathf.Max(numToDistributeIn, 1);
            var distributeInPolys = regime
                .GetCells(key.Data)
                .OrderByDescending(p => p.GetPeep(key.Data).Size)
                .Take(numCells).ToArray();
            for (var i = 0; i < numUnits; i++)
            {
                var cell = distributeInPolys.Modulo(i);
                var unitPos = new MapPos(cell.Id, (-1, 0f));
                Unit.Create(template, regime, unitPos.Copy(), key);
            }
        }
    }
}
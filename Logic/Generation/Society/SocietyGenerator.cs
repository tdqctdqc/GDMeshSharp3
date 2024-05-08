using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using System.Linq;
using System.Threading.Tasks;
using Priority_Queue;

public class SocietyGenerator : Generator
{
    private GenWriteKey _key;
    private GenData _data;
    private MultiTimer _times;
    public override GenReport Generate(GenWriteKey key)
    {
        var report = new GenReport("Society");
        _key = key;
        _data = key.GenData;
        _times = new MultiTimer();
        _times.AddName("food");
        _times.AddName("extraction");
        _times.AddName("settlements");
        _times.AddName("deforest");
        _times.AddName("units");
        _times.AddName("settlement buildings");
        
        foreach (var c in _data.Planet.MapAux.CellHolder.Cells.Values.OfType<LandCell>())
        {
            Peep.Create(c, key);
        }

        var regimes = _data.GetAll<Regime>();
        Parallel.ForEach(regimes, r => GenerateForRegime(r));
        var settlements = regimes.AsParallel()
            .SelectMany(r => GenerateForRegime(r))
            .ToArray();
        foreach (var (cell, size) in settlements)
        {
            Settlement.Create("", cell, size, _key);
        }
        // NameSettlements();

        _times.RunAndTime(() =>
        {
            Parallel.ForEach(regimes, r =>
            {
                MakeSettlementBuildings(r);
            });
        }, "settlement buildings");
        
        
        // _times.RunAndTime(Deforest, "deforest");
        _times.RunAndTime(() => CreateUnits(key), "units");
        
        
        
        _times.Print();

        return report;
    }
    
    private List<(LandCell, int)> GenerateForRegime(Regime r)
    {
        var popSurplus = _times.RunAndTime(
            () => GenerateFoodProducers(r), 
            "food");
        if (popSurplus <= 0) return new List<(LandCell, int)>();

        popSurplus = _times.RunAndTime(
            () => GenerateExtractionBuildings(popSurplus, r),
            "extraction");
        
        return _times.RunAndTime(() => CreateSettlements(r, popSurplus), "settlements");
    }

    private float GenerateFoodProducers(Regime r)
    {
        var developmentScale = _data.GenMultiSettings.SocietySettings.DevelopmentScale.Value;
        var foodConsPerPeep = _data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var territory = r.GetCells(_data)
            .OfType<LandCell>()
            .ToArray();
        var foodSurplus = 0f;
        var techniques = _data.Models.GetModels<FoodProdTechnique>().Values.ToArray();
        
        for (var i = 0; i < territory.Length; i++)
        {
            var p = territory[i];
            var peep = p.GetPeep(_data);
            var foodProd = p.FoodProd;
            var peepIncrease = 0;
            for (var j = 0; j < techniques.Length; j++)
            {
                var technique = techniques[j];
                var buildingSurplus = technique.BaseProd - technique.BaseLabor * foodConsPerPeep;
                var numBuilding = technique.NumForCell(p, _data) * developmentScale;
                foodSurplus += buildingSurplus * numBuilding;
                peepIncrease += Mathf.CeilToInt(technique.BaseLabor * numBuilding);
                foodProd.Add(technique, numBuilding);
            }
            peep.GrowSize(peepIncrease, _key);
        }
        
        return foodSurplus / foodConsPerPeep;
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
        return score;
        
        //
        //
        //
        //
        // if (cell.GetNeighbors(_key.Data)
        //     .Any(e => e is LandCell l 
        //         && l.Polygon.RefId != cell.Polygon.RefId
        //         && l.Polygon.Get(_data).GetEdge(cell.Polygon.Get(_data), _data).IsRiver()))
        // {
        //     score *= 1.5f;
        // }
        // if (cell.GetNeighbors(_key.Data)
        //     .Any(n => n is SeaCell))
        // {
        //     score *= 1.5f;
        // }
        // return score;
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

    private List<(LandCell, int)> CreateSettlements(Regime r, float popSurplus)
    {
        var res = new List<(LandCell, int)>();
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
            var cells = poly.GetCells(_data).OfType<LandCell>();
            var first = cells.MaxBy(CellHabitability);
            
            var cellsByPriority = poly.GetCells(_data).OfType<LandCell>()
                .OrderBy(c => c.GetCenter()
                    .Offset(first.GetCenter(), _data).Length());
            var queue = new Queue<LandCell>(cellsByPriority);
            var carryRatio = .15f;
            var pop = Mathf.CeilToInt(weight * popPerWeight);
            
            while (pop * carryRatio >= minSize && queue.Count > 1)
            {
                var urban = queue.Dequeue();
                var forThis = Mathf.CeilToInt(pop * (1f - carryRatio));
                var forNext = pop - forThis;
                urban.SetLandform(_data.Models.Landforms.Urban, _key);
                urban.SetVegetation(_data.Models.Vegetations.Barren, _key);
                urban.GetPeep(_data).GrowSize(forThis, _key);
                res.Add((urban, forThis));
                pop = forNext;
            }

            var last = queue.Dequeue();
            last.SetLandform(_data.Models.Landforms.Urban, _key);
            last.SetVegetation(_data.Models.Vegetations.Barren, _key);
            last.GetPeep(_data).GrowSize(pop, _key);
            res.Add((last, pop));
        }

        return res;
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

    private void MakeSettlementBuildings(Regime r)
    {
        // var settlements = _data.GetAll<Settlement>()
        //     .Where(s => s.Cell.Get(_data).Controller.RefId == r.Id);
        // var factory = _data.Models.Buildings.Factory;
        // var barracks = _data.Models.Buildings.Barracks;
        //
        // var pattern = new SettlementBuildingModel[]
        //     { factory, factory,
        //         factory, barracks };
        //
        //
        // var labor = _data.Models.Flows.Labor;
        // foreach (var settlement in settlements)
        // {
        //     var cell = (LandCell)settlement.Cell.Get(_data);
        //     var foodLabor = cell.FoodProd.Nums
        //         .GetEnumerableModel(_data)
        //         .Sum(v => v.Key.BaseLabor * v.Value);
        //     var freeLabor = cell.GetPeep(_data).Size - foodLabor;
        //     var numBs = 0;
        //     while (freeLabor > 0)
        //     {
        //         var b = pattern[numBs % pattern.Length];
        //         settlement.Buildings.Add(b, 1);
        //         freeLabor -= b.GetComponent<BuildingProd>()
        //             .Inputs.Get(labor);
        //         numBs++;
        //     }
        // }
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
            var cells = regime
                .GetCells(key.Data)
                .ToArray();

            var score = Mathf.CeilToInt(Mathf.Sqrt(cells.Length));
            var numUnits = score * 2;
            
            var numCells = cells.Length;
            var numToDistributeIn = numCells / 3;
            numCells = Mathf.Max(numToDistributeIn, 1);
            var distributeIn = cells
                .OrderByDescending(p => p.GetPeep(key.Data).Size)
                .ToArray();
            for (var i = 0; i < numUnits; i++)
            {
                var cell = distributeIn.Modulo(i);
                var unitPos = new MapPos(cell.Id, (-1, 0f));
                Unit.Create(template, regime, unitPos, key);
            }
        }
    }
}
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
        var unemployedRatio = .2f;
        var margin = .2f;
        var employed = popSurplus * (1f - (unemployedRatio + margin));
        if (popSurplus <= 0) return;
        
        float score(LandCell p)
        {
            var s = (p.GetPeep(_data).Size + CellHabitability(p));
            return s;
        }
        
        var extractionLabor = GenerateExtractionBuildings(r);
        var surplus = employed - (extractionLabor);
        var forFactories = surplus * .9f;
        var forBanks = surplus * .1f;

        GenerateWorkBuildingType(_key.Data.Models.Buildings.Factory, r, forFactories, score);
        GenerateWorkBuildingType(_key.Data.Models.Buildings.Bank, r, forBanks, score);
        GenerateLaborers(r, employed);
        
        GenerateUnemployed(r, Mathf.FloorToInt(popSurplus * unemployedRatio));
        
        CreateSettlements(r);
        GenerateNonWorkBuildingType(_key.Data.Models.Buildings.Barracks, 
            r, .02f);
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
    private float GenerateExtractionBuildings(Regime r)
    {
        return 0f;
    }
    private float GenerateTownHalls(Regime r, HashSet<MapPolygon> settlementPolys)
    {
        var townHall = _data.Models.Buildings.TownHall;
        
        
        
        foreach (var p in settlementPolys)
        {
            var cell = p.GetCells(_key.Data)
                .Where(c => c.HasBuilding(_key.Data) == false)
                .FirstOrDefault();
            if (cell is not null)
            {
                MapBuilding.CreateGen(cell, townHall, _key);
            }
        }

        return townHall.GetComponent<BuildingProd>().Inputs.Get(_data.Models.Flows.Labor) * settlementPolys.Count();
    }
    
    private void GenerateWorkBuildingType(BuildingModel model, 
        Regime r, float popBudget,
        Func<LandCell, float> suitability)
    {
        if (popBudget <= 0) return;
        var cells = r.GetCells(_data)
            .OfType<LandCell>()
            .Where(c =>
                c.HasBuilding(_data) == false
                    && model.CanBuildInCell(c, _key.Data))
            .OrderBy(c => Game.I.Random.Randf())
            .ToList();
        var portions = Apportioner
            .ApportionLinear(popBudget, cells, suitability);
        var laborReq = model.GetComponent<BuildingProd>().Inputs.Get(_data.Models.Flows.Labor);
        var num = Mathf.FloorToInt(popBudget / laborReq);
        num = Mathf.Min(cells.Count - 1, num);
        for (var i = 0; i < num; i++)
        {
            MapBuilding.CreateGen(cells[i], model, _key);
        }
    }
    private void GenerateNonWorkBuildingType(BuildingModel model, Regime r,
        float buildChance)
    {
        var cells = r.GetCells(_data)
            .Where(p => p.HasBuilding(_data) == false
                && model.CanBuildInCell(p, _key.Data))
            .ToList();
        
        for (var i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            var chance = Game.I.Random.Randf();
            if (chance > buildChance) continue;
            MapBuilding.CreateGen(cell, model, _key);
        }
    }
    private void GenerateLaborers(Regime r, float popSurplus)
    {
        if (popSurplus <= 0) return;
        var cells = r.GetCells(_data).ToList();
        var laborDesire = 0;
        foreach (var c in cells)
        {
            if (c.HasBuilding(_data) == false) continue;
            var building = c.GetBuilding(_data);
            
            laborDesire += (int)building.Model.Get(_data).GetComponent<BuildingProd>().Inputs.Get(_data.Models.Flows.Labor);
        }
        var laborRatio = Mathf.Min(1f, popSurplus / laborDesire);
        if (laborRatio == 0) return;
        foreach (var c in cells)
        {
            if (c.HasBuilding(_data) == false) continue;
            var building = c.GetBuilding(_data);
            var peep = c.GetPeep(_data);
            var laborReq = (int)building.Model.Get(_data).GetComponent<BuildingProd>().Inputs.Get(_data.Models.Flows.Labor);
            peep.GrowSize((int)(laborReq * laborRatio), _key);
        }
    }

    private void GenerateUnemployed(Regime r, int pop)
    {
        var cells = r.GetCells(_data).ToList();
        var portions = Apportioner.ApportionLinear(pop, cells, 
            p => p.GetPeep(_data).Size);
        for (var i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            var peep = cell.GetPeep(_data);
            var cellUnemployed = portions[i];
            if (cellUnemployed <= 0f) continue;
            peep.GrowSize(cellUnemployed, _key);
        }
    }

    private void CreateSettlements(Regime r)
    {
        var minSize = _data.Models.Settlements.TiersBySize.First().MinSize;
        foreach (var p in r.GetCells(_data))
        {
            var settlementSize = p.GetPeep(_data).Size;
            if (settlementSize < minSize) continue;
            Settlement.Create("", p, settlementSize, _key);
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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Google.OrTools.ConstraintSolver;

public class PeepGenerator : Generator
{
    private GenData _data;
    private IdDispenser _id;
    private GenWriteKey _key;
    public PeepGenerator()
    {
        
    }

    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        _id = key.IdDispenser;
        _data = key.GenData;
        var report = new GenReport(GetType().Name);
        
        report.StartSection();
        
        foreach (var p in _data.Planet.Polygons.Entities)
        {
            if(p.IsLand) PolyPeep.Create(p, key);
        }
        
        foreach (var r in _data.Society.Regimes.Entities)
        {
            GenerateForRegime(r);
        }
        _data.Notices.PopulatedWorld.Invoke();
        report.StopSection("All");

        return report;
    }

    private void GenerateForRegime(Regime r)
    {
        var popSurplus = GenerateFoodProducers(r);
        var unemployedRatio = .2f;
        var margin = 0f;
        var employed = popSurplus * (1f - (unemployedRatio + margin));
        if (popSurplus <= 0) return;
        var extractionLabor = GenerateExtractionBuildings(r);
        var adminLabor = GenerateTownHalls(r);
        var forFactories = (employed - (extractionLabor + adminLabor));
        GenerateFactories(r, forFactories);
        GenerateLaborers(r, employed);
        GenerateUnemployed(r, Mathf.FloorToInt(popSurplus * unemployedRatio));
    }

    private float GenerateFoodProducers(Regime r)
    {
        var developmentRatio = .5f;
        var foodConsPerPeep = _data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var territory = r.Polygons.Entities();
        var foodSurplus = new ConcurrentBag<float>();
        makeFoodProd(FoodProdTechniqueManager.Farm);
        makeFoodProd(FoodProdTechniqueManager.Ranch);
        
        void makeFoodProd(FoodProdTechnique technique)
        {
            var buildingSurplus = technique.BaseProd - technique.BaseLabor * foodConsPerPeep;
            Parallel.ForEach(territory, p =>
            {
                var tris = p.Tris.Tris;
                var numBuilding = Mathf.FloorToInt(technique.NumForPoly(p) * developmentRatio);
                if (numBuilding == 0) return;
                foodSurplus.Add(buildingSurplus * numBuilding);
                p.GetPeep(_key.Data)
                    .GrowSize(technique.BaseLabor * numBuilding, _key);
                p.PolyFoodProd.Add(technique, numBuilding);
            });
        }
        
        return foodSurplus.Sum() / foodConsPerPeep;
    }
    
    private float GenerateExtractionBuildings(Regime r)
    {
        var extractBuildings = _data.Models.Buildings.Models
            .Values.SelectWhereOfType<BuildingModel, ExtractionBuildingModel>()
            .ToDictionary(m => m.ProdItem, m => m);
        
        var polyBuildings = new Dictionary<MapPolygon, List<ExtractionBuildingModel>>();
        
        foreach (var p in r.Polygons)
        {
            if (p.GetResourceDeposits(_data) is IEnumerable<ResourceDeposit> rds == false)
            {
                continue;
            }
            var extractSlots = p.PolyBuildingSlots[BuildingType.Extraction];
            polyBuildings.Add(p, new List<ExtractionBuildingModel>());

            foreach (var rd in rds)
            {
                if (extractSlots < 1) break;
                if (extractBuildings.ContainsKey(rd.Item.Model()) == false) continue;
                extractSlots--;
                var b = extractBuildings[rd.Item.Model()];
                polyBuildings[p].Add(b);
            }
        }

        var laborDemand = 0f;
        foreach (var kvp in polyBuildings)
        {
            var poly = kvp.Key;
            foreach (var model in kvp.Value)
            {
                laborDemand += model.JobLaborReqs.Sum(kvp2 => kvp2.Value);
                MapBuilding.CreateGen(poly, model, _key);
            }
        }

        return laborDemand;
    }
    private float GenerateTownHalls(Regime r)
    {
        var townHall = BuildingModelManager.TownHall;
        var settlements = r.Polygons.Where(p => p.HasSettlement(_data))
            .Select(p => p.GetSettlement(_data));
        foreach (var s in settlements)
        {
            var p = s.Poly.Entity();
            MapBuilding.CreateGen(p, townHall, _key);
        }

        return townHall.TotalLaborReq() * settlements.Count();
    }
    private void GenerateFactories(Regime r, float popBudget)
    {
        if (popBudget <= 0) return;
        var factory = BuildingModelManager.Factory;

        var polys = r.Polygons.Entities().Where(p => factory.CanBuildInPoly(p, _key.Data)).ToList();
        var portions = Apportioner.ApportionLinear(popBudget, polys,
            p =>
            {
                return Mathf.Max(0f, p.Moisture - p.Roughness);
                // var ps = p.GetPeeps(_data);
                // if (ps == null) return 0f;
                // return p.GetPeeps(_data).Sum(x => x.Size);
            }
        );
        var factoryLaborReq = factory.TotalLaborReq();
        for (var i = 0; i < polys.Count; i++)
        {
            var p = polys[i];
            var pop = portions[i];
            var numFactories = Mathf.Round(pop / factoryLaborReq);
            numFactories = Mathf.Min(p.PolyBuildingSlots[BuildingType.Industry], numFactories);
            
            for (var j = 0; j < numFactories; j++)
            {
                MapBuilding.CreateGen(p, factory, _key);
            }
        }
    }
    private void GenerateLaborers(Regime r, float popSurplus)
    {
        if (popSurplus <= 0) return;
        var polys = r.Polygons.Entities().ToList();
        var laborDesire = 0;
        foreach (var p in r.Polygons)
        {
            var buildings = p.GetBuildings(_data);
            if (buildings == null) continue;
            
            var laborBuildings = buildings
                .Select(b => b.Model.Model())
                .SelectWhereOfType<BuildingModel, WorkBuildingModel>();
            if (laborBuildings.Count() > 0)
            {
                laborDesire += laborBuildings.Sum(lb => lb.TotalLaborReq());
            }
        }
        var laborRatio = Mathf.Min(1f, popSurplus / laborDesire);
        if (laborRatio == 0) return;
        foreach (var p in r.Polygons)
        {
            var buildings = p.GetBuildings(_data);
            if (buildings == null) continue;
            var workBuildings = buildings
                .Select(b => b.Model.Model())
                .SelectWhereOfType<BuildingModel, WorkBuildingModel>();
            var peep = p.GetPeep(_data);
            foreach (var wb in workBuildings)
            {
                foreach (var laborReq in wb.JobLaborReqs)
                {
                    var num = Mathf.FloorToInt(laborReq.Value * laborRatio);
                    peep.GrowSize(num, _key);
                }
            }
        }
    }

    private void GenerateUnemployed(Regime r, int pop)
    {
        var settlementPolys = r.Polygons.Entities()
            .Where(p => p.HasSettlement(_data))
            .ToList();
        var portions = Apportioner.ApportionLinear(pop, settlementPolys, 
            p => 1);
        for (var i = 0; i < settlementPolys.Count; i++)
        {
            var poly = settlementPolys[i];
            var peep = poly.GetPeep(_data);
            var polyUnemployed = portions[i];
            peep.GrowSize(polyUnemployed, _key);
        }
    }
    
}
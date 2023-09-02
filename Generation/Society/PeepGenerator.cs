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
    private GenWriteKey _key;
    public PeepGenerator()
    {
        
    }

    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        _data = key.GenData;
        var report = new GenReport(GetType().Name);
        
        report.StartSection();
        
        foreach (var p in _data.GetAll<MapPolygon>())
        {
            if(p.IsLand) Peep.Create(p, key);
        }
        
        foreach (var r in _data.GetAll<Regime>())
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
        var surplus = (employed - (extractionLabor + adminLabor));
        var forFactories = surplus * .75f;
        var forBanks = surplus * .25f;
        GenerateBuildingType(_key.Data.Models.Buildings.Factory, r, forFactories,
            p => Mathf.Max(0f, p.Moisture - p.Roughness));
        GenerateBuildingType(_key.Data.Models.Buildings.Bank, r, forBanks,
            p => Mathf.Max(0f, 
                p.HasSettlement(_key.Data) 
            ? 1f//p.GetSettlement(_key.Data).Size
            : 0f));
        GenerateLaborers(r, employed);
        GenerateUnemployed(r, Mathf.FloorToInt(popSurplus * unemployedRatio));
    }

    private float GenerateFoodProducers(Regime r)
    {
        var developmentScale = _data.GenMultiSettings.SocietySettings.DevelopmentScale.Value;
        var foodConsPerPeep = _data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var territory = r.GetPolys(_data);
        var foodSurplus = new ConcurrentBag<float>();
        
        foreach (var foodProdTechnique in _data.Models.GetModels<FoodProdTechnique>().Values)
        {
            makeFoodProd(foodProdTechnique);
        }
        
        void makeFoodProd(FoodProdTechnique technique)
        {
            var buildingSurplus = technique.BaseProd - technique.BaseLabor * foodConsPerPeep;
            Parallel.ForEach(territory, p =>
            {
                var tris = p.Tris.Tris;
                var numBuilding = Mathf
                    .RoundToInt(technique.NumForPoly(p, _data) * developmentScale);
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
            
        var t = _data.Models.GetManager<BuildingModel>().Models
            .Where(kvp => kvp.Value.GetComponent<ExtractionProd>() != null);
        var extractBuildings = new System.Collections.Generic.Dictionary<Item, List<BuildingModel>>();

        foreach (var kvp in t)
        {
            var model = kvp.Value;
            var comps = model.Components.SelectWhereOfType<ExtractionProd>();
            foreach (var extractionProd in comps)
            {
                extractBuildings.AddOrUpdate(extractionProd.ProdItem, model);
            }
        }

        var polyBuildings = new System.Collections.Generic.Dictionary<MapPolygon, List<BuildingModel>>();
        
        foreach (var p in r.GetPolys(_data))
        {
            if (p.GetResourceDeposits(_data) is IEnumerable<ResourceDeposit> rds == false)
            {
                continue;
            }
            var extractSlots = p.PolyBuildingSlots[BuildingType.Extraction];
            polyBuildings.Add(p, new List<BuildingModel>());

            foreach (var rd in rds)
            {
                if (extractSlots < 1) break;
                if (extractBuildings.ContainsKey(rd.Item.Model(_data)) == false) continue;
                extractSlots--;
                var b = extractBuildings[rd.Item.Model(_data)];
                polyBuildings[p].Add(b.First());
            }
        }

        var laborDemand = 0f;
        foreach (var kvp in polyBuildings)
        {
            var poly = kvp.Key;
            foreach (var model in kvp.Value)
            {
                var w = model.GetComponent<Workplace>();
                laborDemand += w.JobLaborReqs.Sum(kvp2 => kvp2.Value);
                MapBuilding.CreateGen(poly, poly.GetCenterWaypoint(_key.Data).Id, model, _key);
            }
        }

        return laborDemand;
    }
    private float GenerateTownHalls(Regime r)
    {
        var townHall = _data.Models.Buildings.TownHall;
        var settlements = r.GetPolys(_data).Where(p => p.HasSettlement(_data))
            .Select(p => p.GetSettlement(_data));
        foreach (var s in settlements)
        {
            var p = s.Poly.Entity(_data);
            MapBuilding.CreateGen(p, p.GetCenterWaypoint(_key.Data).Id, townHall, _key);
        }

        return townHall.GetComponent<Workplace>().TotalLaborReq() * settlements.Count();
    }
    // private void GenerateFactories(Regime r, float popBudget)
    // {
    //     if (popBudget <= 0) return;
    //     var factory = _data.Models.Buildings.Factory;
    //
    //     var polys = r.GetPolys(_data).Where(p => factory.CanBuildInPoly(p, _key.Data)).ToList();
    //     var portions = Apportioner.ApportionLinear(popBudget, polys,
    //         p =>
    //         {
    //             return Mathf.Max(0f, p.Moisture - p.Roughness);
    //             // var ps = p.GetPeeps(_data);
    //             // if (ps == null) return 0f;
    //             // return p.GetPeeps(_data).Sum(x => x.Size);
    //         }
    //     );
    //     var factoryLaborReq = factory.GetComponent<Workplace>().TotalLaborReq();
    //     for (var i = 0; i < polys.Count; i++)
    //     {
    //         var p = polys[i];
    //         var pop = portions[i];
    //         var numFactories = Mathf.Round(pop / factoryLaborReq);
    //         numFactories = Mathf.Min(p.PolyBuildingSlots[BuildingType.Industry], numFactories);
    //         
    //         for (var j = 0; j < numFactories; j++)
    //         {
    //             MapBuilding.CreateGen(p, p.GetCenterWaypoint(_key.Data).Id, factory, _key);
    //         }
    //     }
    // }
    
    private void GenerateBuildingType(BuildingModel model, Regime r, float popBudget,
        Func<MapPolygon, float> suitability)
    {
        if (popBudget <= 0) return;

        var polys = r.GetPolys(_data)
            .Where(p => model.CanBuildInPoly(p, _key.Data)).ToList();
        var portions = Apportioner.ApportionLinear(popBudget, polys, suitability);
        var laborReq = model.GetComponent<Workplace>().TotalLaborReq();
        for (var i = 0; i < polys.Count; i++)
        {
            var p = polys[i];
            var pop = portions[i];
            var num = Mathf.Round(pop / laborReq);
            num = Mathf.Min(p.PolyBuildingSlots[model.BuildingType], num);
            
            for (var j = 0; j < num; j++)
            {
                MapBuilding.CreateGen(p, p.GetCenterWaypoint(_key.Data).Id, model, _key);
            }
        }
    }
    
    private void GenerateLaborers(Regime r, float popSurplus)
    {
        if (popSurplus <= 0) return;
        var polys = r.GetPolys(_data).ToList();
        var laborDesire = 0;
        foreach (var p in polys)
        {
            var buildings = p.GetBuildings(_data);
            if (buildings == null) continue;
            
            var laborBuildings = buildings
                .Select(b => b.Model.Model(_data))
                .SelectWhere(b => b.GetComponent<Workplace>() != null);
            if (laborBuildings.Count() > 0)
            {
                laborDesire += laborBuildings.Sum(b => b.GetComponent<Workplace>().TotalLaborReq());
            }
        }
        var laborRatio = Mathf.Min(1f, popSurplus / laborDesire);
        if (laborRatio == 0) return;
        foreach (var p in polys)
        {
            var buildings = p.GetBuildings(_data);
            if (buildings == null) continue;
            var workBuildings = buildings
                .Select(b => b.Model.Model(_data))
                .SelectWhere(b => b.HasComponent<Workplace>());
            var peep = p.GetPeep(_data);
            foreach (var wb in workBuildings)
            {
                foreach (var laborReq in wb.GetComponent<Workplace>().JobLaborReqs)
                {
                    var num = Mathf.FloorToInt(laborReq.Value * laborRatio);
                    peep.GrowSize(num, _key);
                }
            }
        }
    }

    private void GenerateUnemployed(Regime r, int pop)
    {
        var settlementPolys = r.GetPolys(_data)
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
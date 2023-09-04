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
        foreach (var p in _data.GetAll<MapPolygon>())
        {
            if(p.IsLand) Peep.Create(p, key);
        }
        foreach (var r in _data.GetAll<Regime>())
        {
            GenerateForRegime(r);
        }
        NameSettlements();
        _data.Notices.PopulatedWorld.Invoke();
        Deforest();

        return report;
    }
    
    private void GenerateForRegime(Regime r)
    {
        var popSurplus = GenerateFoodProducers(r);
        var unemployedRatio = .2f;
        var margin = 0f;
        var employed = popSurplus * (1f - (unemployedRatio + margin));
        if (popSurplus <= 0) return;

        var favored = PickFavoredSettlementPolys(r.GetPolys(_data).ToList());
        
        
        float favoredBonus(MapPolygon p)
        {
            if (favored.Contains(p)) return 10f;
            return 1f;
        }

        float score(MapPolygon p)
        {
            var s = (p.GetPeep(_data).Size + PolyHabitability(p)) * favoredBonus(p);
            return s * s * s;
        }
        
        
        var extractionLabor = GenerateExtractionBuildings(r);
        // var adminLabor = GenerateTownHalls(r, settlementPolys);
        var surplus = employed - (extractionLabor);
        var forFactories = surplus * .75f;
        var forBanks = surplus * .25f;

        
        GenerateBuildingType(_key.Data.Models.Buildings.Factory, r, forFactories, score);
        GenerateBuildingType(_key.Data.Models.Buildings.Bank, r, forBanks, score);
        GenerateLaborers(r, employed);
        
        GenerateUnemployed(r, Mathf.FloorToInt(popSurplus * unemployedRatio));
        
        CreateSettlements(r);
    }

    private float GenerateFoodProducers(Regime r)
    {
        var developmentScale = _data.GenMultiSettings.SocietySettings.DevelopmentScale.Value;
        var foodConsPerPeep = _data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var territory = r.GetPolys(_data);
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
    
    private HashSet<MapPolygon> PickFavoredSettlementPolys(List<MapPolygon> regimeUnionPolys)
    {
        float scorePerSettlement = 1f;
        var numSettlements = regimeUnionPolys.Count / 3;
        if (numSettlements == 0) numSettlements = 1;
        
        var polyQueue = new SimplePriorityQueue<MapPolygon>();
        for (var i = 0; i < regimeUnionPolys.Count; i++)
        {
            var p = regimeUnionPolys[i];
            if (p.Tris.Tris.Any(t => t.Landform(_data) != _data.Models.Landforms.Mountain
                           && t.Landform(_data) != _data.Models.Landforms.Peak
                           && t.Landform(_data) != _data.Models.Landforms.River))
            {
                polyQueue.Enqueue(p, -PolyHabitability(p));
            }
        }
        numSettlements = Math.Min(numSettlements, polyQueue.Count);
        var settlementPolys = new HashSet<MapPolygon>();
        var forbidden = new HashSet<MapPolygon>();

        for (var i = 0; i < numSettlements; i++)
        {
            if (polyQueue.Count == 0) break;
            var poly = polyQueue.Dequeue();
            if (forbidden.Contains(poly)) continue;
            foreach (var n in poly.Neighbors.Items(_data))
            {
                forbidden.Add(n);
            }
            settlementPolys.Add(poly);
        }

        return settlementPolys;
    }
    private float PolyHabitability(MapPolygon poly)
    {
        var score = 2f * (poly.Moisture + (1f - poly.Roughness * .5f));
        if (poly.Tris.Tris.Any(t => t.Landform(_data) == _data.Models.Landforms.River))
        {
            score *= 1.5f;
        }
        if (poly.IsCoast(_data))
        {
            score *= 1.5f;
        }
        return score;
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
    private float GenerateTownHalls(Regime r, HashSet<MapPolygon> settlementPolys)
    {
        var townHall = _data.Models.Buildings.TownHall;
        foreach (var p in settlementPolys)
        {
            MapBuilding.CreateGen(p, p.GetCenterWaypoint(_key.Data).Id, townHall, _key);
        }

        return townHall.GetComponent<Workplace>().TotalLaborReq() * settlementPolys.Count();
    }
    
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
        var polys = r.GetPolys(_data).ToList();
        var portions = Apportioner.ApportionLinear(pop, polys, 
            p => p.GetPeep(_data).Size);
        for (var i = 0; i < polys.Count; i++)
        {
            var poly = polys[i];
            var peep = poly.GetPeep(_data);
            var polyUnemployed = portions[i];
            peep.GrowSize(polyUnemployed, _key);
        }
    }

    private void CreateSettlements(Regime r)
    {
        var minSize = _data.Models.Settlements.TiersBySize.First().MinSize;
        var settlementPolys = new HashSet<MapPolygon>();
        foreach (var p in r.GetPolys(_data))
        {
            var settlementSize = p.GetPeep(_data).Size;
            if (settlementSize < minSize) continue;
            Settlement.Create("", p, settlementSize, _key);
            settlementPolys.Add(p);
        }
        SetUrbanTris(settlementPolys);
    }
    private void SetUrbanTris(HashSet<MapPolygon> settlementPolys)
    {
        // var sizeForTri = 500;
        
        foreach (var p in settlementPolys)
        {
            var availTris = p.Tris.Tris
                .Where(t => t.Landform(_data) != _data.Models.Landforms.River
                            && t.Landform(_data) != _data.Models.Landforms.Mountain
                            && t.Landform(_data) != _data.Models.Landforms.Peak)
                .OrderBy(t => t.GetCentroid().LengthSquared()).ToList();
            var settlement = p.GetSettlement(_data);
            var tier = settlement.Tier.Model(_data);
            var numUrbanTris = Mathf.Max(1, tier.NumTris);
            numUrbanTris = Mathf.Min(availTris.Count(), numUrbanTris);
            if (settlement.Tier.Model(_data) == _data.Models.Settlements.Village)
            {
                numUrbanTris = 1;
            }
            for (var j = 0; j < numUrbanTris; j++)
            {
                availTris[j].SetLandform(_data.Models.Landforms.Urban, _key);
                availTris[j].SetVegetation(_data.Models.Vegetations.Barren, _key);
            }
        }
    }
    private void Deforest()
    {
        var polys = _data.GetAll<MapPolygon>();
        foreach (var poly in polys)
        {
            if (poly.IsWater()) continue;
            var forestTris = poly.Tris.Tris
                .Where(t => t.Vegetation(_data) == _data.Models.Vegetations.Forest);
            float deforestStr = 0f;
            if (poly.HasSettlement(_data))
            {
                deforestStr = .25f;
            }
            else if (poly.Neighbors.Items(_data).Any(n => n.HasSettlement(_data)))
            {
                deforestStr = .05f;
            }
            else continue;
            foreach (var tri in forestTris)
            {
                var sample = Game.I.Random.Randf();
                if (sample < deforestStr)
                {
                    tri.SetVegetation(_data.Models.Vegetations.Grassland, _key);
                }
            }
        }
        
    }
    
    private void NameSettlements()
    {
        var taken = new HashSet<string>();
        foreach (var r in _data.GetAll<Regime>())
        {
            var settlements = r.GetPolys(_data).Where(p => p.HasSettlement(_data))
                .Select(p => p.GetSettlement(_data));
            var names = r.Culture.Model(_data).SettlementNames.Where(n => taken.Contains(n) == false).ToList();
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
}
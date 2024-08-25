using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class RegimeGenerator : Generator
{
    private GenData _data;
    private GenKey _key;
    private GenReport _report;
    private int _numPolysToBeMajor = 20;
    public RegimeGenerator()
    {
        
    }

    public override GenReport Generate(GenKey key)
    {
        _key = key;
        _data = key.GenData;
        _report = new GenReport(GetType().Name);
        _report.StartSection();
        var polyRegimes = GenerateRegimes();

        GenerateRegimeTroops(key, polyRegimes);

        _data.Notices.Gen.GeneratedRegimes.Invoke();
        _report.StopSection("all");


        return _report;
    }

    


    private Dictionary<MapPolygon, Regime> GenerateRegimes()
    {
        var numLandPolys = _key.Data.GetAll<MapPolygon>()
            .Count(p => p.IsLand);
        
        var polysPerRegime = Mathf.Max(30, numLandPolys / 20);
        var polyRegimes = 
            new Dictionary<MapPolygon, Regime>();
        var templates = _data.Models.GetModels<RegimeTemplate>().ToHashSet();

        
        _data.Planet.MapAux.LandSea.Landmasses.ForEach(
            lm =>
            {
                var lmRegimes 
                    = GenerateLandmassRegimes(lm.Polys, polysPerRegime, templates);
                polyRegimes.AddRange(lmRegimes);
            });
        
        foreach (var lm in _data.Planet.MapAux.LandSea.Landmasses)
        {
            ExpandRegimes(polyRegimes);
        }
        
        var remainders = _data.GetAll<MapPolygon>()
            .Where(p => p.IsLand 
                        && (polyRegimes.ContainsKey(p) == false))
            .ToHashSet();
        
        HandleRemainder(remainders, templates, polyRegimes);
        
        foreach (var (poly, regime) in polyRegimes)
        {
            foreach (var c in poly.GetCells(_key.Data).OfType<LandCell>())
            {
                c.SetController(regime, _key);
            }
        }
        
        var bySize = polyRegimes
            .SortBy(
                kvp => kvp.Value,
                kvp => kvp.Key)
            .OrderByDescending(v => v.Value.Count)
            .Select(v => v.Key)
            .ToList();
        
        for (var i = 0; i < bySize.Count; i++)
        {
            var regime = bySize[i];
            if (i < bySize.Count / 4 || i < 4)
            {
                regime.SetIsMajor(true, _key);
            }
            else
            {
                regime.SetIsMajor(false, _key);
            }
        }
        
        return polyRegimes;
    }

    
    private Dictionary<MapPolygon, Regime> GenerateLandmassRegimes(
        HashSet<MapPolygon> lm, int polysPerRegime,
        HashSet<RegimeTemplate> templates)
    {
        var res = new Dictionary<MapPolygon, Regime>();
        int numRegimes = lm.Count / polysPerRegime;
        numRegimes = Mathf.Max(1, numRegimes);
        var seeds = lm.GetDistinctRandomElements(numRegimes);
        
        for (var i = 0; i < seeds.Count; i++)
        {
            var template = templates.GetRandomElement();
            var regime = Regime.Create(seeds[i], template,
                false, _key);
            foreach (var c in seeds[i].GetCells(_data).OfType<LandCell>())
            {
                c.SetController(regime, _key);
            }
            res.Add(seeds[i], regime);
        }
        return res;
    }
    private HashSet<MapPolygon> ExpandRegimes(Dictionary<MapPolygon, Regime> polyRegimes)
    {
        var free = _key.Data.GetAll<MapPolygon>()
            .Where(p => p.IsLand)
            .Except(polyRegimes.Keys).ToHashSet();
        
        var picker = new Picker<MapPolygon>(free, p => p.Neighbors.Entities(_key.Data));
        int iter = 1;
        var agents = new Dictionary<IPickerAgent<MapPolygon>, Regime>();
        foreach (var (p, r) in polyRegimes)
        {
            var w = new AdjacencyCountPickerAgent<MapPolygon>(p, picker, iter, 
                x => x.IsLand);
            iter %= 12;
            iter += 4;
            picker.AddAgent(w);
            agents.Add(w, r);
        }
        picker.RandomAgentPick();
        
        foreach (var w in picker.Agents)
        {
            if (w.Picked.Count == 0) throw new Exception();

            var r = agents[w];
            foreach (var p in w.Picked)
            {
                if (polyRegimes.ContainsKey(p) == false)
                {
                    polyRegimes.Add(p, r);
                }
            }
        }
        
        return picker.NotTaken;
    }

    private void HandleRemainder(HashSet<MapPolygon> remainder,
        HashSet<RegimeTemplate> templates,
        Dictionary<MapPolygon, Regime> polyRegimes)
    {
        var unions = UnionFind.Find<MapPolygon, List<MapPolygon>>(
            remainder, 
            (p1, p2) => p1.IsLand == p2.IsLand,
            p => p.Neighbors.Entities(_data)
        );
        // if (unions.Count > templates.Count) throw new Exception();
        int iter = 0;
        
        foreach (var union in unions)
        {
            if (union.Count == 0) continue;
            iter++;
            var prim = ColorsExt.GetRandomColor();
            var sec = prim.Inverted();
            var template = templates.GetRandomElement();
            // templates.Remove(template);
            var regime = Regime.Create(union[0], template, false, _key);
            for (var i = 0; i < union.Count; i++)
            {
                var p = union[i];
                if (polyRegimes.ContainsKey(p)) continue;
                polyRegimes.Add(p, regime);
            }
        }
    }
    
    private void GenerateRegimeTroops(GenKey key, Dictionary<MapPolygon, Regime> polyRegimes)
    {
        foreach (var regime in key.Data.GetAll<Regime>())
        {
            var size = regime.GetCells(_key.Data).Count();
            regime.Stock.Stock.Add(key.Data.Models.Troops.Rifle1, size * 2f);
            regime.Stock.Stock.Add(key.Data.Models.Troops.Artillery1, size);
        }
    }
}
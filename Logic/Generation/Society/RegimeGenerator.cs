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
    private int _polysForRegimeAvg = 20;
    private int _numPolysToBeMajor = 20;
    public RegimeGenerator()
    {
        
    }

    public override GenReport Generate(GenKey key)
    {
        _key = key;
        _data = key.GenData;
        var report = new GenReport(GetType().Name);
        report.StartSection();
        var polyRegimes = GenerateRegimes();
        foreach (var (p, regime) in polyRegimes)
        {
            var size = regime.GetCells(_key.Data).Count();
            regime.Stock.Stock.Add(key.Data.Models.Troops.Rifle1, size * 2f);
            regime.Stock.Stock.Add(key.Data.Models.Troops.Artillery1, size);
        }

        _data.Notices.Gen.GeneratedRegimes.Invoke();

        report.StopSection("all");

        return report;
    }


    private Dictionary<MapPolygon, Regime> GenerateRegimes()
    {
        var polysPerRegime = 30;
        var polyRegimes = 
            new Dictionary<MapPolygon, Regime>();
        var templates = _data.Models.GetModels<RegimeTemplate>().ToHashSet();
        
        _data.Planet.MapAux.LandSea.Landmasses.ForEach(
            lm =>
            {
                var lmRegimes = GenerateLandmassRegimes(lm.Polys, polysPerRegime, templates);
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

    
    private Dictionary<MapPolygon, Regime> GenerateLandmassRegimes(HashSet<MapPolygon> lm, int polysPerRegime,
        HashSet<RegimeTemplate> templates)
    {
        var res = new Dictionary<MapPolygon, Regime>();
        int numRegimes = lm.Count / _polysForRegimeAvg;
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
        
        var picker = new WandererPicker(free);
        int iter = 1;
        foreach (var (p, r) in polyRegimes)
        {
            var w = new RegimeWanderer(r, p, picker, iter, _key.Data);
            iter += 2;
            iter %= 6;
            if (iter == 0) iter++;
            picker.AddWanderer(w);
        }
        picker.Pick(_data);
        
        foreach (var w in picker.Wanderers)
        {
            if (w.Picked.Count == 0) throw new Exception();

            var r = ((RegimeWanderer) w).Regime;
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
            var isMajor = union.Count >= _polysForRegimeAvg * .75;
            var regime = Regime.Create(union[0], template, isMajor, _key);
            for (var i = 0; i < union.Count; i++)
            {
                var p = union[i];
                if (polyRegimes.ContainsKey(p)) continue;
                polyRegimes.Add(p, regime);
            }
        }
    }
}
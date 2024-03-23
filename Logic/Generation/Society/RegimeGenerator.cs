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
    private GenWriteKey _key;
    private int _polysForRegimeAvg = 20;
    private int _numPolysToBeMajor = 20;
    public RegimeGenerator()
    {
        
    }

    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        _data = key.GenData;
        var report = new GenReport(GetType().Name);
        report.StartSection();
        var polyRegimes = GenerateRegimes();

        var landCells = _data.Planet.MapAux.CellHolder.Cells.Values.OfType<LandCell>();
        
        foreach (var landCell in landCells)
        {
            var r = polyRegimes[landCell.Polygon.Get(_data)];
            landCell.SetController(r, key);
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
        var templates = _data.Models.RegimeTemplates.Models.Values.ToHashSet();
        
        _data.Planet.MapAux.LandSea.Landmasses.ForEach(
            lm =>
            {
                var lmRegimes = GenerateLandmassRegimes(lm.Polys, polysPerRegime, templates);
                polyRegimes.AddRange(lmRegimes);
            });

        var remainders = new ConcurrentBag<HashSet<MapPolygon>>();
        
        foreach (var lm in _data.Planet.MapAux.LandSea.Landmasses)
        {
            var remainder = ExpandRegimes(polyRegimes);
            remainders.Add(remainder);
        }
        
        foreach (var r in remainders)
        {
            HandleRemainder(r, templates, polyRegimes);
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
            var regime = Regime.Create(seeds[i], template, false, _key);
            res.Add(seeds[i], regime);
        }
        return res;
    }
    private HashSet<MapPolygon> ExpandRegimes(Dictionary<MapPolygon, Regime> polyRegimes)
    {
        var free = _key.Data.GetAll<MapPolygon>()
            .Where(p => p.IsLand).Except(polyRegimes.Keys).ToHashSet();
        
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
            var r = ((RegimeWanderer) w).Regime;
            foreach (var p in w.Picked)
            {
                if (polyRegimes.ContainsKey(p) == false)
                {
                    polyRegimes.Add(p, r);
                }
            }
            r.SetIsMajor(w.Picked.Count >= _numPolysToBeMajor, _key);
        }
        
        return picker.NotTaken;
    }

    private void HandleRemainder(HashSet<MapPolygon> remainder,
        HashSet<RegimeTemplate> templates,
        Dictionary<MapPolygon, Regime> polyRegimes)
    {
        var unions = UnionFind.Find(
            remainder, 
            (p1, p2) => p1.IsLand == p2.IsLand,
            p => p.Neighbors.Items(_data)
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
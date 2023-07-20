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
    private IdDispenser _id;
    private GenWriteKey _key;
    private int _polysForRegimeAvg = 20;
    private int _numPolysToBeMajor = 20;
    public RegimeGenerator()
    {
        
    }

    public override GenReport Generate(GenWriteKey key)
    {
        _id = key.IdDispenser;
        _key = key;
        _data = key.GenData;
        var report = new GenReport(GetType().Name);
        report.StartSection();
        GenerateRegimes();
        _data.Notices.GeneratedRegimes.Invoke();
        report.StopSection("all");
        return report;
    }

    private void GenerateRegimes()
    {
        var polysPerRegime = 30;
        var lmPickers = new ConcurrentDictionary<HashSet<MapPolygon>, WandererPicker>();
        var templates = _data.Models.RegimeTemplates.Models.Values.ToHashSet();
        _data.Planet.PolygonAux.LandSea.Landmasses.ForEach(lm =>
        {
            var picker = GenerateLandmassRegimes(lm, polysPerRegime, templates);
            lmPickers.TryAdd(lm, picker);
        });

        var remainders = new ConcurrentBag<HashSet<MapPolygon>>();
        
        Parallel.ForEach(_data.Planet.PolygonAux.LandSea.Landmasses, lm =>
        {
            var remainder = ExpandRegimes(lm, lmPickers[lm]);
            remainders.Add(remainder);
        });
        
        foreach (var r in remainders)
        {
            HandleRemainder(r, templates);
        }
    }

    private WandererPicker GenerateLandmassRegimes(HashSet<MapPolygon> lm, int polysPerRegime,
        HashSet<RegimeTemplate> templates)
    {
        var sw = new Stopwatch();
        var hash = lm.ToHashSet();

        int numRegimes = lm.Count / _polysForRegimeAvg;
        numRegimes = Mathf.Max(1, numRegimes);
        
        
        var seeds = lm.GetDistinctRandomElements(numRegimes);
        var group = numRegimes;
        var num6s = numRegimes / 4;
        var num2s = numRegimes / 3;
        
        var picker = new WandererPicker(lm);
        var iter = 0;
        for (var i = 0; i < seeds.Count; i++)
        {
            var prim = ColorsExt.GetRandomColor();
            var sec = prim.Inverted();
            iter++;
            var template = templates.GetRandomElement();
            // templates.Remove(template);
            var regime = Regime.Create(seeds[i], template, false, _key);
            int numToPick = 0;
            if (num6s > 0)
            {
                numToPick = 6;
                num6s--;
            }
            else if (num2s > 0)
            {
                numToPick = 2;
                num2s--;
            }
            else numToPick = 1;
            var wand = new RegimeWanderer(regime, seeds[i], picker, numToPick, _data);
            seeds[i].SetRegime(regime, _key);
        }
        
        return picker;
    }
    private HashSet<MapPolygon> ExpandRegimes(HashSet<MapPolygon> lm, WandererPicker picker)
    {
        picker.Pick(_data);
        
        foreach (var w in picker.Wanderers)
        {
            var r = ((RegimeWanderer) w).Regime;
            foreach (var p in w.Picked)
            {
                r.Polygons.Add(r, p, _key);
                p.SetRegime(r, _key);
            }
            r.SetIsMajor(w.Picked.Count >= _numPolysToBeMajor, _key);
        }
        
        return picker.NotTaken;
    }

    private void HandleRemainder(HashSet<MapPolygon> remainder, HashSet<RegimeTemplate> templates)
    {
        var unions = UnionFind.Find(
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
            for (var i = 1; i < union.Count; i++)
            {
                var p = union[i];
                regime.Polygons.Add(regime, p, _key);
                p.SetRegime(regime, _key);
            }
        }
    }

    
}
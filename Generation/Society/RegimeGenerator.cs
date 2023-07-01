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
        var numLandmassRegimes = Mathf.CeilToInt(lm.Count / polysPerRegime);
        numLandmassRegimes = Math.Max(1, numLandmassRegimes);
        // GD.Print($"{numLandmassRegimes} and {templates.Count} remaining");
        // if (numLandmassRegimes > templates.Count) throw new Exception();
        
        var seeds = lm.GetDistinctRandomElements(numLandmassRegimes);
        
        var picker = new WandererPicker(lm);
        var iter = 0;
        for (var i = 0; i < seeds.Count; i++)
        {
            var prim = ColorsExt.GetRandomColor();
            var sec = prim.Inverted();
            iter++;
            var template = templates.GetRandomElement();
            // templates.Remove(template);
            var regime = Regime.Create(seeds[i], template, _key);
            var wand = new RegimeWanderer(regime, seeds[i], picker);
            seeds[i].SetRegime(regime, _key);
        }

        return picker;
    }
    private HashSet<MapPolygon> ExpandRegimes(HashSet<MapPolygon> lm, WandererPicker picker)
    {
        picker.Pick();
        
        foreach (var w in picker.Wanderers)
        {
            var r = ((RegimeWanderer) w).Regime;
            foreach (var p in w.Picked)
            {
                r.Polygons.AddRef(p, _key);
                p.SetRegime(r, _key);
            }
        }

        return picker.NotTaken;
    }

    private void HandleRemainder(HashSet<MapPolygon> remainder, HashSet<RegimeTemplate> templates)
    {
        var unions = UnionFind.Find(
            remainder, 
            (p1, p2) => p1.IsLand == p2.IsLand,
            p => p.Neighbors.Entities()
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
            var regime = Regime.Create(union[0], template, _key);
            for (var i = 1; i < union.Count; i++)
            {
                var p = union[i];
                regime.Polygons.AddRef(p, _key);
                p.SetRegime(regime, _key);
            }
        }
    }

    
}
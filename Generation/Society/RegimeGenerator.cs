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
        GenerateRegimes();
        _data.Military.TacticalWaypoints.SetInitialOccupiers(key);
        _data.Notices.GeneratedRegimes.Invoke();
        
        foreach (var regime in key.Data.GetAll<Regime>())
        {
            var template = regime.GetUnitTemplates(key.Data)
                .First();

            var score = Mathf.CeilToInt(Mathf.Sqrt(regime.GetPolys(key.Data).Count()));
            var numUnits = score * 50;

            var capitalPoly = regime.Capital.Entity(key.Data);
            var wp = capitalPoly.GetCenterWaypoint(key.Data);
            var pos = (Vector2I)capitalPoly.Center;
            var pt = 
                capitalPoly.Center.GetPolyTri(key.Data).GetPosition();

            var unitPos = new MapPos(pos, 
                new Vector2I(wp.Id, -1), pt);
            for (var i = 0; i < numUnits; i++)
            {
                Unit.Create(template, regime, unitPos.Copy(), key);
            }
        }
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
            var picker = GenerateLandmassRegimes(lm.Polys, polysPerRegime, templates);
            lmPickers.TryAdd(lm.Polys, picker);
        });

        var remainders = new ConcurrentBag<HashSet<MapPolygon>>();
        
        Parallel.ForEach(_data.Planet.PolygonAux.LandSea.Landmasses, lm =>
        {
            var remainder = ExpandRegimes(lm.Polys, lmPickers[lm.Polys]);
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
            seeds[i].SetInitialRegime(regime, _key);

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
            seeds[i].SetInitialRegime(regime, _key);
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
                p.SetInitialRegime(r, _key);
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
                p.SetInitialRegime(regime, _key);
            }
        }
    }
}
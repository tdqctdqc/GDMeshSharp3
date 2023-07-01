
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class BuildingGenerator : Generator
{
    private GenData _data;
    private GenWriteKey _key;
    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        _data = key.GenData;
        var report = new GenReport(nameof(BuildingGenerator));
        
        // report.StartSection();
        // GenerateMines();
        // report.StopSection(nameof(GenerateMines));
        //
        // report.StartSection();
        // GenerateFactories();
        // report.StopSection(nameof(GenerateFactories));
        //
        // GenerateTownHalls();
        return report;
    }
    //
    //
    // private void GenerateMines()
    // {
    //     var minSizeToGetOneMine = 50f;
    //     var depositSizePerMine = 100f;
    //     var mineTris = new ConcurrentDictionary<PolyTriPosition, Item>();
    //     var mineable = _data.Models.Items.Models.Values.Where(v => v.Attributes.Has<MineableAttribute>());
    //     
    //     Parallel.ForEach(_data.Planet.Polygons.Entities, p =>
    //     {
    //         if (p.GetResourceDeposits(_data) is IEnumerable<ResourceDeposit> rds == false)
    //         {
    //             return;
    //         }
    //         var mineableDeposits = rds.Where(rd => mineable.Contains(rd.Item.Model()));
    //         if (mineableDeposits.Count() == 0) return;
    //         int getNumMines(ResourceDeposit rd)
    //         {
    //             var count = Mathf.FloorToInt(rd.Size / depositSizePerMine);
    //             if (rd.Size >= minSizeToGetOneMine) count = Mathf.Max(1, count);
    //             return count;
    //         }
    //         var numMines = mineableDeposits.Sum(getNumMines);
    //         var tris = p.Tris.Tris;
    //         var allowedTris = Enumerable.Range(0, tris.Length)
    //             .Where(i => tris[i].HasBuilding(_data) == false)
    //             .Where(i => Mine.CanBuildInTri(tris[i]))
    //             .OrderByDescending(i => tris[i].Landform.MinRoughness)
    //             .ToList();
    //         var numAllowed = allowedTris.Count();
    //         if (numAllowed == 0) return;
    //         if (numMines > numAllowed) numMines = numAllowed;
    //
    //         foreach (var dep in mineableDeposits)
    //         {
    //             var mineCount = getNumMines(dep);
    //             for (var k = 0; k < mineCount; k++)
    //             {
    //                 var pos = new PolyTriPosition(p.Id, (byte) allowedTris[k]);
    //                 mineTris.TryAdd(pos, dep.Item.Model());
    //             }
    //         }
    //     });
    //     foreach (var p in mineTris)
    //     {
    //         var mine = BuildingModelManager.Mines[p.Value];
    //         MapBuilding.Create(p.Key, mine, _key);
    //     }
    // }
    //
    // private void GenerateFactories()
    // {
    //     var settlementSizePerFactory = 20;
    //     var settlementSizeToGetOneFactory = 10;
    //     var tris = new ConcurrentBag<PolyTriPosition>();
    //     var factory = BuildingModelManager.Factory;
    //     var settlements = _data.Society.Settlements.Entities;
    //
    //     Parallel.ForEach(settlements, s =>
    //     {
    //         if (s.Size < settlementSizeToGetOneFactory) return;
    //         var poly = s.Poly.Entity();
    //         var availTris = poly.Tris.Tris.Where(t => factory.CanBuildInTri(t, _data)).ToList();
    //         if (availTris.Count == 0) return;
    //         var numFactories = Mathf.CeilToInt((float) s.Size / settlementSizePerFactory);
    //         numFactories = Mathf.Min(numFactories, availTris.Count);
    //         for (var i = 0; i < numFactories; i++)
    //         {
    //             tris.Add(new PolyTriPosition(poly.Id, availTris[i].Index));
    //         }
    //     });
    //     foreach (var pos in tris)
    //     {
    //         MapBuilding.Create(pos, factory, _key);
    //     }
    // }
    //
    // private void GenerateTownHalls()
    // {
    //     var townHall = BuildingModelManager.TownHall;
    //     foreach (var s in _data.Society.Settlements.Entities)
    //     {
    //         var p = s.Poly.Entity();
    //         var tri = p.Tris.Tris.First(t => t.Landform == LandformManager.Urban);
    //         s.Buildings.AddGen(townHall.Name, _key);
    //         // MapBuilding.Create(new PolyTriPosition(p.Id, tri.Index), townHall, _key);
    //     }
    // }
}
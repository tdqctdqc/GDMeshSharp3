using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LandSeaManager
{
    public List<HashSet<MapPolygon>> Landmasses { get; private set; }
    public Dictionary<MapPolygon, HashSet<MapPolygon>> LandmassDic { get; private set; }
    public List<HashSet<MapPolygon>> Seas { get; private set; }
    public Dictionary<MapPolygon, HashSet<MapPolygon>> SeaDic { get; private set; }

    public LandSeaManager()
    {
        
    }

    public void SetMasses(Data data)
    {
        Landmasses = new List<HashSet<MapPolygon>>();
        LandmassDic = new Dictionary<MapPolygon, HashSet<MapPolygon>>();
        var polys = data.GetAll<MapPolygon>();
        var landPolys = polys.Where(p => p.IsLand);
        var seaPolys = polys.Where(p => p.IsWater());
        var landmasses =
            UnionFind.Find(landPolys.ToList(), (p1, p2) => p1.HasNeighbor(p2), p1 => p1.Neighbors.Items(data));
        landmasses.ForEach(m =>
        {
            var hash = m.ToHashSet();
            Landmasses.Add(hash);
            m.ForEach(p => LandmassDic.Add(p, hash));
        });
        
        Seas = new List<HashSet<MapPolygon>>();
        SeaDic = new Dictionary<MapPolygon, HashSet<MapPolygon>>();
        var seamasses =
            UnionFind.Find(seaPolys.ToList(), 
                (p1, p2) => p1.HasNeighbor(p2), p1 => p1.Neighbors.Items(data));
        seamasses.ForEach(m =>
        {
            var hash = m.ToHashSet();
            Seas.Add(hash);
            m.ForEach(p => SeaDic.Add(p, hash));
        });

        var landAndSeaCount = landPolys.Count() + seaPolys.Count();
        if (landAndSeaCount != polys.Count) throw new Exception();
    }
}
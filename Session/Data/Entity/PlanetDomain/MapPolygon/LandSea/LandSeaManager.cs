using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LandSeaManager
{
    public List<Landmass> Landmasses { get; private set; }
    public Dictionary<MapPolygon, Landmass> LandmassDic { get; private set; }
    public List<Sea> Seas { get; private set; }
    public Dictionary<MapPolygon, Sea> SeaDic { get; private set; }

    public LandSeaManager()
    {
        
    }

    public void SetMasses(Data data)
    {
        Landmasses = new List<Landmass>();
        LandmassDic = new Dictionary<MapPolygon, Landmass>();
        var polys = data.GetAll<MapPolygon>();
        var landPolys = polys.Where(p => p.IsLand);
        var seaPolys = polys.Where(p => p.IsWater());
        var landmasses =
            UnionFind.Find(landPolys.ToList(), (p1, p2) => p1.HasNeighbor(p2), p1 => p1.Neighbors.Items(data));
        landmasses.ForEach(m =>
        {
            var lm = new Landmass(m.ToHashSet());
            Landmasses.Add(lm);
            m.ForEach(p => LandmassDic.Add(p, lm));
        });
        
        Seas = new List<Sea>();
        SeaDic = new Dictionary<MapPolygon, Sea>();
        var seamasses =
            UnionFind.Find(seaPolys.ToList(), 
                (p1, p2) => p1.HasNeighbor(p2), p1 => p1.Neighbors.Items(data));
        seamasses.ForEach(m =>
        {
            var sea = new Sea(m.ToHashSet());
            Seas.Add(sea);
            m.ForEach(p => SeaDic.Add(p, sea));
        });

        var landAndSeaCount = landPolys.Count() + seaPolys.Count();
        if (landAndSeaCount != polys.Count) throw new Exception();
    }
}
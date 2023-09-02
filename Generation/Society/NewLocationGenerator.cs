using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Priority_Queue;

public class NewLocationGenerator : Generator
{
    private Data _data;
    private GenWriteKey _key;
    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        _data = key.Data;
        var report = new GenReport("Location");
        var settlementPolys = _data
            .GetAll<MapPolygon>().Where(p => p.HasSettlement(_data)).ToList();
        report.StartSection();
        SetSettlementSizes(settlementPolys);
        SetUrbanTris(settlementPolys);
        Deforest();
        report.StopSection("");
        return report;
    }

    private void SetSettlementSizes(List<MapPolygon> settlementPolys)
    {
        var peepsPerSettlementSize = 50;

        foreach (var p in settlementPolys)
        {
            var settlement = p.GetSettlement(_data);
            settlement.SetSizeGen(p.GetPeep(_data).Size / peepsPerSettlementSize, _key);
        }
    }
    
    
    private void SetUrbanTris(List<MapPolygon> settlementPolys)
    {
        var sizeForFirstTri = 3;
        
        foreach (var p in settlementPolys)
        {
            var size = p.GetSettlement(_data).Size / sizeForFirstTri;
            var availTris = p.Tris.Tris
                .Where(t => t.Landform(_data) != _data.Models.Landforms.River
                            && t.Landform(_data) != _data.Models.Landforms.Mountain
                            && t.Landform(_data) != _data.Models.Landforms.Peak)
                .OrderBy(t => t.GetCentroid().LengthSquared());
            
            var numUrbanTris = Mathf.Min(availTris.Count(), trisForSize(size));
            for (var j = 0; j < numUrbanTris; j++)
            {
                availTris.ElementAt(j).SetLandform(_data.Models.Landforms.Urban, _key);
                availTris.ElementAt(j).SetVegetation(_data.Models.Vegetations.Barren, _key);
            }
        }
        
        int trisForSize(float size)
        {
            return Mathf.Max(1, Mathf.CeilToInt(size / sizeForFirstTri));
        }
    }
    private void Deforest()
    {
        var polys = _data.GetAll<MapPolygon>();
        foreach (var poly in polys)
        {
            var forestTris = poly.Tris.Tris.Where(t => t.Vegetation(_data) == _data.Models.Vegetations.Forest);
            if (forestTris.Count() == 0)
            {
                continue;
            }
            float deforestStr = 0f;
            if (poly.HasSettlement(_data))
            {
                deforestStr = .5f;
            }
            else if (poly.Neighbors.Items(_data).Any(n => n.HasSettlement(_data)))
            {
                deforestStr = .1f;
            }
            else continue;
            foreach (var tri in forestTris)
            {
                if (Game.I.Random.Randf() < deforestStr)
                {
                    tri.SetVegetation(_data.Models.Vegetations.Grassland, _key);
                }
            }
        }
        
    }
}
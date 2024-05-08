using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VoronoiSandbox;

public class FaultLine 
{
    public GenPlate LowId { get; private set; }
    public GenPlate HighId { get; private set; }
    public float Friction { get; private set; }
    public MapPolygon Origin => HighId.GetSeedPoly();
    public FaultLine(float friction, GenPlate highId, 
        GenPlate lowId, GenData data)
    {
        Friction = friction;
        HighId = highId;
        LowId = lowId;

    }


    public void DoEffect(GenData data)
    {
        if (Friction < .25f) return;
        var hiCells = HighId.Cells.SelectMany(c => c.Polys)
            .SelectMany(p => data.GenAuxData.PreCellPolys[p]).ToHashSet();

        var loCells = LowId.Cells.SelectMany(c => c.Polys)
            .SelectMany(p => data.GenAuxData.PreCellPolys[p]).ToHashSet();
        
        var borderCells = hiCells.Where(c => c.Neighbors.Any(loCells.Contains))
            .Concat(loCells.Where(c => c.Neighbors.Any(hiCells.Contains))).ToHashSet();
        
        var gSettings = data.GenMultiSettings.GeologySettings;
        var roughnessScale = gSettings.RoughnessScale.Value;
        var altScale = gSettings.FaultLineAltitudeScale.Value;
        var faultRangeSetting = gSettings.FaultLineRange.Value;
        var frictionAltEffect = gSettings.FrictionAltEffect.Value * altScale;
        var roughnessErosionMult = gSettings.RoughnessErosionMult.Value * roughnessScale;
        var frictionRoughnessEffect = gSettings.FrictionRoughnessEffect.Value * roughnessScale;

        
        var radius = Mathf.FloorToInt(Friction * 3f);
        var old = new HashSet<PreCell>();
        var curr = borderCells;
        
        for (var i = 1; i < radius + 1; i++)
        {
            var frictionEffect = Friction 
                * Friction
                * frictionRoughnessEffect / i;
            var rand = Game.I.Random.RandfRange(-.6f, .2f);
            
            foreach (var c in curr)
            {
                var newRoughness = Mathf.Clamp(frictionEffect 
                                               // - roughnessErosion 
                                               + rand, 
                    0f, 1f);
                c.SetRoughness(newRoughness + c.Roughness);
            }
            old.UnionWith(curr);
            curr = curr.SelectMany(c => 
                    c.Neighbors.Where(n =>
                        (hiCells.Contains(n) || loCells.Contains(n))
                        && old.Contains(n) == false))
                .ToHashSet();
        }
    }
}
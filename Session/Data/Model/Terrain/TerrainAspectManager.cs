using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TerrainAspectManager<TAspect> : IModelManager<TAspect>
    where TAspect : TerrainAspect
{
    public Dictionary<string, TAspect> ByName { get; private set; }
    public static Dictionary<byte, TAspect> ByMarker { get; private set; } //bad
    public List<TAspect> ByPriority { get; private set; }
    Dictionary<string, TAspect> IModelManager<TAspect>.Models => ByName;
    public TerrainAspectManager(TAspect waterDefault, 
        TAspect landDefault, List<TAspect> byPriority)
    {
        if (byPriority.Count + 2 > byte.MaxValue - 1) throw new Exception();
        ByPriority = byPriority;
        ByName = new Dictionary<string, TAspect>();
        ByMarker = new Dictionary<byte, TAspect>();
        ByPriority.ForEach(ta => ByName.Add(ta.Name, ta));
        for (var i = 0; i < ByPriority.Count; i++)
        {
            ByMarker.Add((byte)i, ByPriority[i]);
            ByPriority[i].SetMarker((byte)i);
        }
    }

    

}
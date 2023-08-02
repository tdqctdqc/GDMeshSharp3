// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Godot;
//
// public abstract class TerrainAspectManager<TAspect> : IModelManager<TAspect>
//     where TAspect : TerrainAspect
// {
//     public Dictionary<string, TAspect> ByName { get; private set; }
//     // public static Dictionary<byte, TAspect> ByMarker { get; private set; } //bad
//     Dictionary<string, TAspect> IModelManager<TAspect>.Models => ByName;
//     public TerrainAspectManager(List<TAspect> byPriority)
//     {
//         if (byPriority.Count + 2 > byte.MaxValue - 1) throw new Exception();
//         ByName = new Dictionary<string, TAspect>();
//         
//     }
//
//     
//
// }
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;
namespace VoronoiSandbox;

public class PreCellResult
{
    public List<PreCell> Cells { get;  set; }
    public List<PrePoly> Polys { get;  set; }
    public Dictionary<Vector2I, PreEdge> Edges { get;  set; }
    public List<PreNexus> Nexi { get; set; }
    public HashSet<PreCell> Join4Cells { get; private set; }
    public ConcurrentDictionary<Vector2I, 
            (Vector2I a1, Vector2I a2, Vector2I c1, Vector2I c2)> 
        SplitBag { get; set; }
    public PreCellResult()
    {
        Cells = new();
        Polys = new();
        Edges = new();
        Nexi = new();
        Join4Cells = new HashSet<PreCell>();
        SplitBag = new();
    }
}
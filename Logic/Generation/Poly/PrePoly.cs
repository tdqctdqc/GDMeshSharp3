using System.Collections.Generic;
using Godot;

namespace VoronoiSandbox;

public class PrePoly
{
    public int Id { get; private set; }
    public Vector2I RelTo { get; private set; }
    public List<PreCell> Cells { get; private set; }
    public HashSet<PrePoly> Neighbors { get; set; }
    
    public PrePoly(int id, Vector2I relTo)
    {
        Id = id;
        Cells = new List<PreCell>();
        Neighbors = new HashSet<PrePoly>();
        RelTo = relTo;
    }
}
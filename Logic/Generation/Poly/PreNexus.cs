using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace VoronoiSandbox;

public class PreNexus
{
    public int Id { get; set; }
    public Vector2I Pos { get; set; }
    public PrePoly P1 { get; set; }
    public PrePoly P2 { get; set; }
    public PrePoly P3 { get; set; }
    public PreEdge E1 { get; set; }
    public PreEdge E2 { get; set; }
    public PreEdge E3 { get; set; }

    public PreNexus(int id,
        Vector2I pos)
    {
        Id = id;
        Pos = pos;
    }
}
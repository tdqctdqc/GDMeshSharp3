using System;
using Godot;

namespace VoronoiSandbox;
public class PreEdge
{
    public int Id { get; private set; }
    public PrePoly P1 { get; private set; }
    public PrePoly P2 { get; private set; }
    public PreNexus N1 { get; private set; }
    public PreNexus N2 { get; private set; }

    public PreEdge(int id, PrePoly p1, PrePoly p2)
    {
        Id = id;
        P1 = p1;
        P2 = p2;
        N1 = null;
        N2 = null;
    }

    public void SetNexus(PreNexus pre)
    {
        if (N1 == null)
        {
            N1 = pre;
        }
        else if (N2 == null)
        {
            N2 = pre;
        }
        else
        {
            // GD.Print($"more than 2 nexi for edge {P1.RelTo} {P2.RelTo}");
            // throw new Exception();
        }
    }
}
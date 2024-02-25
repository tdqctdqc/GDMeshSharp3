using System.Collections.Generic;
using Godot;


public class PreCellResult
{
    public List<PreCell> Cells { get; private set; }
    public List<PrePoly> Polys { get; private set; }
    public Dictionary<Vector2I, PreEdge> Edges { get; private set; }
    public List<PreNexus> Nexi { get; private set; }

    public PreCellResult(List<PreCell> cells, List<PrePoly> polys, 
        Dictionary<Vector2I, PreEdge> edges, 
        List<PreNexus> nexi)
    {
        Cells = cells;
        Polys = polys;
        Edges = edges;
        Nexi = nexi;
    }
}
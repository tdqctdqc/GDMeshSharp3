
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class StrategicContextIssue : Issue
{
    public StrategicContext Context { get; private set; }
    public StrategicContextIssue(Regime r, StrategicContext context, Data d)
        : base(r.Capital.Get(d).GetCenter(), 
            $"{r.Name} Strategic Context", d.GetTick())
    {
        Context = context;
        AddLayer("cells", mb => Context.DrawCells(mb, Pos, d));

        var iter = 0;
        foreach (var (key, value) in Context.EdgeMergeMap)
        {
            AddLayer($"edge {iter++}", mb => DrawEdge(key, mb, d));
        }
        AddLayer("nodes", mb => DrawNodes(mb, d));
        
    }

    private void DrawEdge(List<FrontFace> edge, MeshBuilder mb, Data d)
    {
        var (start, end) = edge.GetStartEndNexus(d);
        var mapsTo = Context.EdgeMergeMap[edge];
        
        mb.DrawFrontFaces(edge, Colors.White, 5f, Pos, d);

        foreach (var map in mapsTo)
        {
            mb.DrawFrontFaces(map, Colors.Blue, 3f, Pos, d);
        }
    }

    private void DrawNodes(MeshBuilder mb, Data d)
    {
        foreach (var graphNode in Context.Graph.Nodes)
        {
            var join = Cell.GetNexusPoint(graphNode.Element, d);
            
            mb.AddCircle(Pos.Offset(join, d), 3f, 12, Colors.Black);
        }
    }

    
    
}
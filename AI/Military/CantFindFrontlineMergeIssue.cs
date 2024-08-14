using System.Collections.Generic;
using System.Linq;
using Godot;

public class CantFindFrontlineMergeIssue : Issue
{
    public FrontFace Face { get; private set; }
    public List<FrontFace> OldFrontline { get; private set; }
    public StrategicContext Context { get; private set; }
    public CantFindFrontlineMergeIssue(Alliance a, 
        FrontFace face,
        List<FrontFace> oldFrontline,
        StrategicContext context,
        Data d) 
        : base(face.GetMid(d), 
            $"{a.Leader.Get(d).Name} can't find frontline merge", 
            d.GetTick())
    {
        Face = face;
        OldFrontline = oldFrontline;
        Context = context;
        AddLayer("Cells", mb => Context.DrawCells(mb, Pos, d));
        AddLayer("Old frontline", mb => DrawOldFrontline(mb, d));
        AddLayer("New edges", mb => DrawNewEdges(mb, d));
        AddLayer("Face", mb => DrawFace(mb, d));
    }

    private void DrawFace(MeshBuilder mb, Data d)
    {
        mb.DrawFrontFace(Face, Colors.Red, 2f, Pos, d);
    }
    private void DrawOldFrontline(MeshBuilder mb, Data d)
    {
        mb.DrawFrontFaces(OldFrontline, Colors.White, 5f, Pos, d);
    }
    private void DrawNewEdges(MeshBuilder mb, Data d)
    {
        foreach (var frontFace in Context.Graph.Edges.SelectMany(e => e)
                     .SelectMany(e => e))
        {
            mb.DrawFrontFace(frontFace, Colors.Black, 3f, Pos, d);
        }
    }
}
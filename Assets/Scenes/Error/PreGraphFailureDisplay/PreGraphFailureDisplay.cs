using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PreGraphFailureDisplay : Node2D
{
    public void Setup(PreGraphFailureException e)
    {
        var mb = new MeshBuilder();
        foreach (var lineSegment in e.Graph.Edges)
        {
            mb.AddArrow(lineSegment.From, lineSegment.To, 3f, Colors.Red);
        }

        foreach (var center in e.Graph.Nodes.Select(n => n.Element.Center))
        {
            mb.AddPointMarkers(new List<Vector2>{center}, 10f, Colors.Blue);
        }
        AddChild(mb.GetMeshInstance());
    }
}

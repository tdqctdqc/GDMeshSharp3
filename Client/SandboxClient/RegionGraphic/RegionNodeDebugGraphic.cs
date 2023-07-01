using System;
using System.Linq;
using Godot;

public partial class RegionNodeDebugGraphic : Area2D
{
    private IReadOnlyGraph<Vector2> _graph;
    private Line2D _edges;
    private Action _click;
    public IReadOnlyGraphNode<Vector2> Node { get; private set; }

    public RegionNodeDebugGraphic()
    {
    }

    public RegionNodeDebugGraphic(IReadOnlyGraphNode<Vector2> regionNode, 
        IGraph<Vector2, LineSegment> graph,
        Action click,
        float dist)
    {
        var r = dist / 3f;
        _click = click;
        Position = regionNode.Element;
        var circ = new CircleShape2D();
        circ.Radius = r;
        var colShape = new CollisionShape2D();
        colShape.Shape = circ;
        AddChild(colShape);
        var mesh = MeshGenerator.GetCircleMesh(Vector2.Zero, r, 16);
        Node = regionNode;
        _graph = graph;
        _edges = new Line2D();
        _edges.DefaultColor = Colors.Blue;
        _edges.Width = dist / 6f;
        _edges.ZIndex = 0;
        _edges.ZAsRelative = false;
        mesh.ZIndex = 1;
        mesh.ZAsRelative = false;
        AddChild(_edges);
        AddChild(mesh);
        var ns = graph.GetNeighbors(regionNode.Element).Select(n => graph.GetNode(n));
        foreach (var nNode in ns)
        {
            _edges.AddPoint(Vector2.Zero);
            _edges.AddPoint(nNode.Element - Node.Element);
        }
    }


    public override void _InputEvent(Viewport viewport, InputEvent e, int shapeIdx)
    {
        if (e is InputEventMouseButton m && m.Pressed == false)
        {
            _click?.Invoke();
        }
    }
}
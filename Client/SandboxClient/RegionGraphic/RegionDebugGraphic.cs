using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegionDebugGraphic : Node2D
{
    private Dictionary<IReadOnlyGraphNode<Vector2>, RegionNodeDebugGraphic> _nodeGraphics;
    private Region<Vector2> _region;
    private Graph<Vector2, LineSegment> _graph;
    private Action<DisplayableException> _displayException;
    private Control _debugHook;
    private Node2D _boundaryGraphics;
    public void Setup(Action<DisplayableException> displayException, Control debugHook)
    {
        _debugHook = debugHook;
        _displayException = displayException;
        _nodeGraphics = new Dictionary<IReadOnlyGraphNode<Vector2>, RegionNodeDebugGraphic>();
        SetupGraph();
        SetupGraphics();
        Draw();
    }

    private void SetupGraph()
    {
        _graph = new Graph<Vector2, LineSegment>();

        var dist = 1000f;
        var elsPerAxis = 10;
        
        var grid = new IGraphNode<Vector2>[elsPerAxis][];
        var ps = new HashSet<Vector2>();
        
        for (var i = 0; i < elsPerAxis; i++)
        {
            grid[i] = new IGraphNode<Vector2>[elsPerAxis];
        }
        
        for (var i = 0; i < elsPerAxis; i++)
        {
            for (var j = 0; j < elsPerAxis; j++)
            {
                var p = new Vector2(i * dist, j * dist);
                var node = _graph.AddNode(p);
                ps.Add(p);
                grid[i][j] = node;
            }
        }
        grid.DoForGridNeighbors((g, h) =>
        {
            if (h.Element.X >= g.Element.X && h.Element.Y >= g.Element.Y)
            {
                _graph.AddEdge(g.Element, h.Element, new LineSegment(g.Element, h.Element));
            }
            return true;
        });
        
        _region = new Region<Vector2>(ps, _graph);
    }


    private void SetupGraphics()
    {
        foreach (var graphNode in _graph.Nodes)
        {
            var graphic = new RegionNodeDebugGraphic(graphNode, _graph,
                () => HandleClick(graphNode),
                1000f
            );
            AddChild(graphic);
            _nodeGraphics.Add(graphNode, graphic);
        }
    }

    private void HandleClick(IReadOnlyGraphNode<Vector2> graphNode)
    {
        ExceptionCatcher.Try(() =>
        {
            if (_region.Elements.Contains(graphNode.Element))
            {
                _region.RemoveElement(graphNode.Element);
            }
            else
            {
                _region.AddElement(graphNode.Element);
            }
            
            Draw();
        }, _displayException);

    }
    public void Draw()
    {
        foreach (var kvp in _nodeGraphics)
        {
            var node = kvp.Key;
            var graphic = kvp.Value;

            if (_region.Elements.Contains(node.Element))
            {
                var cont = _region.NodeRegions[node.Element];
                if (cont.IsBridgeElement(node.Element))
                {
                    graphic.Modulate = Colors.HotPink;
                }
                else if (cont.Border.Elements.Contains(node.Element))
                {
                    graphic.Modulate = Colors.Black;
                }
                else
                {
                    graphic.Modulate = _region.GetRegionColor(cont);
                }
            }
            else
            {
                graphic.Modulate = Colors.White;
            }
        }
        
        _boundaryGraphics?.Free();
        var boundarySegs = _region.ContiguousRegions
            .SelectMany(c => c.Border.Segments).ToList();
        var borderPairs = _region.ContiguousRegions
            .SelectMany(c => c.Border.OrderedBorderPairs).ToList();
        var mb = new MeshBuilder();
        mb.AddLines(borderPairs.Select(p =>
        {
            var mid = (p.Foreign + p.Native) / 2f;
            return new LineSegment(p.Native, p.Foreign).Rotated(Mathf.Pi / 2f, .5f);
        }).ToList(), 50f, Colors.Black);
        if (mb.Tris.Count > 0)
        {
            _boundaryGraphics = mb.GetMeshInstance();
            AddChild(_boundaryGraphics);
        }
    }
}
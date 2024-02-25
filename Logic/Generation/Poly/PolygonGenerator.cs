using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DelaunatorSharp;


public class PolygonGenerator : Generator
{
    private Vector2I _dimensions;
    private bool _leftRightWrap;
    private Data _data;
    public PolygonGenerator(Vector2I dimensions, 
        bool leftRightWrap)
    {
        _dimensions = dimensions;
        _leftRightWrap = leftRightWrap;
    }
    public override GenReport Generate(GenWriteKey key)
    {
        var report = new GenReport(GetType().Name);
        _data = key.Data;
        
        var bounds = new Vector2[]
        {
            Vector2.Zero,
            Vector2.Right * _dimensions.X,
            new Vector2(_dimensions.X, _dimensions.Y),
            Vector2.Down * _dimensions.Y
        };
        
        report.StartSection();
        var res = PreCellGenerator.Make(_dimensions, 
            key);
        foreach (var prePoly in res.Polys)
        {
            var poly = MapPolygon
                .Create(prePoly, _dimensions.X, key);
            key.GenData.GenAuxData.PreCellPolys.Add(poly, prePoly.Cells);
        }
        foreach (var pre in res.Nexi)
        {
            var nexus = MapPolyNexus.Create(pre, key);
        }
        foreach (var (vector2I, pre) in res.Edges)
        {
            var p1 = key.Data.Get<MapPolygon>(vector2I.X);
            var p2 = key.Data.Get<MapPolygon>(vector2I.Y);
            var edge = MapPolygonEdge.Create(pre, key);
        }
        return report;
    }
}

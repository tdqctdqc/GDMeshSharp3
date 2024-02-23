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
        foreach (var (vector2I, pre) in res.Edges)
        {
            var p1 = key.Data.Get<MapPolygon>(vector2I.X);
            var p2 = key.Data.Get<MapPolygon>(vector2I.Y);
            var edge = MapPolygonEdge.Create(pre, key);
        }

        var nexiByEdgeId = new Dictionary<int, MapPolyNexus>();
        
        foreach (var (vector3I, pre) in res.Nexi)
        {
            var p1 = key.Data.Get<MapPolygon>(vector3I.X);
            var p2 = key.Data.Get<MapPolygon>(vector3I.Y);
            var p3 = key.Data.Get<MapPolygon>(vector3I.Z);
            var nexus = MapPolyNexus.Create(pre, key);
            
            void add(MapPolygon poly1, MapPolygon poly2)
            {
                var edge = poly1.GetEdge(poly2, key.Data);
                if (nexiByEdgeId.TryGetValue(edge.Id, out var otherNexus))
                {
                    edge.SetNexi(nexus, otherNexus, key);
                }
                else
                {
                    nexiByEdgeId.Add(edge.Id, nexus);
                }
            }
        }
        
        foreach (var (edgeId, nexus) in nexiByEdgeId)
        {
            var edge = key.Data.Get<MapPolygonEdge>(edgeId);
            if (edge.HighPoly.IsEmpty())
            {
                
            }
        }
        
        return report;
    }
}

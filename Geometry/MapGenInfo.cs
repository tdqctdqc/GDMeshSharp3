using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapGenInfo
{
    public Vector2 Dimensions { get; private set; }
    public bool LRWrap { get; private set; }
    public List<Vector2> Points,
        TopPoints,
        BottomPoints,
        CornerPoints,
        LeftPoints,
        RightPoints;
    public List<MapPolygon> Polys,
        TopPolys,
        BottomPolys,
        CornerPolys,
        LeftPolys,
        RightPolys;
    public HashSet<Vector2> LRCornerCenterHash { get; private set; }
    public HashSet<MapPolygon> LRCornerPolyHash { get; private set; }
    public Dictionary<MapPolygon, MapPolygon> LRPairs { get; private set; }
    public Dictionary<Vector2,MapPolygon> PolysByCenter { get; private set; }
    public MapGenInfo(List<Vector2> points, Vector2 dimensions, float polySize, bool leftRightWrap)
    {
        Dimensions = dimensions;
        LRWrap = leftRightWrap;
        var numLrEdgePoints = (int)(dimensions.Y / polySize);
        var numTbEdgePoints = (int)(dimensions.X / polySize);
    
        if (leftRightWrap)
        {
            var leftRightPoints = GetLeftRightWrapEdgePoints(dimensions, points, numLrEdgePoints, .1f);
            LeftPoints = leftRightPoints.leftPoints;
            RightPoints = leftRightPoints.rightPoints;
        }
        else
        {
            LeftPoints = GetConstrainedRandomFloats(dimensions.Y, .1f, numLrEdgePoints)
                .Select(l => new Vector2(0f, l).Intify()).ToList();
        
            RightPoints = GetConstrainedRandomFloats(dimensions.Y, .1f, numLrEdgePoints)
                .Select(l => new Vector2(dimensions.X, l).Intify()).ToList();
        
        }
        CornerPoints = new List<Vector2> { new Vector2(0f, 0f).Intify(), new Vector2(dimensions.X, 0f).Intify(), 
            new Vector2(0f, dimensions.Y).Intify(), new Vector2(dimensions.X, dimensions.Y).Intify()};
        
        TopPoints = GetConstrainedRandomFloats(dimensions.X, .1f, numTbEdgePoints)
            .Select(l => new Vector2(l, 0f).Intify()).ToList();
        BottomPoints = GetConstrainedRandomFloats(dimensions.X, .1f, numTbEdgePoints)
            .Select(l => new Vector2(l, dimensions.Y).Intify()).ToList();

        LRCornerCenterHash = LeftPoints.Union(RightPoints).Union(CornerPoints).ToHashSet();

        CornerPolys = new List<MapPolygon>();
        LRPairs = new Dictionary<MapPolygon, MapPolygon>();
        PolysByCenter = new Dictionary<Vector2, MapPolygon>();
        Points = points
            .Union(CornerPoints)
            .Union(LeftPoints)
            .Union(RightPoints)
            .Union(TopPoints)
            .Union(BottomPoints).ToList();
    }

    public void SetupPolys(List<MapPolygon> polys)
    {
        Polys = polys;
        var centerHash = Points.ToHashSet();
        Polys.ForEach(p =>
        {
            if (centerHash.Contains(p.Center) == false)
            {
                throw new Exception();
            }
        });
        PolysByCenter = polys.ToDictionary(p => p.Center, p => p);
        LeftPolys = LeftPoints.Select(p => PolysByCenter[p]).ToList();
        RightPolys = RightPoints.Select(p => PolysByCenter[p]).ToList();
        TopPolys = TopPoints.Select(p => PolysByCenter[p]).ToList();
        BottomPolys = BottomPoints.Select(p => PolysByCenter[p]).ToList();
        CornerPolys = CornerPoints.Select(p => PolysByCenter[p]).ToList();
        LRCornerPolyHash = LRCornerCenterHash.Select(v => PolysByCenter[v]).ToHashSet();
    }
    private static (List<Vector2> leftPoints, List<Vector2> rightPoints) GetLeftRightWrapEdgePoints(Vector2 dimensions, List<Vector2> points, 
        int numEdgePoints, float marginRatio)
    {
        var left = new List<Vector2>();
        var right = new List<Vector2>();
        var lengthPer = dimensions.Y / numEdgePoints;
        var margin = lengthPer * marginRatio;
        var lats = GetConstrainedRandomFloats(dimensions.Y, .1f, numEdgePoints);
        for (int i = 0; i < lats.Count; i++)
        {
            var lat = lats[i];
            left.Add(new Vector2(0f, lat).Intify());
            right.Add(new Vector2(dimensions.X, lat).Intify());
        }

        return (left, right);
    }
    private static List<float> GetConstrainedRandomFloats(float range, float marginRatio, int count)
    {
        var result = new List<float>();
        var lengthPer = range / count;
        var margin = lengthPer * marginRatio;
        for (int i = 1; i < count - 1; i++)
        {
            var sample = Game.I.Random.RandfRange(lengthPer * i + margin, lengthPer * (i + 1) - margin);
            result.Add(sample);
        }
        return result;
    }
}
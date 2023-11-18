using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class PointsGenerator 
{
    public static List<Vector2> GetSquareMarkerMesh(List<Vector2> points, float markerSize)
    {
        var list = new List<Vector2>();
        foreach (var p in points)
        {
            var topLeft = p + Vector2.Up * markerSize / 2f
                            + Vector2.Left * markerSize / 2f;
            var topRight = p + Vector2.Up * markerSize / 2f
                            + Vector2.Right * markerSize / 2f;
            var bottomLeft = p + Vector2.Down * markerSize / 2f
                            + Vector2.Left * markerSize / 2f;
            var bottomRight = p + Vector2.Down * markerSize / 2f
                            + Vector2.Right * markerSize / 2f;
            list.Add(topLeft);
            list.Add(topRight);
            list.Add(bottomLeft);
            list.Add(topRight);
            list.Add(bottomRight);
            list.Add(bottomLeft);
        }

        return list;
    }
    public static List<Vector2> GenerateSemiRegularPoints(Vector2 dim, 
                                                    float cellSize, 
                                                    int triesBeforeExit,
                                                    bool square,
                                                    int maxPoints = int.MaxValue)
    {
        var points = new List<Vector2>();
        var grid = new RegularGrid<Vector2>(v => v, cellSize);
        var rand = new RandomNumberGenerator();
        int badTries = 0;

        while(badTries < triesBeforeExit && points.Count < maxPoints)
        {
            float x = rand.RandfRange(0f, dim.X);
            float y = rand.RandfRange(0f, dim.Y);
            var vec = new Vector2(x,y);
            if(grid.GetElementsAtPoint(vec).Count == 0)
            {
                badTries = 0;
                grid.AddElement(vec);
                points.Add(vec);
            }
            else
            {
                badTries++;
            }
        }
        if(square)
        {
            AddMeshBorder(points, dim, cellSize, cellSize / 2f);
        }
        return points;
    }

    public static List<Vector2> GenerateConstrainedSemiRegularPoints(Vector2 dim,
                                                                float cellSize,
                                                                float constraintSize,
                                                                bool square,
                                                                bool roundToInt)
    {
        var rand = new RandomNumberGenerator();
        var points = new List<Vector2>();
        int xCells = Mathf.CeilToInt(dim.X / cellSize);
        int yCells = Mathf.CeilToInt(dim.Y / cellSize);
        for (int i = 0; i < xCells; i++)
        {
            for (int j = 0; j < yCells; j++)
            {
                var cellCenter = Vector2.One * cellSize / 2f + new Vector2(i,j) * cellSize;
                float x = rand.RandfRange(-constraintSize / 2f, constraintSize / 2f);
                float y = rand.RandfRange(-constraintSize / 2f, constraintSize / 2f);
                var point = cellCenter + new Vector2(x,y);
                if (roundToInt) point = new Vector2((int) point.X, (int) point.Y);
                points.Add(point);
            }
        }
        if(square) AddMeshBorder(points, dim, cellSize, cellSize / 2f);
        return points;
    }

    private static void AddMeshBorder(List<Vector2> points, 
                                        Vector2 dim, 
                                        float cellSize,
                                        float margin)
    {
        int xPoints = Mathf.CeilToInt((dim.X + margin * 2f) / cellSize);
        int yPoints = Mathf.CeilToInt((dim.Y + margin * 2f) / cellSize);

        for (int i = 0; i < xPoints; i++)
        {
            var top = new Vector2(i * cellSize, 0f) - Vector2.One * margin;
            var bottom = new Vector2(i * cellSize, dim.Y) + Vector2.One * margin;
            if(points.Contains(top) == false) points.Add(top);
            if(points.Contains(bottom) == false) points.Add(bottom);
        }
        for (int i = 0; i < yPoints; i++)
        {
            var left = new Vector2(0f, i * cellSize) - Vector2.One * margin;
            var right = new Vector2(dim.X, i * cellSize) + Vector2.One * margin;
            if(points.Contains(left) == false) points.Add(left);
            if(points.Contains(right) == false) points.Add(right);
        }
        var bl = new Vector2(-margin, cellSize * (yPoints - .5f));
        var tr = new Vector2(cellSize * (xPoints - .5f), -margin);
        if(points.Contains(bl) == false) points.Add(bl);
        if(points.Contains(tr) == false) points.Add(tr);
    }

    public static void GenerateInteriorPoints(this Vector2[] border, float cellSize,
        float cellMarginRatio, Action<Vector2> add)
    {
        var minX = border.Min(v => v.X);
        var maxX = border.Max(v => v.X);
        var minY = border.Min(v => v.Y);
        var maxY = border.Max(v => v.Y);

        var width = Mathf.Abs(maxX - minX);
        var numXPartitions = Mathf.FloorToInt(width / cellSize);
        var cellWidth = width / numXPartitions;
        var cellWidthMargin = cellWidth * cellMarginRatio;

        var height = Mathf.Abs(maxY - minY);
        var numYPartitions = Mathf.FloorToInt(height / cellSize);
        var cellHeight = height / numYPartitions;
        var cellHeightMargin = cellHeight * cellMarginRatio;

        for (var i = 0; i < numXPartitions; i++)
        {
            for (var j = 0; j < numYPartitions; j++)
            {
                var cellMinX = minX + cellWidth * i + cellWidthMargin;
                var cellMaxX = minX + cellWidth * (i + 1) - cellWidthMargin;
                var cellMinY = minY + cellHeight * j + cellHeightMargin;
                var cellMaxY = minY + cellHeight * (j + 1) - cellHeightMargin;
                if (cellMinX >= cellMaxX || cellMinY >= cellMaxY) throw new Exception();
                var randP = new Vector2(Game.I.Random.RandfRange(cellMinX, cellMaxX),
                    Game.I.Random.RandfRange(cellMinY, cellMaxY));
                if (Geometry2D.IsPointInPolygon(randP, border)) add(randP);
            }
        }
    }
    public static void GenerateInteriorPointsMargin(this Vector2[] border, float cellSize, float margin, Action<Vector2> add)
    {
        var minX = border.Min(b => b.X);
        var maxX = border.Max(b => b.X);
        
        var minY = border.Min(b => b.Y);
        var maxY = border.Max(b => b.Y);

        var xCells = Mathf.Abs(maxX - minX) / cellSize;
        var yCells = Mathf.Abs(maxY - minY) / cellSize;
        var mod = Vector2.Right * cellSize * .1f;
        var shift = Vector2.One * cellSize * .5f;

        var innerBorder = Geometry2D.OffsetPolygon(border, -margin).Cast<Vector2[]>();

        for (int i = 0; i < xCells; i++)
        {
            for (int j = 0; j < yCells; j++)
            {
                mod = mod.Rotated(Game.I.Random.RandfRange(0f, Mathf.Pi * 2f));
                var p = new Vector2(minX + cellSize * i, minY + cellSize * j) + mod + shift;
                if(innerBorder.Any(b => Geometry2D.IsPointInPolygon(p, b)))
                {
                    add(p);
                }
            }
        }
    }
}
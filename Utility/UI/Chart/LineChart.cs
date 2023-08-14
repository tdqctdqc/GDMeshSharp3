using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class LineChart : VBoxContainer
{
    public LineChart(Vector2 dim, float thickness, List<string> names, List<List<Vector2>> lines, List<Color> colors,
        bool xInt, bool yInt)
    {
        if (lines.Count == 0) return;
        var panel = new Panel();
        var padding = 50f;
        panel.Size = dim + Vector2.One * padding;
        AddChild(panel);
        LayoutDirection = LayoutDirectionEnum.Ltr;
        var minX = lines.Min(l => l.Count == 0 ? 0f : l.Select(v => v.X).Min());
        var maxX = lines.Max(l => l.Count == 0 ? 0f : l.Select(v => v.X).Max());
        var xRange = maxX - minX;
        var minY = lines.Min(l => l.Count == 0 ? 0f : l.Select(v => v.Y).Min());
        var maxY = lines.Max(l => l.Count == 0 ? 0f : l.Select(v => v.Y).Max());
        var yRange = maxY - minY;
        var lineHolder = new Control();
        lineHolder.CustomMinimumSize = dim;
        var nameHolder = new HBoxContainer();

        var scaleMarks = 11;
        Control scaleY;
        if (yInt)
        {
            scaleY = MakeIntScale(dim.Y, scaleMarks, yRange, minY, maxY, Vector2.Down);
        }
        else
        {
            scaleY = MakeFloatScale(dim.Y, scaleMarks, yRange, minY, maxY, Vector2.Down);
        }
        
        Control scaleX;
        if (xInt)
        {
            scaleX = MakeIntScale(dim.X, scaleMarks, xRange, minX, maxX, Vector2.Right);
        }
        else
        {
            scaleX = MakeFloatScale(dim.X, scaleMarks, xRange, minX, maxX, Vector2.Right);
        }
        
        scaleY.Position += Vector2.Left * padding / 2f;
        scaleX.Position += Vector2.Down * padding / 2f;
        scaleX.Position += Vector2.Down * dim.Y;
        lineHolder.Position = Vector2.One * padding;
        
        lineHolder.AddChild(scaleX);
        lineHolder.AddChild(scaleY);
        panel.AddChild(lineHolder);
        AddChild(nameHolder);

        for (var i = 0; i < names.Count; i++)
        {
            var name = names[i];
            var nameLabel = new Label();
            nameLabel.Modulate = colors[i];
            nameLabel.Text = name;
            nameHolder.AddChild(nameLabel);
            var points = lines[i];
            var line2d = new Line2D();
            lineHolder.AddChild(line2d);
            line2d.Width = thickness;
            line2d.DefaultColor = colors[i];
            
            for (var j = 0; j < points.Count; j++)
            {
                var p = TransformPoint(points[j], dim, minX, maxX, minY, maxY);
                line2d.AddPoint(p);
            }
        }
    }

    private Control MakeFloatScale(float dim, int scaleMarks, float range, float min, float max, Vector2 basis)
    {
        var scale = new Control();
        for (var i = 0; i < scaleMarks; i++)
        {
            var ratio = (float) i / scaleMarks;
            
            var val = min + ratio * range;
            var pos = dim * (1f - ratio);
            var label = new Label();
            label.Text = val.ToString().PadDecimals(2);
            label.Position = basis * pos;
            scale.AddChild(label);
        }

        return scale;
    }
    private Control MakeIntScale(float dim, int scaleMarks, float range, float min, float max, Vector2 basis)
    {
        var scale = new Control();
        var ticksPerMark = Mathf.CeilToInt(range / scaleMarks);
        for (var i = 0; i < max; i += ticksPerMark)
        {
            var val = min + i;
            var ratio = val / max;
            var pos = dim * ratio;
            var label = new Label();
            label.Text = val.ToString();
            label.Position = basis * pos;
            scale.AddChild(label);
        }

        return scale;
    }
    private LineChart()
    {
    }

    private Vector2 TransformPoint(Vector2 worldPoint, Vector2 dim, float minX, float maxX, float minY, float maxY)
    {
        var xRange = maxX - minX;
        var xRatio = (worldPoint.X - minX) / xRange;
        var yRange = maxY - minY;
        var yRatio = 1f - (worldPoint.Y - minY) / yRange;
        return new Vector2(dim.X * xRatio, dim.Y * yRatio);
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DeployOnLineOrder : UnitOrder
{
    public List<Vector2> Points { get; private set; }

    public DeployOnLineOrder(List<Vector2> points)
    {
        Points = points;
        for (var i = 0; i < points.Count; i++)
        {
            var p = points[i];
            if (float.IsNaN(p.X) || float.IsNaN(p.Y))
            {
                throw new Exception("bad point " + p);
            }
        }
    }

    public override void Handle(UnitGroup g, Data d, HandleUnitOrdersProcedure proc)
    {
        var units = g.Units.Items(d);
        var count = units.Count();
        var iter = 0;
        foreach (var unit in units)
        {
            iter++;
            var pos = Points.GetPointAlongLine(
                (v, w) => v.GetOffsetTo(w, d),
                (float)iter / count);
            proc.NewUnitPosesById.Add(unit.Id, pos);
        }
    }

    public override void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data data)
    {
        var innerColor = group.Regime.Entity(data).PrimaryColor;
        var outerColor = group.Regime.Entity(data).PrimaryColor;
        var squareSize = 5f;
        var lineSize = 1f;
        for (var i = 0; i < Points.Count; i++)
        {
            var p = Points[i];
            var pRel = relTo.GetOffsetTo(p, data);
            if (i < Points.Count - 1)
            {
                var pNext = Points[i + 1];
                var pNextRel = relTo.GetOffsetTo(pNext, data);
                mb.AddLine(pRel, pNextRel, innerColor, lineSize);
            }
            // mb.AddSquare(pRel, squareSize, outerColor);
            // mb.AddSquare(pRel, squareSize * .75f, innerColor);
        }
    }
}
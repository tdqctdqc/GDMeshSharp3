
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
                (v, w) => d.Planet.GetOffsetTo(v, w),
                (iter + 1) / (count + 1));
            iter++;
            proc.NewUnitPosesById.Add(unit.Id, pos);
        }
    }

    public override void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data data)
    {
        var innerColor = ColorsExt.GetRandomColor();
        var outerColor = group.Regime.Entity(data).PrimaryColor;
        for (var i = 0; i < Points.Count; i++)
        {
            var p = Points[i];
            var pRel = data.Planet.GetOffsetTo(relTo, p);
            if (i < Points.Count - 1)
            {
                var pNext = Points[i + 1];
                var pNextRel = data.Planet.GetOffsetTo(relTo, pNext);
                mb.AddLine(pRel, pNextRel, innerColor, 4f);
            }
            mb.AddPointMarker(pRel, 12f, outerColor);
            mb.AddPointMarker(pRel, 10f, innerColor);

        }
    }
}
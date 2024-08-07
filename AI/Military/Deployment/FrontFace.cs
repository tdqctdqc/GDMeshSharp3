
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public struct FrontFace
{
    public int Native { get; private set; }
    public int Foreign { get; private set; }
    public static FrontFace
        Construct(Cell native, Cell foreign, Data d)
    {
        return new FrontFace(native.Id, foreign.Id);
    }
    private FrontFace(int native, int foreign)
    {
        Native = native;
        Foreign = foreign;
    }

    public int GetLeftNeighborCellId(Data d)
    {
        var key = GetIdEdgeKey();
        var flip = Native < Foreign;
        return flip
            ? d.Planet.MapAux.CellHolder.Rights[key]
            : d.Planet.MapAux.CellHolder.Lefts[key];
    }
    public FrontFace GetLeftNeighborFace(Func<Cell, bool> isNative,
        Data d)
    {
        var leftId = GetLeftNeighborCellId(d);
        if (leftId == -1) throw new Exception();
        var left = PlanetDomainExt.GetPolyCell(leftId, d);
        var native = PlanetDomainExt.GetPolyCell(Native, d);
        if (isNative(left))
        {
            var foreign = PlanetDomainExt.GetPolyCell(Foreign, d);
            return FrontFace.Construct(left, foreign, d);
        }
        return FrontFace.Construct(native, left, d);
    }
    public int GetRightNeighborCellId(Data d)
    {
        var key = GetIdEdgeKey();
        var flip = Native < Foreign;
        return flip
            ? d.Planet.MapAux.CellHolder.Lefts[key]
            : d.Planet.MapAux.CellHolder.Rights[key];
    }
    public FrontFace GetRightNeighborFace(
        Func<Cell, bool> isNative,
        Data d)
    {
        var rightId = GetRightNeighborCellId(d);
        
        if (rightId == -1) throw new Exception();
        var right = PlanetDomainExt.GetPolyCell(rightId, d);
        var native = PlanetDomainExt.GetPolyCell(Native, d);
        if (isNative(right))
        {
            var foreign = PlanetDomainExt.GetPolyCell(Foreign, d);
            return FrontFace.Construct(right, foreign, d);
        }
        return FrontFace.Construct(native, right, d);
    }
    
    public void DoForNeighborsAlong(
        Func<FrontFace, bool> valid,
        Func<Cell, bool> isNative,
        bool toLeft, Action<FrontFace> action, Data d)
    {
        var curr = this;
        while (toLeft 
                   ? curr.GetLeftNeighborCellId(d) != -1 
                   : curr.GetRightNeighborCellId(d) != -1)
        {
            curr = toLeft ? curr.GetLeftNeighborFace(isNative, d) 
                : curr.GetRightNeighborFace(isNative, d);
            if (valid(curr) == false) return;
            action(curr);
        }
    }

    public List<FrontFace> GetFrontLeftToRightByControl(
        Regime r, Func<FrontFace, bool> valid, Data d)
    {
        return GetFrontLeftToRight(c => c.Controller.RefId == r.Id,
            valid, d);
    }
    public List<FrontFace> GetFrontLeftToRight(
        Func<Cell, bool> isNative,
        Func<FrontFace, bool> valid, Data d)
    {
        var res = new List<FrontFace>();
        var furthestLeft = this;
        while (furthestLeft.GetLeftNeighborCellId(d) != -1)
        {
            var nextLeft = furthestLeft.GetLeftNeighborFace(isNative, d);
            if (nextLeft.Equals(this) || valid(nextLeft) == false) break;
            furthestLeft = nextLeft;
        }
        res.Add(furthestLeft);
        var curr = furthestLeft;
        while (curr.GetRightNeighborCellId(d) != -1)
        {
            var nextRight = curr.GetRightNeighborFace(isNative, d);
            if (nextRight.Equals(furthestLeft) || valid(nextRight) == false) break;
            res.Add(nextRight);
            curr = nextRight;
        }
        
        return res;
    }

    public Vector2 GetMid(Data d)
    {
        var n = this.GetNative(d);
        var f = this.GetForeign(d);
        return (n.GetCenter() + n.GetCenter().Offset(f.GetCenter(), d) / 2f).ClampPosition(d);

    }


    public Vector2I GetIdEdgeKey()
    {
        return Native.GetIdEdgeKey(Foreign);
    }

    public (Vector2 leftJoin, Vector2 rightJoin) 
        GetJoinPoints(Data d)
    {
        Vector2 leftJoin = Vector2.Inf;
        Vector2 rightJoin = Vector2.Inf;
        var native = PlanetDomainExt.GetPolyCell(Native, d);
        var foreign = PlanetDomainExt.GetPolyCell(Foreign, d);

        var leftId = GetLeftNeighborCellId(d);
        var rightId = GetRightNeighborCellId(d);
        if (leftId != -1)
        {
            var left = PlanetDomainExt.GetPolyCell(leftId, d);
            leftJoin = native.AbsBoundary(d)
                .Intersect(foreign.AbsBoundary(d))
                .Intersect(left.AbsBoundary(d))
                .First();
            if (rightId == -1)
            {
                rightJoin = native.AbsBoundary(d)
                    .Intersect(foreign.AbsBoundary(d))
                    .Where(p => p != leftJoin)
                    .First();
            }
        }

        if (rightId != -1)
        {
            var right = PlanetDomainExt.GetPolyCell(rightId, d);
            rightJoin = native.AbsBoundary(d)
                .Intersect(foreign.AbsBoundary(d))
                .Intersect(right.AbsBoundary(d))
                .First();
            if (leftId == -1)
            {
                leftJoin = native.AbsBoundary(d)
                    .Intersect(foreign.AbsBoundary(d))
                    .Where(p => p != rightJoin)
                    .First();
            }
        }

        if (leftId == -1 && rightId == -1)
        {
            GD.Print($"bad edge at {Native} {Foreign} ");
            return (Vector2.Zero, Vector2.Zero);
        }

        return (leftJoin, rightJoin);
    }
}

public static class FrontFaceExt
{
    public static Vector2 GetAxis(this FrontFace f, Data d)
    {
        return f.GetNative(d).GetCenter().Offset(f.GetForeign(d).GetCenter(), d);
    }
    
    public static Cell GetNative(this FrontFace f, Data d)
    {
        return PlanetDomainExt.GetPolyCell(f.Native, d);
    }
    public static Cell GetForeign(this FrontFace f, Data d)
    {
        return PlanetDomainExt.GetPolyCell(f.Foreign, d);
    }

}

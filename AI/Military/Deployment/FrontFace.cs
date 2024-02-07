
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public struct FrontFace
{
    public int Native { get; private set; }
    public int Foreign { get; private set; }
    public int Left { get; private set; }
    public int Right { get; private set; }
    public static FrontFace
        Construct(PolyCell native, PolyCell foreign, Data d)
    {
        if (native is not LandCell || foreign is not LandCell)
        {
            return new FrontFace(native.Id, foreign.Id, -1, -1);
        }
        PolyCell left = null;
        PolyCell right = null;
        
        var nfAxis = native.GetCenter().GetOffsetTo(foreign.GetCenter(), d);
        var sharedNs = native.Neighbors
            .Intersect(foreign.Neighbors)
            .Distinct()
            .Select(i => PlanetDomainExt.GetPolyCell(i, d))
            .Where(n => n is LandCell)
            .ToArray();
        
        if (sharedNs.Length > 2)
        {
            GD.Print($"{sharedNs.Length} shared neighbors for {native.Id} {foreign.Id}");
            return new FrontFace(native.Id, foreign.Id, -1, -1);
        }
        
        for (var i = 0; i < sharedNs.Length; i++)
        {
            var sharedN = sharedNs[i];
            var nAxis = native.GetCenter().GetOffsetTo(sharedN.GetCenter(), d);
            var onLeft = nfAxis.GetCCWAngleTo(nAxis) < Mathf.Pi;
            if (onLeft)
            {
                if (left != null) throw new Exception();
                left = sharedN;
            }
            else
            {
                if (right != null) throw new Exception();
                right = sharedN;
            }
        }

        return new FrontFace(native.Id, foreign.Id,
            left is not null ? left.Id : -1,
            right is not null ? right.Id : -1);
    }
    private FrontFace(int native, int foreign,
        int left, int right)
    {
        Native = native;
        Foreign = foreign;
        Left = left;
        Right = right;
    }

    public FrontFace GetLeftNeighbor(Data d)
    {
        if (Left == -1) throw new Exception();
        var left = PlanetDomainExt.GetPolyCell(Left, d);
        var native = PlanetDomainExt.GetPolyCell(Native, d);
        if (left.Controller.RefId == native.Controller.RefId)
        {
            var foreign = PlanetDomainExt.GetPolyCell(Foreign, d);
            return FrontFace.Construct(left, foreign, d);
        }
        return FrontFace.Construct(native, left, d);
    }
    
    public FrontFace GetRightNeighbor(Data d)
    {
        if (Right == -1) throw new Exception();
        var right = PlanetDomainExt.GetPolyCell(Right, d);
        var native = PlanetDomainExt.GetPolyCell(Native, d);
        if (right.Controller.RefId == native.Controller.RefId)
        {
            var foreign = PlanetDomainExt.GetPolyCell(Foreign, d);
            return FrontFace.Construct(right, foreign, d);
        }
        return FrontFace.Construct(native, right, d);
    }
    
    public void DoForNeighborsAlong(
        Func<FrontFace, bool> valid,
        bool toLeft, Action<FrontFace> action, Data d)
    {
        var curr = this;
        while (toLeft ? curr.Left != null : curr.Right != null)
        {
            curr = toLeft ? curr.GetLeftNeighbor(d) 
                : curr.GetRightNeighbor(d);
            if (valid(curr) == false) return;
            action(curr);
        }
    }

    public List<FrontFace> GetFrontLeftToRight(
        Func<FrontFace, bool> valid, Data d)
    {
        var res = new List<FrontFace>();
        var furthestLeft = this;
        while (furthestLeft.Left != -1)
        {
            var nextLeft = furthestLeft.GetLeftNeighbor(d);
            if (nextLeft.Equals(this) || valid(nextLeft) == false) break;
            furthestLeft = nextLeft;
        }
        res.Add(furthestLeft);
        var curr = furthestLeft;
        while (curr.Right != -1)
        {
            var nextRight = curr.GetRightNeighbor(d);
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
        return (n.GetCenter() + n.GetCenter().GetOffsetTo(f.GetCenter(), d) / 2f).ClampPosition(d);

    }
}

public static class FrontFaceExt
{
    public static Vector2 GetAxis(this FrontFace f, Data d)
    {
        return f.GetNative(d).GetCenter().GetOffsetTo(f.GetForeign(d).GetCenter(), d);
    }
    
    public static PolyCell GetNative(this FrontFace f, Data d)
    {
        return PlanetDomainExt.GetPolyCell(f.Native, d);
    }
    public static PolyCell GetForeign(this FrontFace f, Data d)
    {
        return PlanetDomainExt.GetPolyCell(f.Foreign, d);
    }
}

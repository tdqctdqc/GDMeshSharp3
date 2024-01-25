
using System;
using System.Collections.Generic;
using System.Linq;

public struct FrontFace<T> where T : IIdentifiable
{
    public int Native { get; private set; }
    public int Foreign { get; private set; }

    public FrontFace(int native, int foreign)
    {
        Native = native;
        Foreign = foreign;
    }

    public IEnumerable<FrontFace<T>> GetNeighbors(
        Func<T, IEnumerable<T>> getNeighbors,
            Func<T, bool> isForeign, 
            Func<T, bool> isNative,
            Func<int, T> getElement)
    {
        var native = Native;
        var foreign = Foreign;
        
        return getNeighbors(getElement(Native))
            .Intersect(getNeighbors(getElement(Foreign)))
            .Where(n => isNative(n) || isForeign(n))
            .Select(n =>
            {
                if (isForeign(n))
                {
                    return new FrontFace<T>(native, n.Id);
                }

                return new FrontFace<T>(n.Id, foreign);
            });
    }
}

public static class FrontFaceExt
{
    public static PolyCell GetNative(this FrontFace<PolyCell> f, Data d)
    {
        return PlanetDomainExt.GetPolyCell(f.Native, d);
    }
    public static PolyCell GetForeign(this FrontFace<PolyCell> f, Data d)
    {
        return PlanetDomainExt.GetPolyCell(f.Foreign, d);
    }
}
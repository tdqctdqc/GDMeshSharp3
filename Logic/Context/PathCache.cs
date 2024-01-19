
using System;
using System.Collections.Generic;

public class PathCache : ThreadSafeCache<(MoveType moveType, Alliance a, PolyCell from, PolyCell to), List<PolyCell>>
{
    private Data _data;
    public PathCache(Data d) 
    {
        _data = d;
    }

    protected override List<PolyCell> Make((MoveType moveType, Alliance a, PolyCell from, PolyCell to) key)
    {
        var path = PathFinder.FindPath(key.moveType,
            key.a, key.from, key.to, _data);
        return path;
    }
}
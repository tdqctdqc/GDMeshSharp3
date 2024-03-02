
using System;
using System.Collections.Generic;

public class PathCache : ThreadSafeCache<(MoveType moveType, Alliance a, Cell from, Cell to), List<Cell>>
{
    private Data _data;
    public PathCache(Data d) 
    {
        _data = d;
    }

    protected override List<Cell> Make((MoveType moveType, Alliance a, Cell from, Cell to) key)
    {
        var path = PathFinder.FindPath(key.moveType,
            key.a, key.from, key.to, _data);
        return path;
    }
}
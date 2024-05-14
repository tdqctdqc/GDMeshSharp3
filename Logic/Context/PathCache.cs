
using System;
using System.Collections.Generic;

public class PathCache : ThreadSafeCache<(MoveType moveType, Alliance a, Cell from, Cell to), List<Cell>>
{
    private Data _data;
    private bool _thruRival;
    public PathCache(bool thruRival, Data d)
    {
        _thruRival = thruRival;
        _data = d;
    }
    
    public List<Cell> FindPath(MoveType m, Alliance a,
        Cell from, Cell to)
    {
        return GetOrAdd((m, a, from, to));
    }
    protected override List<Cell> Make((MoveType moveType, Alliance a, 
        Cell from, Cell to) key)
    {
        if (_thruRival)
        {
            var path = PathFinder.FindPathThroughFriendlyAndRival(key.moveType,
                key.a, key.from, key.to, _data);
            return path;
        }
        else
        {
            var path = PathFinder.FindPathThroughFriendly(key.moveType,
                key.a, key.from, key.to, _data);
            return path;
        }
    }
}
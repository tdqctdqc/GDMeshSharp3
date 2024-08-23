
using System;
using System.Collections.Generic;

public class PathCache : ThreadSafeCache<(MoveType moveType, Regime a, Cell from, Cell to), List<Cell>>
{
    private Data _data;
    private bool _thruRival;
    public PathCache(bool thruRival, Data d)
    {
        _thruRival = thruRival;
        _data = d;
    }
    public List<Cell> FindPath(MoveType m, Regime a,
        Cell from, Cell to)
    {
        var path = GetOrAdd((m, a, from, to));
        if (path is null)
        {
            var issue = new CantFindPathIssue(a,
                "Can't find path", from,
                new List<Cell> { to }, m, Game.I.Client.Data.GetTick());
            Game.I.Client.Data.ClientPlayerData.Issues.Add(issue);
        }
        return path;
    }
    protected override List<Cell> Make((MoveType moveType, Regime a, 
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
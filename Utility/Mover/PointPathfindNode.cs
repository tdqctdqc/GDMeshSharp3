using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PointPathfindNode : IMapPathfindNode
{
    public Vector2 Pos { get; private set; }
    IEnumerable<IMapPathfindNode> IMapPathfindNode.Neighbors(Data d) => Neighbors;
    public HashSet<IMapPathfindNode> Neighbors { get; private set; }
    public PolyTriPosition Tri { get; private set; }
    private static float _joinDist = 50f;
    public PointPathfindNode(Vector2 pos, MoveType moveType,
        Alliance a, Data d)
    {
        Pos = pos;
        Tri = d.Context.GetPolyTri(pos, d).GetPosition();
        Neighbors = d.Military.WaypointGrid
            .GetWithin(pos, _joinDist, v => true)
            .Where(wp => moveType.Passable(wp, a, d))
            .AsEnumerable<IMapPathfindNode>()
            .ToHashSet();
        if (Neighbors.Count == 0)
        {
            Neighbors = d.Military.WaypointGrid
                .GetWithin(pos, _joinDist * 2f, v => true)
                .Where(wp => moveType.Passable(wp, a, d))
                .AsEnumerable<IMapPathfindNode>()
                .ToHashSet();
            // if (Neighbors.Count == 0) throw new Exception();
        }
    }

    public static void Join(PointPathfindNode p1, 
        PointPathfindNode p2, Data d)
    {
        var dist = p1.Pos.GetOffsetTo(p2.Pos, d).Length();
        if (dist <= _joinDist)
        {
            p1.Neighbors.Add(p2);
            p2.Neighbors.Add(p1);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class FrontSegmentAssignment : ForceAssignment
{
    public Vector2 Center { get; private set; }
    public Vector2 Right { get; set; }
    public Vector2 RightJoin { get; set; }
    public Vector2 Left { get; set; }
    public Vector2 LeftJoin { get; set; }
    public HashSet<int> HeldWaypointIds { get; private set; }

    public static FrontSegmentAssignment Construct(
        EntityRef<Regime> r,
        IEnumerable<Waypoint> heldWaypoints,
        Data d)
    {
        return new FrontSegmentAssignment(heldWaypoints.Select(wp => wp.Id).ToHashSet(),
            Vector2.Zero,
            Vector2.Zero,
            Vector2.Zero,
            Vector2.Zero,
            Vector2.Zero,
            new HashSet<int>(),
            r);
    }
    [SerializationConstructor] private FrontSegmentAssignment(
        HashSet<int> heldWaypointIds,
        Vector2 center,
        Vector2 left,
        Vector2 right,
        Vector2 leftJoin,
        Vector2 rightJoin,
        HashSet<int> groupIds, 
        EntityRef<Regime> regime) 
        : base(groupIds, regime)
    {
        HeldWaypointIds = heldWaypointIds;
        Center = center;
        Left = left;
        Right = right;
        RightJoin = rightJoin;
        LeftJoin = leftJoin;
    }

    public void CalcLeftRight(Vector2 relTo, Data d)
    {
        var alliance = Regime.Entity(d).GetAlliance(d);
        var segmentWps = GetHeldWaypoints(d);
        Center = d.Planet.GetAveragePosition(segmentWps.Select(wp => wp.Pos));
        var relCenter = relTo.GetOffsetTo(Center, d);
        var threatDir = segmentWps
            .Select(wp =>
            {
                return wp
                    .TacNeighbors(d)
                    .Where(n => n.IsDirectlyThreatened(alliance, d))
                    .Select(n => wp.Pos.GetOffsetTo(n.Pos, d))
                    .Sum();
            })
            .Sum().Normalized();
        var axis = threatDir.Rotated(-Mathf.Pi / 2f);
            
        var rightDist = 0f;
        var leftDist = 0f;
        foreach (var segmentWp in segmentWps)
        {
            var wpRelPos = relTo.GetOffsetTo(segmentWp.Pos, d);
            var offset = wpRelPos - relCenter;
            var dist = offset.Length();
            if (threatDir.GetClockwiseAngleTo(offset) < Mathf.Pi
                && dist > rightDist)
            {
                rightDist = dist;
            }
            else if(dist > leftDist)
            {
                leftDist = dist;
            }
        }

        Right = (Center + axis * rightDist).ClampPosition(d);
        RightJoin = Right;
        Left = (Center - axis * leftDist).ClampPosition(d);
        LeftJoin = Left;
    }
    public override void CalculateOrders(MinorTurnOrders orders, LogicWriteKey key)
    {
        // if (GroupIds.Count == 0) return;
        var areaRadius = 500f;
        if (GroupIds.Count() == 0) return;
        Assigner.AssignAlongLine<Vector2, int>(
            new List<Vector2>{LeftJoin, Left, Center, Right, RightJoin},
            GroupIds.ToList(),
            g => key.Data.Get<UnitGroup>(g).GetPowerPoints(key.Data),
            (v,w) => 1f,
            wp => wp,
            (v1, v2) => v1.GetOffsetTo(v2, key.Data),
            (g, l) =>
            {
                var order = new DeployOnLineOrder(l);
                var proc = new SetUnitOrderProcedure(new EntityRef<UnitGroup>(g), order);
                key.SendMessage(proc);
            }
        );
    }

    public IEnumerable<Waypoint> GetHeldWaypoints(Data d)
    {
        return HeldWaypointIds.Select(id => MilitaryDomain.GetTacWaypoint(id, d));
    }

    public static void CalcPositions(FrontAssignment fa, Data d)
    {
        var segments = fa.Segments;
        var front = fa.Front;
        foreach (var seg in segments)
        {
            seg.CalcLeftRight(front.RelTo(d), d);
        }
        if (segments.Count < 2) return;

        var nearestRightDic = new Dictionary<FrontSegmentAssignment, 
            (FrontSegmentAssignment, Vector2)>();
        var nearestLeftDic = new Dictionary<FrontSegmentAssignment,
            (FrontSegmentAssignment, Vector2)>();
        var points = segments.Select(s => s.Left)
            .Union(segments.Select(s => s.Right))
            .ToList();
        for (var i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            var nearLeftDist = float.PositiveInfinity;
            Vector2 nearLeft = Vector2.Inf;
            FrontSegmentAssignment nearLeftFsa = null;
            var nearRightDist = float.PositiveInfinity;
            Vector2 nearRight = Vector2.Inf;
            FrontSegmentAssignment nearRightFsa = null;
            foreach (var s2 in segments)
            {
                if (s2 == seg) continue;
                checkP(ref nearLeftFsa, s2, 
                    seg.Left, s2.Left, ref nearLeftDist, ref nearLeft);
                checkP(ref nearLeftFsa, s2, 
                    seg.Left, s2.Right, ref nearLeftDist, ref nearLeft);
                checkP(ref nearRightFsa, s2, 
                    seg.Right, s2.Right, ref nearRightDist, ref nearRight);
                checkP(ref nearRightFsa, s2, 
                    seg.Right, s2.Left, ref nearRightDist, ref nearRight);
            }

            void checkP(ref FrontSegmentAssignment fsa,
                FrontSegmentAssignment fsaCand, 
                Vector2 p,
                Vector2 pCand, 
                ref float dist, ref Vector2 v)
            {
                var testDist = pCand.GetOffsetTo(p, d).Length();
                if (testDist >= dist) return;
                dist = testDist;
                v = pCand;
                fsa = fsaCand;
            }
            nearestLeftDic.Add(seg, (nearLeftFsa, nearLeft));
            nearestRightDic.Add(seg, (nearRightFsa, nearRight));
        }
        
        foreach (var seg in segments)
        {
            var (nearestLeftFsa, nearestLeft) = nearestLeftDic[seg];
            var lAxis = seg.Center.GetOffsetTo(seg.Left, d);
            var nearLAxis = seg.Left.GetOffsetTo(nearestLeft, d);
            if (nearestLeftDic[nearestLeftFsa].Item1 == seg)
            {
                seg.LeftJoin = seg.Left + nearLAxis / 2f;
                var closestOnAxis = seg.LeftJoin
                    .GetClosestPointOnLineSegment(seg.Left - lAxis, seg.Left);
                seg.Left = closestOnAxis;
            }
            
            var (nearestRightFsa, nearestRight) = nearestRightDic[seg];
            var rAxis = seg.Center.GetOffsetTo(seg.Right, d);
            var nearRAxis = seg.Right.GetOffsetTo(nearestRight, d);

            if (nearestRightDic[nearestRightFsa].Item1 == seg)
            {
                seg.RightJoin = seg.Right + seg.Right.GetOffsetTo(nearestRight, d) / 2f;
                var closestOnAxis = seg.RightJoin
                    .GetClosestPointOnLineSegment(seg.Right - rAxis, seg.Right);
                seg.Right = closestOnAxis;
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Godot;

public class CantFindWaypointPathIssue : Issue
{
    public Waypoint Start { get; private set; }
    public Waypoint Dest { get; private set; }
    public MoveType MoveType { get; private set; }
    public Alliance Alliance { get; private set; }

    public CantFindWaypointPathIssue(Vector2 point, 
        Alliance alliance,
        string message, Waypoint start, Waypoint dest, MoveType moveType) 
        : base(point, message)
    {
        Alliance = alliance;
        Start = start;
        Dest = dest;
        MoveType = moveType;
    }

    public override void Draw(Client c)
    {
        var startNeighborhood = new HashSet<Waypoint>();
        var destNeighborhood = new HashSet<Waypoint>();
        startNeighborhood.Add(Start);
        destNeighborhood.Add(Dest);
        int iter = 0;
        bool touched = false;
        while (iter < 5 || touched == false)
        {
            if (touched) iter++;
            
            var moreNeighbors = check(startNeighborhood, destNeighborhood);
            if (moreNeighbors == false) break;
            moreNeighbors = check(destNeighborhood, startNeighborhood);
            if (moreNeighbors == false) break;

            bool check(HashSet<Waypoint> oldWps, HashSet<Waypoint> otherWps)
            {
                var newWps = oldWps
                    .SelectMany(wp => wp.TacNeighbors(c.Data))
                    .Where(wp => startNeighborhood.Contains(wp) == false)
                    .ToArray();
                if (newWps.Length == 0) return false;
                foreach (var newWp in newWps)
                {
                    if (touched == false)
                    {
                        if (otherWps.Contains(newWp)) touched = true;
                    }
                    oldWps.Add(newWp);
                }

                return true;
            }
        }

        var highlighter = c.GetComponent<MapGraphics>().Highlighter;
        highlighter.Clear();
        var union = startNeighborhood.Union(destNeighborhood).Distinct();
        foreach (var wp in union)
        {
            Color canPass = MoveType.Passable(wp, Alliance, c.Data)
                ? Colors.White : Colors.Black;
            Color isStartOrDest = Colors.White;
            var size = 10f;
            if (wp == Start)
            {
                isStartOrDest = Colors.Green;
            }
            else if (wp == Dest)
            {
                isStartOrDest = Colors.Red;
            }
            else
            {
                isStartOrDest = Colors.Yellow;
                size = 5f;
            }
            highlighter.DrawPoint(wp.Pos, size, canPass);
            highlighter.DrawPoint(wp.Pos, size / 2f, isStartOrDest);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Godot;

public class CantFindPathIssue : Issue
{
    public IMapPathfindNode Start { get; private set; }
    public IMapPathfindNode Dest { get; private set; }
    public MoveType MoveType { get; private set; }
    public Alliance Alliance { get; private set; }
    public CantFindPathIssue(Vector2 point, 
        Alliance alliance,
        string message, IMapPathfindNode start, IMapPathfindNode dest, 
        MoveType moveType) 
        : base(point, message)
    {
        Alliance = alliance;
        Start = start;
        Dest = dest;
        MoveType = moveType;
    }

    public override void Draw(Client c)
    {
        var startNeighborhood = new HashSet<IMapPathfindNode>();
        var destNeighborhood = new HashSet<IMapPathfindNode>();
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

            bool check(HashSet<IMapPathfindNode> oldWps, 
                HashSet<IMapPathfindNode> otherWps)
            {
                var newWps = oldWps
                    .SelectMany(wp => wp.Neighbors(c.Data))
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

        var debugDrawer = c.GetComponent<MapGraphics>()
            .DebugOverlay;
        debugDrawer.Clear();
        var union = startNeighborhood.Union(destNeighborhood).Distinct();
        foreach (var n in union)
        {
            Color canPass;
            if (n is Waypoint wp)
            {
                canPass = MoveType.Passable(wp, Alliance, c.Data)
                    ? Colors.White : Colors.Black;
            }
            else
            {
                 canPass = Colors.Gray;
            }
            
            Color isStartOrDest = Colors.White;
            var size = 10f;
            if (n == Start)
            {
                isStartOrDest = Colors.Green;
            }
            else if (n == Dest)
            {
                isStartOrDest = Colors.Red;
            }
            else
            {
                isStartOrDest = Colors.Yellow;
                size = 5f;
            }
            debugDrawer.Draw(mb => mb.AddPoint(Vector2.Zero, size, canPass), 
                n.Pos);
            debugDrawer.Draw(mb => mb.AddPoint(Vector2.Zero, size / 2f, isStartOrDest), 
                n.Pos);
        }
    }
}
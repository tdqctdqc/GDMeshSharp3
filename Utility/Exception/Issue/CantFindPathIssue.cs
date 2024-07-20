
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CantFindPathIssue : Issue
{
    public Cell Start { get; private set; }
    public List<Cell> Dests { get; private set; }
    public MoveType MoveType { get; private set; }
    public Alliance Alliance { get; private set; }
    public CantFindPathIssue(
        Alliance alliance,
        string message, 
        Cell start, 
        List<Cell> dests, 
        MoveType moveType) 
        : base(start.GetCenter(), message)
    {
        Alliance = alliance;
        Start = start;
        Dests = dests;
        MoveType = moveType;
    }

    public override void Draw(Client c)
    {
        var startNeighborhood = new HashSet<Cell>();
        var destNeighborhood = new HashSet<Cell>();
        startNeighborhood.Add(Start);
        destNeighborhood.AddRange(Dests);
        int iter = 0;
        bool touched = false;
        while (iter < 5 || touched == false)
        {
            if (touched) iter++;
            
            var moreNeighbors = check(startNeighborhood, destNeighborhood);
            if (moreNeighbors == false) break;
            moreNeighbors = check(destNeighborhood, startNeighborhood);
            if (moreNeighbors == false) break;

            bool check(HashSet<Cell> oldWps, 
                HashSet<Cell> otherWps)
            {
                var newWps = oldWps
                    .SelectMany(wp => wp.GetNeighbors(c.Data))
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
            .GetOverlay(LayerOrder.Debug);
        debugDrawer.Clear();
        var union = startNeighborhood.Union(destNeighborhood).Distinct();
        foreach (var n in union)
        {
            Color canPass = canPass = MoveType.PassableFriendly(n, Alliance, c.Data)
                ? Colors.White : Colors.Black;
            
            Color isStartOrDest = Colors.White;
            var size = 10f;
            if (n == Start)
            {
                isStartOrDest = Colors.Green;
            }
            else if (Dests.Contains(n))
            {
                isStartOrDest = Colors.Red;
            }
            else
            {
                isStartOrDest = Colors.Yellow;
                size = 5f;
            }
            debugDrawer.Draw(mb => mb.AddSquare(Vector2.Zero, size, canPass), 
                n.GetCenter());
            debugDrawer.Draw(mb => mb.AddSquare(Vector2.Zero, size / 2f, isStartOrDest), 
                n.GetCenter());
        }
    }
}
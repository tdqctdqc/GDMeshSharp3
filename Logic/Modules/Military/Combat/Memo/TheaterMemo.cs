
using System.Collections.Generic;
using System.Linq;

public class TheaterMemo
{
    public HashSet<PolyCell> Cells { get; private set; }
    public HashSet<UnitGroup> Reserves { get; private set; }
    public HashSet<DeploymentBranch> Children { get; private set; }
    public TheaterMemo(Theater theater, Data d)
    {
        Cells = theater.GetCells(d).ToHashSet();
        Reserves = theater.Reserve.Groups.Select(g => g.Entity(d)).ToHashSet();
        Children = theater.Children()
            .OfType<DeploymentBranch>()
            .Where(b => b is FrontSegment == false)
            .ToHashSet();
    }

    public TheaterMemo(HashSet<PolyCell> cells, HashSet<UnitGroup> reserves, HashSet<DeploymentBranch> children)
    {
        Cells = cells;
        Reserves = reserves;
        Children = children;
    }
}
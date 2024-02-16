
using System.Collections.Generic;

public class Theater
{
    public HashSet<PolyCell> Cells { get; private set; }
    public HashSet<Frontline> Frontlines { get; private set; }

    public Theater(HashSet<PolyCell> cells, HashSet<Frontline> frontlines)
    {
        Cells = cells;
        Frontlines = frontlines;
    }
}
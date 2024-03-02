
using System.Collections.Generic;

public class Theater
{
    public HashSet<Cell> Cells { get; private set; }
    public HashSet<Frontline> Frontlines { get; private set; }

    public Theater(HashSet<Cell> cells, HashSet<Frontline> frontlines)
    {
        Cells = cells;
        Frontlines = frontlines;
    }
}
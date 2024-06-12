
using System.Collections.Generic;

public class ArmyPosition
{
    public CellRef HomeCell { get; private set; }
    public HashSet<int> Cells { get; private set; }
}
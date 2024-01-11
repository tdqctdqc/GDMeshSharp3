using System.Linq;

public class CombatGraphNode
{
    public Unit Unit { get; private set; }
    public CombatGraphNode(Unit unit)
    {
        Unit = unit;
    }
}


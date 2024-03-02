
using System.Linq;

public class UnitAttackEdge : UnitCombatEdge
{
    public CellAttackNode AttackNode { get; private set; }

    public override ICombatGraphNode Node2 => AttackNode;

    public static UnitAttackEdge ConstuctAndAddToGraph
        (Cell target, Unit u, CombatCalculator combat, Data d)
    {
        var e = new UnitAttackEdge(target, u, combat, d);
        combat.Graph.AddEdge(e, d);
        return e;
    }
    protected UnitAttackEdge(Cell target, Unit u, 
        CombatCalculator combat, Data d)
        : base(u)
    {
        AttackNode = combat.GetOrAddCellAttackNode(target, d);
    }

    public override void CalculateCombat(CombatCalculator combat, Data d)
    {
    }
    public override void DirectResults(CombatCalculator combat, LogicWriteKey key)
    {
    }
    public override void InvoluntaryResults(CombatCalculator combat, LogicWriteKey key)
    {
    }
    public override void VoluntaryResults(CombatCalculator combat, LogicWriteKey key)
    {
    }
}
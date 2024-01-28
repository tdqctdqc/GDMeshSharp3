
using System.Linq;

public class UnitAttackEdge : UnitCombatEdge
{
    public override void PrepareGraph(CombatCalculator combat, 
        ICombatGraphNode n1, ICombatGraphNode n2, Data d)
    {
        var (unit, target) = GetNodes(n1, n2);
        var alliance = unit.Regime.Entity(d).GetAlliance(d);
        var cellAttackNode = combat.CellAttackNodes[(alliance, target)];
        combat.Graph.AddEdge(unit, target, this, d);
    }

    public override void Calculate(CombatCalculator combat, ICombatGraphNode n1, ICombatGraphNode n2, Data d)
    {
        throw new System.NotImplementedException();
    }

    public override void DirectResults(CombatCalculator combat, ICombatGraphNode n1, ICombatGraphNode n2, Data d)
    {
        throw new System.NotImplementedException();
    }

    public override void InvoluntaryResults(CombatCalculator combat, ICombatGraphNode n1, ICombatGraphNode n2, Data d)
    {
        throw new System.NotImplementedException();
    }

    public override void VoluntaryResults(CombatCalculator combat, ICombatGraphNode n1, ICombatGraphNode n2, Data d)
    {
        throw new System.NotImplementedException();
    }

    protected override Unit GetUnit(CombatCalculator combat, ICombatGraphNode n1, ICombatGraphNode n2, Data d)
    {
        return GetNodes(n1, n2).unit;
    }

    private (Unit unit, PolyCell target) GetNodes(ICombatGraphNode n1,
        ICombatGraphNode n2)
    {
        if (n1 is Unit) return ((Unit)n1, (PolyCell)n2);
        return ((Unit)n2, (PolyCell)n1);
    }
}
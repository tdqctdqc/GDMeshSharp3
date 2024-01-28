
public abstract class UnitCombatEdge : ICombatGraphEdge
{
    public abstract void PrepareGraph(CombatCalculator combat, ICombatGraphNode n1, ICombatGraphNode n2, Data d);
    public abstract void Calculate(CombatCalculator combat, ICombatGraphNode n1, ICombatGraphNode n2, Data d);
    public abstract void DirectResults(CombatCalculator combat, ICombatGraphNode n1, ICombatGraphNode n2, Data d);
    public abstract void InvoluntaryResults(CombatCalculator combat, ICombatGraphNode n1, ICombatGraphNode n2, Data d);
    public abstract void VoluntaryResults(CombatCalculator combat, ICombatGraphNode n1, ICombatGraphNode n2, Data d);
    public bool Suppressed(CombatCalculator combat, 
        ICombatGraphNode n1, ICombatGraphNode n2, Data d)
    {
        var u = GetUnit(combat, n1, n2, d);
        return combat.Suppressed.Contains(u);
    }

    protected abstract Unit GetUnit(CombatCalculator combat,
        ICombatGraphNode n1, ICombatGraphNode n2, Data d);
}
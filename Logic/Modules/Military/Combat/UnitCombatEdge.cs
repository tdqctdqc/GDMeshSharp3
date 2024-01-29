
public abstract class UnitCombatEdge : ICombatGraphEdge
{
    public Unit Unit { get; private set; }
    ICombatGraphNode ICombatGraphEdge.Node1 => Unit;
    public abstract ICombatGraphNode Node2 { get; }
    public abstract void CalculateCombat(CombatCalculator combat, Data d);
    public abstract void DirectResults(CombatCalculator combat, LogicWriteKey key);
    public abstract void InvoluntaryResults(CombatCalculator combat, LogicWriteKey key);
    public abstract void VoluntaryResults(CombatCalculator combat, LogicWriteKey key);

    protected UnitCombatEdge(Unit u)
    {
        Unit = u;
    }
    public bool Suppressed(CombatCalculator combat, 
        Data d)
    {
        return combat.Suppressed.Contains(Unit);
    }
}
using System.Collections.Generic;

public interface ICombatGraphEdge
{
    ICombatGraphNode Node1 { get; }
    ICombatGraphNode Node2 { get; }
    void CalculateCombat(CombatCalculator combat,
        Data d);
    void DirectResults(CombatCalculator combat,
        LogicWriteKey key);
    void InvoluntaryResults(CombatCalculator combat,
        LogicWriteKey key);
    void VoluntaryResults(CombatCalculator combat,
        LogicWriteKey key);

    bool Suppressed(CombatCalculator combat,
        Data d);
}
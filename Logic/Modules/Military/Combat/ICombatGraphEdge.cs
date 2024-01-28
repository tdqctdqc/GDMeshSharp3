using System.Collections.Generic;

public interface ICombatGraphEdge
{
    void PrepareGraph(CombatCalculator combat,
        ICombatGraphNode n1, ICombatGraphNode n2,
        Data d);
    void Calculate(CombatCalculator combat,
        ICombatGraphNode n1, ICombatGraphNode n2,
        Data d);
    void DirectResults(CombatCalculator combat,
        ICombatGraphNode n1, ICombatGraphNode n2,
        Data d);
    void InvoluntaryResults(CombatCalculator combat,
        ICombatGraphNode n1, ICombatGraphNode n2,
        Data d);
    void VoluntaryResults(CombatCalculator combat,
        ICombatGraphNode n1, ICombatGraphNode n2,
        Data d);

    bool Suppressed(CombatCalculator combat,
        ICombatGraphNode n1, ICombatGraphNode n2,
        Data d);
}
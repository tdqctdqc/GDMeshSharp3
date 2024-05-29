using System.Linq;

public interface ICombatGraphNode : IIdentifiable
{
    void DistributeResources(CombatCalculator combat, 
        Data d);
    void CalculateCombat(CombatCalculator combat,
        Data d);
    void DirectResults(CombatCalculator combat,
        LogicWriteKey key);
    void InvoluntaryResults(CombatCalculator combat,
        LogicWriteKey key);
    void VoluntaryResults(CombatCalculator combat,
        LogicWriteKey key);
}


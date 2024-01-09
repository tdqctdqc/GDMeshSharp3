
public class RetreatAction : CombatAction
{
    public override Unit[] GetCombatGraphTargets(Unit u, Data d)
    {
        return null;
    }

    public override CombatResult CalcResult(Unit u, CombatCalculator.CombatCalcData cData, Data d)
    {
        throw new System.NotImplementedException();
    }
}
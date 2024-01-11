public abstract class DefendNode : CombatGraphNode
{
    public DefendNode(Unit unit) 
        : base(unit)
    {
    }

    public abstract void DetermineDefenseResult(CombatCalculator.CombatCalcData cData,
        Data d);
}

using Godot;

public abstract class CombatAction
{
    public abstract Unit[] GetCombatGraphTargets(Unit u, Data d);
    public abstract CombatResult CalcResult(
        Unit u,
        CombatCalculator.CombatCalcData cData,
        Data d);
}
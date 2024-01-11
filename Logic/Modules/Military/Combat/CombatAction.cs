
using Godot;

public abstract class CombatAction
{
    public abstract Unit[] GetCombatGraphTargets(Unit u, Data d);
}
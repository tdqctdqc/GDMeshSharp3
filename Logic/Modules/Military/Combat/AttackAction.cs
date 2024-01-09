
public abstract class AttackAction : CombatAction
{
    public abstract void CalculateLosses(AttackNode attacker, 
        DefendNode defender, CombatGraphEdge edge, Data d);
}
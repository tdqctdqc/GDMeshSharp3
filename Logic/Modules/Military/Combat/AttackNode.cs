public class AttackNode : CombatGraphNode
{
    public AttackAction Action { get; private set; }

    public AttackNode(Unit unit, AttackAction action) 
        : base(unit)
    {
        Action = action;
    }
}
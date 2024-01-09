public class CombatGraphNode
{
    public Unit Unit { get; private set; }
    public CombatGraphNode(Unit unit)
    {
        Unit = unit;
    }
}
public class AttackNode : CombatGraphNode
{
    public AttackAction Action { get; private set; }

    public AttackNode(Unit unit, AttackAction action) 
        : base(unit)
    {
        Action = action;
    }
}
public class DefendNode : CombatGraphNode
{
    public DefendNode(Unit unit) 
        : base(unit)
    {
    }
}
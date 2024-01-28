
public static class CombatExt
{
    public static CellAttackNode GetCellAttackNode(
        this CombatCalculator combat, Alliance a, 
        PolyCell target, Data d)
    {
        if (combat.CellAttackNodes.ContainsKey((a, target)) == false)
        {
            // var node = new CellAttackNode();
            // var edge = new CellAttackEdge();
            // combat.Graph.AddEdge(node, target, edge, d);
        }

        return combat.CellAttackNodes[(a, target)];
    }
}
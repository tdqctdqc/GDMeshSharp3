
using System.Linq;

public class ArmyCombatHistory
{
    public ERef<Unit>[] Units { get; private set; }
    public CellRef[] Occupied { get; private set; }
    public CellRef[] Attacked { get; private set; }
    public CellRef[] Defended { get; private set; }

    public static ArmyCombatHistory Construct(Army army,
        CombatGraph graph)
    {
        var edges = graph.GetNodeEdges(army);
        var units = army.Units.Refs.ToArray();
        var occupied = army.Cells.Refs.ToArray();
        var attacks = edges
            .OfType<ArmyAttackEdge>()
            .Select(e => e.CellCombatNode.Cell.MakeRef())
            .ToArray();
        var defends = edges.OfType<ArmyDefendEdge>()
            .Select(e => e.CellCombatNode.Cell.MakeRef())
            .ToArray();
        return new ArmyCombatHistory(units, occupied, attacks, defends);
    }
    public ArmyCombatHistory(
        ERef<Unit>[] units,
        CellRef[] occupied, 
        CellRef[] attacked, 
        CellRef[] defended)
    {
        Units = units;
        Occupied = occupied;
        Attacked = attacked;
        Defended = defended;
    }
}
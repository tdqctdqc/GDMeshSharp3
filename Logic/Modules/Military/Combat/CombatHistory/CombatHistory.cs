
using System.Collections.Generic;
using MessagePack;

public class CombatHistory
{
    public int Tick { get; private set; }
    public Dictionary<CellRef, CellCombatHistory> CellCombatHistories { get; private set; }
    public Dictionary<ERef<Army>, ArmyCombatHistory> ArmyCombatHistories { get; private set; }
    public static CombatHistory Construct(int tick)
    {
        return new CombatHistory(tick,
            new Dictionary<ERef<Army>, ArmyCombatHistory>(),
            new Dictionary<CellRef, CellCombatHistory>());
    }
    [SerializationConstructor] public CombatHistory(int tick,
        Dictionary<ERef<Army>, ArmyCombatHistory> armyCombatHistories, 
        Dictionary<CellRef, CellCombatHistory> cellCombatHistories)
    {
        Tick = tick;
        ArmyCombatHistories = armyCombatHistories;
        CellCombatHistories = cellCombatHistories;
    }

    public void DoCombatStage(CombatGraph graph)
    {
        foreach (var node in graph.GetNodes())
        {
            if (node is CellCombatNode cellNode)
            {
                var cellHistory = CellCombatHistory.Construct(cellNode);
                CellCombatHistories.Add(cellNode.Cell.MakeRef(), cellHistory);
            }
            else if (node is Army a)
            {
                var armyHistory = ArmyCombatHistory.Construct(a, graph);
                ArmyCombatHistories.Add(a.MakeRef(), armyHistory);
            }
        }
    }
}
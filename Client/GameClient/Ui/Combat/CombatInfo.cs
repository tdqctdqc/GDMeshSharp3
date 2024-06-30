using System.Linq;

namespace Ui.Combat;

public class CombatInfo
{
    public Landform Landform { get; private set; }
    public Vegetation Vegetation { get; private set; }
    public UnitCombatInfo[] Attackers { get; private set; }
    public UnitCombatInfo[] Defenders { get; private set; }
    public bool DefendersForcedBack { get; private set; }

    public CombatInfo()
    {
        
    }
    public void Setup(Landform landform, Vegetation vegetation, UnitCombatInfo[] attackers, UnitCombatInfo[] defenders, bool defendersForcedBack)
    {
        Landform = landform;
        Vegetation = vegetation;
        Attackers = attackers;
        Defenders = defenders;
        DefendersForcedBack = defendersForcedBack;
    }

    public void Setup(CellDefenseNode node, CombatGraph graph,
        Data d)
    {
        Landform = node.Cell.Get(d).Landform.Get(d);
        Vegetation = node.Cell.Get(d).Vegetation.Get(d);
        Defenders = node.UnitInfos.ToArray();
        Attackers = node.GetAttackers(graph).ToArray();
        DefendersForcedBack = node.DefendersForcedBack;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Army : Entity, ICombatGraphNode
{
    public ERef<Regime> Regime { get; private set; }
    public ERefSet<Unit> Units { get; private set; }
    public HashSet<int> Cells { get; private set; }
    public UnitGroupOrder GroupOrder { get; private set; }
    public Color Color { get; private set; }
    public MoveType MoveType(Data d) => Units.Items(d)
        .FirstOrDefault()?.Template.Get(d).MoveType.Get(d);
    public static Army Create(Regime r, 
        Cell startCell,
        IEnumerable<int> unitIds, ICreateWriteKey key)
    {
        var id = key.Data.IdDispenser.TakeId();
        var units = ERefSet<Unit>.Construct(nameof(Units), id, unitIds.ToHashSet());
        var u = new Army(id, r.MakeRef(), units,
            new DoNothingUnitGroupOrder(),
            new HashSet<int>{startCell.Id},
            ColorsExt.GetRandomColor());
        key.Create(u);
        return u;
    }
    [SerializationConstructor] private Army(int id,
        ERef<Regime> regime, 
        ERefSet<Unit> units,
        UnitGroupOrder groupOrder,
        HashSet<int> cells,
        Color color) 
        : base(id)
    {
        Regime = regime;
        Units = units;
        GroupOrder = groupOrder;
        Color = color;
        Cells = cells;
    }

    public static void ChangeUnitGroup(Unit u, 
        Army oldG, Army newG,
        ProcedureWriteKey key)
    {
        oldG?.Units.Remove(u, key);
        newG?.Units.Add(u, key);
        key.Data.Notices.Military.UnitChangedGroup.Invoke(u, newG, oldG);
    }

    public Cell GetHomeCell(Data d)
    {
        return PlanetDomainExt.GetPolyCell(Cells.Min(), d);
    }
    public HashSet<Cell> GetCells(Data d)
    {
        return Cells.Select(c => PlanetDomainExt.GetPolyCell(c, d)).ToHashSet();
    }
    public void SetOrder(UnitGroupOrder groupOrder, ProcedureWriteKey key)
    {
        GroupOrder = groupOrder;
    }

    public float GetPowerPoints(Data data)
    {
        return Units.Items(data).Sum(u => u.GetPowerPoints(data));
    }

    public override void CleanUp(StrongWriteKey key)
    {
        if (Units.Count() > 0) throw new Exception();
    }

    public void SetCells(IEnumerable<int> cells, ProcedureWriteKey key)
    {
        Cells.Clear();
        Cells.UnionWith(cells);
    }

    public void DistributeResources(CombatCalculator combat, Data d)
    {
        GD.Print("distributing");
        var edges = combat.Graph
            .GetNodeEdges(this);
        var edgeNeeds = edges
            .ToDictionary(e => e, 
                e =>
                {
                    float demand = 0f;
                    if (e is ArmyAttackEdge atk)
                    {
                        demand = atk.CellCombatNode.GetPotentialDefendingPower(d, combat);
                    }
                    else if (e is ArmyDefendEdge def)
                    {
                        demand = def.CellCombatNode.GetPotentialAttackingPower(d, combat);
                    }
                    else throw new Exception();

                    return demand;
                });
        var assigns = 
            Assigner.AssignFractional<ICombatGraphEdge, Unit>(
                edges,
                Units.Items(d).ToList(),
                e => edgeNeeds[e],
                u => u.GetPowerPoints(d)
            );
        for (var i = 0; i < assigns.Count; i++)
        {
            var (edge, unit, proportion) = assigns[i];
            if (edge is ArmyAttackEdge atk)
            {
                GD.Print("adding attacker proportion " + proportion);
                atk.Attackers.Add((unit, proportion));
            }
            else if (edge is ArmyDefendEdge def)
            {
                def.Defenders.Add((unit, proportion));
            }
            else throw new Exception();
        } 
    }

    public void CalculateCombat(CombatCalculator combat, Data d)
    {
    }

    public void DirectResults(CombatCalculator combat, LogicWriteKey key)
    {
    }

    public void InvoluntaryResults(CombatCalculator combat, LogicWriteKey key)
    {
    }

    public void VoluntaryResults(CombatCalculator combat, LogicWriteKey key)
    {
    }
}
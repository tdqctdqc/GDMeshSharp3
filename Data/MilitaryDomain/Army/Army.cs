
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Army : Entity, ICombatGraphNode
{
    public ERef<Regime> Regime { get; private set; }
    public ERefSetCallback<Unit> Units { get; private set; }
    public RefSetCallback<CellRef> Cells { get; private set; }
    public LineMission LineMission { get; private set; }
    public HashSet<ArmyMission> OtherOrders { get; private set; }
    public Color Color { get; private set; }
    public MoveType MoveType(Data d) => Units.Entities(d)
        .FirstOrDefault()?.Template.Get(d).MoveType.Get(d);
    public static Army Create(Regime r, 
        IEnumerable<Cell> startCells,
        IEnumerable<int> unitIds, IHostWriteKey key)
    {
        var id = key.Data.IdDispenser.TakeId();
        var units = ERefSetCallback<Unit>.Construct
            (unitIds.Select(id => new ERef<Unit>(id))
                .ToHashSet());
        var u = new Army(id, r.MakeRef(), units,
            new LineMission(startCells.Select(c => c.Id).ToHashSet(),
                new HashSet<int>(), false),
            new HashSet<ArmyMission>(),
            RefSetCallback<CellRef>.Construct(startCells.Select(c => c.MakeRef())),
            ColorsExt.GetRandomColor());
        key.Create(u);
        
        return u;
    }
    [SerializationConstructor] private Army(int id,
        ERef<Regime> regime, 
        ERefSetCallback<Unit> units,
        LineMission lineMission,
        HashSet<ArmyMission> otherOrders,
        RefSetCallback<CellRef> cells,
        Color color) 
        : base(id)
    {
        Regime = regime;
        Units = units;
        Units.AddIndexerCallbacks(this, 
            d => d.Military.UnitAux.UnitByGroup);
        LineMission = lineMission;
        OtherOrders = otherOrders;
        Color = color;
        Cells = cells;
        Cells.AddIndexerCallbacks(this,
            d => d.Military.UnitAux.ArmiesByOccupancy);
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
        return d.Context.ArmyHomeCells[this];
    }
    public HashSet<Cell> GetCells(Data d)
    {
        return Cells.Refs.Select(c => c.Get(d)).ToHashSet();
    }

    public void SetLineOrder(LineMission mission, ProcedureWriteKey key)
    {
        LineMission = mission;
    }
    public void AddOrder(ArmyMission groupMission, ProcedureWriteKey key)
    {
        OtherOrders.Add(groupMission);
    }

    public float GetPowerPoints(Data data)
    {
        return Units.Entities(data).Sum(u => u.GetPowerPoints(data));
    }

    public override void CleanUp(StrongWriteKey key)
    {
        if (Units.Count() > 0) throw new Exception();
    }

    public void SetCells(IEnumerable<int> cells, ProcedureWriteKey key)
    {
        Cells.Clear(key);
        foreach (var cell in cells)
        {
            Cells.Add(new CellRef(cell), key);
        }
    }

    public void DistributeResources(CombatCalculator combat, Data d)
    {
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
                Units.Entities(d).ToList(),
                e => edgeNeeds[e],
                u => Mathf.Max(u.GetPowerPoints(d), 1f)
            );
        for (var i = 0; i < assigns.Count; i++)
        {
            var (edge, unit, proportion) = assigns[i];
            if (edge is ArmyAttackEdge atk)
            {
                atk.Attackers.Add((unit, proportion));
            }
            else if (edge is ArmyDefendEdge def)
            {
                def.Defenders.Add((unit, proportion));
            }
            else throw new Exception();
        } 
    }

    public void RemoveIfOverrunOrDestroyed(CombatCalculator combat, 
        LogicWriteKey key)
    {
        if (Cells.Count() == 0)
        {
            GD.Print($"destroying army {Id}");
            combat.Graph.RemoveNode(this);
            var update = new DestroyArmyProcedure(this.MakeRef());
            key.SendMessage(update);
            return;
        }
    }

    public Vector2 GetHealth(Data d)
    {
        return Units.Entities(d).Select(u => u.GetHealth(d))
            .Sum();
    }
}
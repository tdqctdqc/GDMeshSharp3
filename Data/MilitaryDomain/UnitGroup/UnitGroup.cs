
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class UnitGroup : Entity
{
    public ERef<Regime> Regime { get; private set; }
    public ERefSet<Unit> Units { get; private set; }
    public UnitGroupOrder GroupOrder { get; private set; }
    public Color Color { get; private set; }
    public MoveType MoveType(Data d) => Units.Items(d)
        .FirstOrDefault()?.Template.Entity(d).MoveType.Model(d);
    public static UnitGroup Create(Regime r, IEnumerable<int> unitIds, ICreateWriteKey key)
    {
        var id = key.Data.IdDispenser.TakeId();
        var units = ERefSet<Unit>.Construct(nameof(Units), id, unitIds.ToHashSet(), key.Data);
        var u = new UnitGroup(id, r.MakeRef(), units,
            new DoNothingUnitGroupOrder(),
            ColorsExt.GetRandomColor());
        key.Create(u);
        return u;
    }
    [SerializationConstructor] private UnitGroup(int id,
        ERef<Regime> regime, 
        ERefSet<Unit> units,
        UnitGroupOrder groupOrder,
        Color color) 
        : base(id)
    {
        Regime = regime;
        Units = units;
        GroupOrder = groupOrder;
        Color = color;
    }

    public static void ChangeUnitGroup(Unit u, 
        UnitGroup oldG, UnitGroup newG,
        ProcedureWriteKey key)
    {
        oldG?.Units.Remove(u, key);
        newG?.Units.Add(u, key);
        key.Data.Military.UnitAux.UnitChangedGroup.Invoke(u, newG, oldG);
    }

    public PolyCell GetCell(Data d)
    {
        var unit = Units.Items(d).First();
        return unit.Position.GetCell(d);
        // var moveType = unit.Template.Entity(d)
        //     .MoveType.Model(d);
        // var pos = unit.Position;
        // var alliance = Regime.Entity(d).GetAlliance(d);
        // var found = d.Military.WaypointGrid.TryGetClosest(pos.Pos,
        //     out var wp, 
        //     w => MoveType(d).TerrainPassable(w.Tri.Tri(d), d));
        // if (found == false) throw new Exception();
        // return wp;
    }

    public Vector2 GetPosition(Data d)
    {
        return Units.Items(d).First().Position.GetCell(d).GetCenter();
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
}
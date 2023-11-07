
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class UnitGroup : Entity
{
    public EntityRef<Regime> Regime { get; private set; }
    public EntRefCol<Unit> Units { get; private set; }
    public UnitOrder Order { get; private set; }
    public static UnitGroup Create(Regime r, IEnumerable<int> unitIds, ICreateWriteKey key)
    {
        var id = key.Data.IdDispenser.TakeId();
        var units = EntRefCol<Unit>.Construct(nameof(Units), id, unitIds.ToHashSet(), key.Data);
        var u = new UnitGroup(id, r.MakeRef(), units,
            new DoNothingUnitOrder());
        key.Create(u);
        return u;
    }
    [SerializationConstructor] private UnitGroup(int id,
        EntityRef<Regime> regime, 
        EntRefCol<Unit> units,
        UnitOrder order) 
        : base(id)
    {
        Regime = regime;
        Units = units;
        Order = order;
    }

    public static void ChangeUnitGroup(Unit u, 
        UnitGroup oldG, UnitGroup newG,
        ProcedureWriteKey key)
    {
        oldG?.Units.Remove(u, key);
        newG?.Units.Add(u, key);
        key.Data.Military.UnitAux.UnitChangedGroup.Invoke(u, newG, oldG);
    }

    public Waypoint GetWaypoint(Data d)
    {
        var p = d.Planet.GetAveragePosition(Units.Items(d).Select(u => u.Position));
        var wp = d.Planet.NavAux.WaypointGrid.GetElementAtPoint(p);
        return wp;
    }

    public Vector2 GetPosition(Data d)
    {
        return d.Planet.GetAveragePosition(Units.Items(d).Select(u => u.Position));
    }
    public void SetOrder(UnitOrder order, ProcedureWriteKey key)
    {
        Order = order;
    }

    public float GetPowerPoints(Data data)
    {
        return Units.Items(data).Sum(u => u.GetPowerPoints(data));
    }
}
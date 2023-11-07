using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using MessagePack;

public class StartConstructionProcedure : Procedure
{
    public EntityRef<Regime> OrderingRegime { get; private set; }
    public Construction Construction { get; private set; }

    public static StartConstructionProcedure Construct(ModelRef<BuildingModel> building, 
        PolyTriPosition pos, 
        int waypoint,
        EntityRef<Regime> orderingRegime, Data data)
    {
        var c = new Construction(building, pos, building.Model(data).NumTicksToBuild, waypoint);
        return new StartConstructionProcedure(c, orderingRegime);
    }
    [SerializationConstructor] private StartConstructionProcedure(Construction construction, 
        EntityRef<Regime> orderingRegime)
    {
        OrderingRegime = orderingRegime;
        Construction = construction;
    }

    public override bool Valid(Data data)
    {
        var poly = Construction.Pos.Poly(data);
        var regime = OrderingRegime.Entity(data);
        var noOngoing = data.Infrastructure.CurrentConstruction.ByPoly.ContainsKey(poly.Id) == false;
        if (noOngoing == false)
        {
            return false;
        }
        var polyHasRegime = poly.OwnerRegime.Fulfilled();
        if (polyHasRegime == false)
        {
            return false;
        }

        var itemCosts = Construction.Model.Model(data)
            .Makeable.ItemCosts.GetEnumerableModel(data);
        if (itemCosts.Any(kvp => regime.Items.Get(kvp.Key) < kvp.Value))
        {
            return false;
        }
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        Construction.Pos.Poly(key.Data).PolyBuildingSlots
            .RemoveSlot(Construction.Model.Model(key.Data).BuildingType, Construction.Pos);
        var regime = Construction.Pos.Poly(key.Data).OwnerRegime.Entity(key.Data);

        var itemCosts = Construction.Model.Model(key.Data)
            .Makeable.ItemCosts.GetEnumerableModel(key.Data);
        foreach (var kvp in itemCosts)
        {
            regime.Items.Remove(kvp.Key, kvp.Value);
            regime.History.ItemHistory.GetLatest(kvp.Key).Consumed += kvp.Value;
        }
        key.Data.Infrastructure.CurrentConstruction.StartConstruction(Construction, key);
    }
}

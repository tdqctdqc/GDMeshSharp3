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

    public static StartConstructionProcedure Construct(ModelRef<BuildingModel> building, PolyTriPosition pos, 
        EntityRef<Regime> orderingRegime, Data data)
    {
        var c = new Construction(building, pos, building.Model(data).NumTicksToBuild);
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
        var polyHasRegime = poly.Regime.Fulfilled();
        if (polyHasRegime == false)
        {
            return false;
        }

        if (Construction.Model.Model(data).BuildCosts.Any(kvp => regime.Items[kvp.Key] < kvp.Value))
        {
            return false;
        }
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        Construction.Pos.Poly(key.Data).PolyBuildingSlots
            .RemoveSlot(Construction.Model.Model(key.Data).BuildingType, Construction.Pos);
        var regime = Construction.Pos.Poly(key.Data).Regime.Entity(key.Data);
        foreach (var kvp in Construction.Model.Model(key.Data).BuildCosts)
        {
            regime.Items.Remove(kvp.Key, kvp.Value);
            regime.History.ItemHistory.Latest(kvp.Key).Consumed += kvp.Value;
        }
        key.Data.Infrastructure.CurrentConstruction.StartConstruction(Construction, key);
    }
}

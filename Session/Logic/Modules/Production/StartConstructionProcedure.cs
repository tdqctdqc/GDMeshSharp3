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
        EntityRef<Regime> orderingRegime)
    {
        var c = new Construction(building, pos, building.Model().NumTicksToBuild);
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
        var regime = OrderingRegime.Entity();
        var noOngoing = data.Society.CurrentConstruction.ByPoly.ContainsKey(poly.Id) == false;
        if (noOngoing == false)
        {
            return false;
        }
        var polyHasRegime = poly.Regime.Fulfilled();
        if (polyHasRegime == false)
        {
            return false;
        }

        if (Construction.Model.Model().BuildCosts.Any(kvp => regime.Items[kvp.Key] < kvp.Value))
        {
            return false;
        }
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        Construction.Pos.Poly(key.Data).PolyBuildingSlots
            .RemoveSlot(Construction.Model.Model().BuildingType, Construction.Pos);
        var regime = Construction.Pos.Poly(key.Data).Regime.Entity();
        foreach (var kvp in Construction.Model.Model().BuildCosts)
        {
            regime.Items.Remove(kvp.Key, kvp.Value);
        }
        key.Data.Society.CurrentConstruction.StartConstruction(Construction, key);
    }
}

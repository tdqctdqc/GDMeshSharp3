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
        int polyCellId, 
        EntityRef<Regime> orderingRegime, Data data)
    {
        var c = new Construction(building, polyCellId, building.Model(data).NumTicksToBuild);
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
        var cell = PlanetDomainExt.GetPolyCell(Construction.PolyCellId, data);

        var poly = ((LandCell)cell).Polygon.Entity(data);
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
        var cell = PlanetDomainExt.GetPolyCell(Construction.PolyCellId, key.Data);
        var poly = ((LandCell)cell).Polygon.Entity(key.Data);
        poly.PolyBuildingSlots
            .RemoveSlot(Construction.Model.Model(key.Data).BuildingType, Construction.PolyCellId);
        var regime = poly.OwnerRegime.Entity(key.Data);

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

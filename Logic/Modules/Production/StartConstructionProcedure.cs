using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using MessagePack;

public class StartConstructionProcedure : Procedure
{
    public ERef<Regime> OrderingRegime { get; private set; }
    public Construction Construction { get; private set; }

    public static StartConstructionProcedure Construct(ModelRef<BuildingModel> building, 
        int polyCellId, 
        ERef<Regime> orderingRegime, Data data)
    {
        var c = new Construction(building, polyCellId, building.Model(data).NumTicksToBuild);
        return new StartConstructionProcedure(c, orderingRegime);
    }
    [SerializationConstructor] private StartConstructionProcedure(Construction construction, 
        ERef<Regime> orderingRegime)
    {
        OrderingRegime = orderingRegime;
        Construction = construction;
    }

    public override bool Valid(Data data, out string error)
    {
        var cell = PlanetDomainExt.GetPolyCell(Construction.PolyCellId, data);
        if (cell is LandCell == false)
        {
            error = "Cell is not land";
            return false;
        }
        var model = Construction.Model.Model(data);
        var poly = ((LandCell)cell)
            .Polygon.Entity(data);
        if (model.CanBuildInPoly(poly, data) == false)
        {
            error = "Cannot build in poly";
            return false;
        }

        var regime = OrderingRegime.Entity(data);
        if (cell.Controller.RefId != OrderingRegime.RefId)
        {
            error = "Controller is not same as ordering regime";
            return false;
        }
        var ongoing = data.Infrastructure
            .CurrentConstruction.ByPolyCell
            .ContainsKey(cell.Id);
        if (ongoing)
        {
            error = "Ongoing construction in cell";
            return false;
        }
        var buildingInCell = data.Infrastructure
            .BuildingAux.ByCell.ContainsKey(cell);
        if (buildingInCell)
        {
            error = "Building in cell";
            return false;
        }

        var itemCosts = Construction.Model.Model(data)
            .Makeable.ItemCosts.GetEnumerableModel(data);
        if (itemCosts.Any(kvp => regime.Items.Get(kvp.Key) < kvp.Value))
        {
            error = "Cannot meet item cost";
            return false;
        }

        error = "";
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
        key.Data.Infrastructure
            .CurrentConstruction.StartConstruction(Construction, key);
    }
}

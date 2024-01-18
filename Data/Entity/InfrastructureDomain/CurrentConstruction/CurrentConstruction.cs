using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class CurrentConstruction : Entity
{
    public Dictionary<int, Construction> ByPolyCell { get; private set; }
    public Dictionary<int, List<Construction>> ByPoly { get; private set; }
    public static CurrentConstruction Create(GenWriteKey key)
    {
        var cc = new CurrentConstruction(key.Data.IdDispenser.TakeId(), 
            new Dictionary<int, Construction>(),
            new Dictionary<int, List<Construction>>());
        key.Create(cc);
        return cc;
    }
    [SerializationConstructor] private CurrentConstruction(int id, 
        Dictionary<int, Construction> byPolyCell,
        Dictionary<int, List<Construction>> byPoly) : base(id)
    {
        ByPolyCell = byPolyCell;
        ByPoly = byPoly;
    }

    public List<Construction> GetPolyConstructions(MapPolygon poly)
    {
        return ByPoly.ContainsKey(poly.Id)
            ? ByPoly[poly.Id]
            : null;
    }
    public void StartConstruction(Construction construction, 
        ProcedureWriteKey key)
    {
        var polyCell = PlanetDomainExt.GetPolyCell(construction.PolyCellId, key.Data);
        var poly = ((LandCell)polyCell).Polygon.Entity(key.Data);
        ByPoly.AddOrUpdate(poly.Id, construction);
        if (ByPolyCell.ContainsKey(construction.PolyCellId))
        {
            throw new Exception($"trying to build {construction.Model.Model(key.Data).Name}" +
                                $"but already constructing {ByPolyCell[construction.PolyCellId].Model.Model(key.Data).Name} in tri");
        }
        ByPolyCell.Add(construction.PolyCellId, construction);
        key.Data.Infrastructure.ConstructionAux.StartedConstruction.Invoke(construction);
    }
    public void FinishConstruction(MapPolygon poly, PolyCell cell, ProcedureWriteKey key)
    {
        var construction = ByPolyCell[cell.Id];
        key.Data.Infrastructure.ConstructionAux.EndedConstruction.Invoke(construction);
        ByPoly[poly.Id].RemoveAll(c => c.PolyCellId == cell.Id);
        if (ByPoly[poly.Id].Count == 0) ByPoly.Remove(poly.Id);
        ByPolyCell.Remove(cell.Id);
    }
}

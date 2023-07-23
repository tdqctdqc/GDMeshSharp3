using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class CurrentConstruction : Entity
{
    public Dictionary<PolyTriPosition, Construction> ByTri { get; private set; }
    public Dictionary<int, List<Construction>> ByPoly { get; private set; }
    public static CurrentConstruction Create(GenWriteKey key)
    {
        var cc = new CurrentConstruction(-1, 
            new Dictionary<PolyTriPosition, Construction>(),
            new Dictionary<int, List<Construction>>());
        key.Create(cc);
        return cc;
    }
    [SerializationConstructor] private CurrentConstruction(int id, 
        Dictionary<PolyTriPosition, Construction> byTri,
        Dictionary<int, List<Construction>> byPoly) : base(id)
    {
        ByTri = byTri;
        ByPoly = byPoly;
    }

    public List<Construction> GetPolyConstructions(MapPolygon poly)
    {
        return ByPoly.ContainsKey(poly.Id)
            ? ByPoly[poly.Id]
            : null;
    }
    public void StartConstruction(Construction construction, ProcedureWriteKey key)
    {
        var poly = construction.Pos.Poly(key.Data);
        ByPoly.AddOrUpdate(poly.Id, construction);
        if (ByTri.ContainsKey(construction.Pos))
        {
            throw new Exception($"trying to build {construction.Model.Model(key.Data).Name}" +
                                $"but already constructing {ByTri[construction.Pos].Model.Model(key.Data).Name} in tri");
        }
        ByTri.Add(construction.Pos, construction);
        key.Data.Notices.StartedConstruction.Invoke(construction);
    }
    public void FinishConstruction(MapPolygon poly, PolyTriPosition pos, ProcedureWriteKey key)
    {
        var construction = ByTri[pos];
        key.Data.Notices.EndedConstruction.Invoke(construction);
        ByPoly[poly.Id].RemoveAll(c => c.Pos.Equals(pos));
        if (ByPoly[poly.Id].Count == 0) ByPoly.Remove(poly.Id);
        ByTri.Remove(pos);
    }
}

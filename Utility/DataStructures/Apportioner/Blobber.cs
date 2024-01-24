
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class Blobber
{
    public static IEnumerable<TBlob> Blob<TBlob, TElement>(
        IEnumerable<TElement> allValidElements,
        IEnumerable<TBlob> blobs,
        Func<TBlob, IEnumerable<TElement>> getEls,
        Func<TElement, IEnumerable<TElement>> getNeighbors,
        Action<TBlob, IEnumerable<TBlob>> distributeInto,
        Func<HashSet<TElement>, TBlob> makeBlob)
            where TElement : class
    {
        var validHash = allValidElements.ToHashSet();
        var newUnions = UnionFind.Find(
            validHash,
            (e, f) => true,
            e => getNeighbors(e).Where(validHash.Contains))
            .Select(l => l.ToHashSet());
        var newBlobs = newUnions.ToDictionary(
            u => u,
            u => makeBlob(u));
        foreach (var blob in blobs)
        {
            var overlapping = newBlobs
                .Where(kvp => getEls(blob).Any(e => kvp.Key.Contains(e)))
                .Select(kvp => kvp.Value);
            distributeInto(blob, overlapping);
        }

        return newBlobs.Values;
    }

    public static IEnumerable<TheaterAssignment>
        Blob(this IEnumerable<TheaterAssignment> theaters, Regime regime, Data d)
    {
        var cells = d.Planet.PolygonAux.PolyCells.Cells.Values
            .OfType<LandCell>().Where(c => c.Controller.RefId == regime.Id);
        return Blob(
            cells, 
            theaters,
            t => t.GetCells(d).OfType<LandCell>(),
            wp => wp.GetNeighbors(d).OfType<LandCell>(),
            divideInto,
            makeBlob
        );

        TheaterAssignment makeBlob(IEnumerable<LandCell> wps)
        {
            return new TheaterAssignment(d.IdDispenser.TakeId(),
                regime.MakeRef(), new HashSet<ForceAssignment>(),
                wps.Select(wp => wp.Id).ToHashSet(), new HashSet<int>());
        }
        void divideInto(TheaterAssignment dissolve, IEnumerable<TheaterAssignment> intos)
        {

            foreach (var assgn in dissolve.Assignments)
            {
                var wp = assgn.GetCharacteristicCell(d);
                var theater = intos.First(t => t.HeldCellIds.Contains(wp.Id));
                theater.Assignments.Add(assgn);
                theater.GroupIds.AddRange(assgn.GroupIds);
                dissolve.GroupIds.RemoveWhere(assgn.GroupIds.Contains);
            }

            foreach (var dissolveGroupId in dissolve.GroupIds)
            {
                var group = d.Get<UnitGroup>(dissolveGroupId);
                var theater = intos.FirstOrDefault(f => f.HeldCellIds.Contains(group.GetCell(d).Id));
                if (theater is TheaterAssignment == false)
                {
                    theater = intos
                        .MinBy(f =>
                            f.GetCharacteristicCell(d).GetCenter()
                                .GetOffsetTo(group.GetCell(d).GetCenter(), d));
                }

                theater.GroupIds.Add(group.Id);
            }
        }
    }
    public static IEnumerable<FrontAssignment>
        Blob(this IEnumerable<FrontAssignment> fronts, 
            TheaterAssignment theater,
            LogicWriteKey key)
    {
        var d = key.Data;
        var regime = theater.Regime.Entity(d);
        var alliance = regime.GetAlliance(d);

        var cells = theater.GetCells(d)
            .Where(c => c.GetNeighbors(d).Any(n => n.RivalControlled(alliance, d)));
        return Blob(
            cells, fronts,
            t => t.GetCells(d),
            wp => wp.GetNeighbors(d),
            divideInto,
            makeBlob
        );

        FrontAssignment makeBlob(IEnumerable<PolyCell> wps)
        {
            return new FrontAssignment(d.IdDispenser.TakeId(),
                regime.MakeRef(), 
                wps.Select(wp => wp.Id).ToHashSet(), 
                new HashSet<int>(),
                new HashSet<int>(), new HashSet<ForceAssignment>(),
                ColorsExt.GetRandomColor());
        }
        void divideInto(FrontAssignment dissolve, IEnumerable<FrontAssignment> intos)
        {
            foreach (var assgn in dissolve.Assignments)
            {
                var wp = assgn.GetCharacteristicCell(d);
                var front = intos.First(t => t.HeldCellIds.Contains(wp.Id));
                front.Assignments.Add(assgn);
                front.GroupIds.AddRange(assgn.GroupIds);
                dissolve.GroupIds.RemoveWhere(assgn.GroupIds.Contains);
            }

            foreach (var dissolveGroupId in dissolve.GroupIds)
            {
                var group = d.Get<UnitGroup>(dissolveGroupId);
                var front = intos.FirstOrDefault(f => f.HeldCellIds.Contains(group.GetCell(d).Id));
                if (front is FrontAssignment == false)
                {
                    front = intos
                        .MinBy(f =>
                            f.GetCharacteristicCell(d).GetCenter()
                                .GetOffsetTo(group.GetCell(d).GetCenter(), d));
                }

                front.GroupIds.Add(group.Id);
            }
        }
    }
}
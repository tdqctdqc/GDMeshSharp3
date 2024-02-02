
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

    public static IEnumerable<Theater>
        Blob(this IEnumerable<Theater> theaters, 
            DeploymentAi ai,
            Regime regime, LogicWriteKey key)
    {
        var d = key.Data;
        var cells = d.Planet.PolygonAux.PolyCells.Cells.Values
            .OfType<LandCell>().Where(c => c.Controller.RefId == regime.Id);
        return Blob(
            cells, 
            theaters,
            t => t.GetCells(d).OfType<LandCell>(),
            wp => wp.GetNeighbors(d).OfType<LandCell>(),
            (t, ts) => t.DissolveInto(ai, ts, key),
            makeBlob
        );

        Theater makeBlob(IEnumerable<LandCell> wps)
        {
            var ai = key.Data.HostLogicData.RegimeAis[regime]
                .Military.Deployment;
            var t = Theater.Construct(ai, regime, wps, key);
            t.SetParent(ai, ai.Root, key);
            ai.AddNode(t);
            return t;
        }
    }
    public static IEnumerable<Front>
        Blob(this IEnumerable<Front> fronts, 
            DeploymentAi ai,
            Theater theater,
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
            (f,fs) => f.DissolveInto(ai, fs, key),
            makeBlob
        );

        Front makeBlob(IEnumerable<PolyCell> wps)
        {
            var f = Front.Construct(ai, regime, wps, key);
            ai.AddNode(f);
            f.SetParent(ai, theater, key);
            return f;
        }
    }
}
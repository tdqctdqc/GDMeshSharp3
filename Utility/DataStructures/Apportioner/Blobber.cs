
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

    
}

using System;
using System.Collections.Generic;
using System.Linq;

public static class Blobber
{
    public static IEnumerable<TBlob> Blob<TBlob, TElement>(
        HashSet<TElement> allValidElements,
        IEnumerable<TBlob> blobs,
        Func<TBlob, IEnumerable<TElement>> getEls,
        Func<TElement, IEnumerable<TElement>> getNeighbors,
        Func<TElement, bool> validElement,
        Action<TBlob, IEnumerable<TBlob>> divideInto,
        Action<(TBlob dissolve, TBlob into)> mergeInto,
        Func<HashSet<TElement>, TBlob> makeBlob)
            where TElement : class
    {
        var claimed = new HashSet<TElement>();
        var unions = new Dictionary<HashSet<TElement>, TBlob>();
        
        foreach (var blob in blobs)
        {
            var valids = getEls(blob)
                .Where(validElement).ToHashSet();
            if (valids.Count == 0)
            {
                continue;
            }

            var thisUnions = new HashSet<HashSet<TElement>>();
            while (valids.Count > 0)
            {
                var start = valids.First();
                var flood = FloodFill<TElement>.GetFloodFill(
                    start, validElement, getNeighbors);
                valids.RemoveWhere(flood.Contains);
                thisUnions.Add(flood);
            }

            var newBlobs = thisUnions
                .Select(makeBlob);
            divideInto(blob, newBlobs);
            foreach (var newBlob in newBlobs)
            {
                register(newBlob, getEls(newBlob).ToHashSet());
            }
        }

        var unclaimed = allValidElements
            .Except(claimed).ToHashSet();

        if (unclaimed.Count() > 0)
        {
            var unclaimedUnions = UnionFind.Find(unclaimed,
                (t, r) => true,
                getNeighbors)
                .Select(l => l.ToHashSet());
            foreach (var unclaimedUnion in unclaimedUnions)
            {
                var blob = makeBlob(unclaimedUnion);
                unions.Add(unclaimedUnion, blob);
            }
        }
        return unions.Values;
        void register(TBlob blob, HashSet<TElement> els)
        {
            if (els.FirstOrDefault(claimed.Contains) is TElement claimedEl)
            {
                var union = unions
                    .First(kvp => kvp.Key.Contains(claimedEl))
                    .Key;
                mergeInto((blob, unions[union]));
            }
            else
            {
                unions.Add(els, blob);
                claimed.AddRange(els);
            }
        }
    }
}
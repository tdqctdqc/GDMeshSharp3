
using System;
using System.Collections.Generic;
using System.Linq;

public static class Blobber
{
    public static IEnumerable<TBlob> Blob<TBlob, TElement>(
        IEnumerable<TElement> allValidElements,
        IEnumerable<TBlob> blobs,
        Func<TBlob, IEnumerable<TElement>> getEls,
        Func<TElement, IEnumerable<TElement>> getNeighbors,
        Func<TElement, bool> validElement,
        Action<TBlob, IEnumerable<TBlob>> distributeInto,
        Action<(TBlob dissolve, TBlob into)> mergeInto,
        Func<HashSet<TElement>, TBlob> makeBlob)
            where TElement : class
    {
        var claimed = new Dictionary<TElement, TBlob>();
        var unions = new Dictionary<HashSet<TElement>, TBlob>();
        var distribute = new Dictionary<TBlob, HashSet<TBlob>>();
        foreach (var blob in blobs)
        {
            var valids = getEls(blob)
                .Where(validElement).ToHashSet();
            if (valids.Count == 0)
            {
                continue;
            }
            

            var newUnions = new HashSet<HashSet<TElement>>();
            while (valids.Count > 0)
            {
                var start = valids.First();
                if (claimed.ContainsKey(start))
                {
                    var master = claimed[start];
                    var kvp = unions
                        .First(kvp => kvp.Key.Contains(start));
                    distribute.GetOrAdd(blob, t => new HashSet<TBlob>())
                        .Add(master);
                    foreach (var element in kvp.Key)
                    {
                        valids.Remove(element);
                    }
                    continue;
                }
                var flood = FloodFill<TElement>.GetFloodFill(
                    start, validElement, getNeighbors);
                foreach (var element in flood)
                {
                    valids.Remove(element);
                }
                newUnions.Add(flood);
            }
            foreach (var newUnion in newUnions)
            {
                var newBlob = makeBlob(newUnion);
                distribute.GetOrAdd(blob, v => new HashSet<TBlob>())
                    .Add(newBlob);
                foreach (var element in newUnion)
                {
                    claimed.Add(element, newBlob);
                }
            }
        }

        var unclaimed = allValidElements
            .Except(claimed.Keys).ToArray();

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

        
        foreach (var kvp in distribute)
        {
            distributeInto(kvp.Key, kvp.Value);
        }
        return unions.Values;
    }

    public static IEnumerable<TheaterAssignment>
        Blob(this IEnumerable<TheaterAssignment> theaters, Regime regime, Data d)
    {
        var wps = d.Military.TacticalWaypoints.Waypoints.Values
            .Where(wp => wp.GetOccupyingRegime(d) == regime);
        return Blob(
            wps, theaters,
            t => t.GetWaypoints(d),
            wp => wp.GetNeighbors(d),
            wp => wp.GetOccupyingRegime(d) == regime,
            divideInto,
            mergeInto,
            makeBlob
        );

        TheaterAssignment makeBlob(IEnumerable<Waypoint> wps)
        {
            return new TheaterAssignment(d.IdDispenser.TakeId(),
                regime.MakeRef(), new HashSet<ForceAssignment>(),
                wps.Select(wp => wp.Id).ToHashSet(), new HashSet<int>());
        }

        void mergeInto((TheaterAssignment dissolve, TheaterAssignment into) v)
        {
            v.into.Assignments.AddRange(v.dissolve.Assignments);
        }
        void divideInto(TheaterAssignment dissolve, IEnumerable<TheaterAssignment> intos)
        {
            foreach (var assgn in dissolve.Assignments)
            {
                var wp = assgn.GetCharacteristicWaypoint(d);
                var theater = intos.First(t => t.TacWaypointIds.Contains(wp.Id));
                theater.Assignments.Add(assgn);
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
        var wps = theater.GetWaypoints(d);

        return Blob(
            wps, fronts,
            t => t.GetWaypoints(d),
            wp => wp.GetNeighbors(d),
            wp => wp.GetOccupyingRegime(d) == regime,
            divideInto,
            mergeInto,
            makeBlob
        );

        FrontAssignment makeBlob(IEnumerable<Waypoint> wps)
        {
            return new FrontAssignment(d.IdDispenser.TakeId(),
                regime.MakeRef(), wps.Select(wp => wp.Id).ToHashSet(), 
                new HashSet<int>(),
                new HashSet<int>(), new HashSet<ForceAssignment>());
        }

        void mergeInto((FrontAssignment dissolve, FrontAssignment into) v)
        {
            v.into.MergeInto(v.dissolve, key);
        }
        void divideInto(FrontAssignment dissolve, IEnumerable<FrontAssignment> intos)
        {
            foreach (var assgn in dissolve.Assignments)
            {
                var wp = assgn.GetCharacteristicWaypoint(d);
                var front = intos.First(t => t.HeldWaypointIds.Contains(wp.Id));
                front.Assignments.Add(assgn);
                front.GroupIds.AddRange(assgn.GroupIds);
            }
        }
    }
    
    
    
    
    
    // public static IEnumerable<FrontSegmentAssignment>
    //     Blob(this IEnumerable<FrontSegmentAssignment> segs, 
    //         FrontAssignment theater,
    //         Data d)
    // {
    //     var regime = theater.Regime.Entity(d);
    //     var wps = theater.GetWaypoints(d);
    //
    //     return Blob(
    //         wps, segs,
    //         t => t.GetWaypoints(d),
    //         wp => wp.GetNeighbors(d),
    //         wp => wp.GetOccupyingRegime(d) == regime,
    //         divideInto,
    //         mergeInto,
    //         makeBlob
    //     );
    //
    //     FrontSegmentAssignment makeBlob(IEnumerable<Waypoint> wps)
    //     {
    //         return new FrontSegmentAssignment(d.IdDispenser.TakeId(),
    //             regime.MakeRef(), wps.Select(wp => wp.Id).ToHashSet(), 
    //             new HashSet<int>(),
    //             new HashSet<int>(), new HashSet<ForceAssignment>());
    //     }
    //
    //     void mergeInto((FrontSegmentAssignment dissolve, FrontSegmentAssignment into) v)
    //     {
    //         v.into.GroupIds.AddRange(v.dissolve.GroupIds);
    //     }
    //     void divideInto(FrontSegmentAssignment dissolve, IEnumerable<FrontSegmentAssignment> intos)
    //     {
    //         
    //         foreach (var assgn in dissolve.Assignments)
    //         {
    //             var wp = assgn.GetCharacteristicWaypoint(d);
    //             var front = intos.First(t => t.HeldWaypointIds.Contains(wp.Id));
    //             front.Assignments.Add(assgn);
    //         }
    //     }
    // }
}
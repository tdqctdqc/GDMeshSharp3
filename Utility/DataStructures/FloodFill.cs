
using System;
using System.Collections.Generic;
using System.Linq;

public static class FloodFill<T>
{
    public static HashSet<T> FloodFillForIter(T seed,
        int iter,
        Func<T, IEnumerable<T>> getNeighbors,
        Func<T, bool> valid)
    {
        var list = new List<T>{ seed };
        var res = new HashSet<T> { seed };
        var lastAddition = 1;
        for (var i = 0; i < iter; i++)
        {
            var thisAddition = 0;
            var count = list.Count;
            for (int j = count - lastAddition; j < count; j++)
            {
                var t = list[j];
                foreach (var n in getNeighbors(t))
                {
                    if (res.Contains(n) == false && valid(n))
                    {
                        list.Add(n);
                        res.Add(n);
                        thisAddition++;
                    }
                }
            }

            if (thisAddition == 0) return res;
            lastAddition = thisAddition;
        }

        return res;
    }
    
    public static Dictionary<T, List<T>> FloodFillMultiple
        (IEnumerable<T> seeds, Func<T, IEnumerable<T>> getNs,
            Func<T, T, float> getHeuristic,
            HashSet<T> free)
    {
        var seedsQueue = new PriorityQueue<T, int>();
        foreach (var seed in seeds)
        {
            seedsQueue.Enqueue(seed, 1);
        }
        var res = seeds.ToDictionary(s => s, 
            s => new List<T>{s});

        while (seedsQueue.Count > 0)
        {
            var seed = seedsQueue.Dequeue();
            var set = res[seed];
            var freeNs = set.SelectMany(s => getNs(s))
                .Where(free.Contains)
                .OrderBy(t => getHeuristic(seed, t));
            if (freeNs.Any() == false)
            {
                continue;
            }

            var take = freeNs.First();
            free.Remove(take);
            set.Add(take);
            seedsQueue.Enqueue(seed, set.Count);
        }

        if (free.Count > 0) throw new Exception();
        return res;
    }
    public static HashSet<T> GetFloodFill(T start, 
        Func<T, bool> valid,
        Func<T, IEnumerable<T>> getNeighbors)
    {
        var res = new HashSet<T>{start};
        var queue = new Queue<T>();
        queue.Enqueue(start);
        while (queue.TryDequeue(out var curr))
        {
            var neighbors = getNeighbors(curr);
            foreach (var neighbor in neighbors)
            {
                if (res.Contains(neighbor)) continue;
                if (valid(neighbor) == false) continue;
                queue.Enqueue(neighbor);
                res.Add(neighbor);
            }
        }
        
        return res;
    }

    public static HashSet<T> FloodTilFirst(T start, 
        Func<T, bool> validNeighbor,
        Func<T, IEnumerable<T>> getNeighbors,
        Func<T, bool> validResult,
        int maxIter = 1_000)
    {
        var res = new HashSet<T>{start};
        var queue = new Queue<T>();
        queue.Enqueue(start);
        int iter = 0;
        while (queue.TryDequeue(out var curr))
        {
            iter++;
            if (iter == maxIter) break;
            var neighbors = getNeighbors(curr);
            foreach (var neighbor in neighbors)
            {
                if (validResult(neighbor))
                {
                    return res;
                }
                if (res.Contains(neighbor)) continue;
                if (validNeighbor(neighbor) == false) continue;
                queue.Enqueue(neighbor);
                res.Add(neighbor);
            }
        }

        return res;
    }

    public static HashSet<T> FloodFillTilAllFoundMultipleStarts
    (IEnumerable<T> starts, 
        Func<T, bool> validNeighbor,
        Func<T, IEnumerable<T>> getNeighbors,
        IEnumerable<T> need,
        int maxIter = 1_000)
    {
        var res = starts.ToHashSet();
        var unencountered = need.ToHashSet();
        var queue = new Queue<T>();
        foreach (var r in res)
        {
            queue.Enqueue(r);
        }
        int iter = 0;
        while (queue.TryDequeue(out var curr))
        {
            iter++;
            if (iter == maxIter) break;
            var neighbors = getNeighbors(curr);
            foreach (var neighbor in neighbors)
            {
                if (unencountered.Contains(neighbor))
                {
                    unencountered.Remove(neighbor);
                }
                if (res.Contains(neighbor)) continue;
                if (validNeighbor(neighbor) == false) continue;
                queue.Enqueue(neighbor);
                res.Add(neighbor);
                if (unencountered.Count == 0) return res;
            }
        }

        return res;
    }
}
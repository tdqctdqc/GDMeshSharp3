
using System;
using System.Collections.Generic;
using System.Linq;

public static class FloodFill<T>
{
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

    public static void FloodTilFirst(T start, 
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
                    return;
                }
                if (res.Contains(neighbor)) continue;
                if (validNeighbor(neighbor) == false) continue;
                queue.Enqueue(neighbor);
                res.Add(neighbor);
            }
        }
    }
}
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableExt
{
    private static RandomNumberGenerator _rand = new RandomNumberGenerator();
    
    public static IEnumerable<T> Yield<T>(this T item)
    {
        if (item == null) yield break;
        yield return item;
    }
    public static List<T> GetBetween<T>(this IList<T> list, T from, T to)
    {
        var start = list.IndexOf(from);
        if (start == -1) throw new Exception();
        var res = new List<T>();
        for (var i = 1; i < list.Count; i++)
        {
            var val = list.Modulo(start + i);
            if (val.Equals(from))
            {
                throw new Exception();
            }
            if (val.Equals(to))
            {
                break;
            }
            res.Add(val);
        }

        return res;
    }
    public static T GetRandomElement<T>(this IEnumerable<T> enumerable)
    {
        var index = _rand.RandiRange(0, enumerable.Count() - 1);
        return enumerable.ElementAt(index);
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
    {
        return new HashSet<T>(enumerable);
    }
    public static List<T> GetDistinctRandomElements<T>(this IEnumerable<T> enumerable, int n)
    {
        var choices = new List<T>(enumerable);
        var indices = new HashSet<int>();
        var count = choices.Count;
        int iter = 0;
        while (indices.Count < n)
        {
            indices.Add(Game.I.Random.RandiRange(iter, count - 1 - iter));
            iter++;
        }

        return indices.Select(i => choices[i]).ToList();
    }
    
    public static float Product(this IEnumerable<float> floats)
    {
        var res = 1f;
        foreach (var f in floats)
        {
            res *= f;
        }

        return res;
    }



    public static void DoForGridAround(this Func<int, int, bool> action, int x, int y, 
        bool skipCenter = true)
    {
        bool cont = true;

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (skipCenter && i == 0 && j == 0) continue;
                cont = action(i, j);
                if (cont == false)
                {
                    break;
                }
            }
            if (cont == false)
            {
                break;
            }
        }
    }

    
    public static T Modulo<T>(this IList<T> list, int i)
    {
        while (i < 0) i += list.Count;
        return list[i % list.Count];
    }

    public static void AddRange<T>(this ICollection<T> hash, IEnumerable<T> en)
    {
        foreach (var t in en)
        {
            hash.Add(t);
        }
    }


    public static Dictionary<T, int> GetCounts<T>(this IEnumerable<T> elements)
    {
        var res = new Dictionary<T, int>();
        foreach (var element in elements)
        {
            res.AddOrSum(element, 1);
        }
        return res;
    }
    
    public static void DoForRuns<T>(this List<T> list,
        Func<T, bool> valid,
        Action<List<T>> handleRun)
    {
        var goodStartIndex = -1;
        for (var i = 0; i < list.Count; i++)
        {
            var val = list[i];
            if (valid(val) == false)
            {
                handle(goodStartIndex, i - 1);
                goodStartIndex = -1;
            }
            else
            {
                if (goodStartIndex == -1)
                {
                    goodStartIndex = i;
                }
            }

            if (i == list.Count - 1)
            {
                handle(goodStartIndex, i);
            }
        }

        void handle(int from, int to)
        {
            if (from == -1) return;
            handleRun(list.GetRange(from, to - from + 1));
        }
    }
    public static void DoForRunsCircular<T>(this List<T> list,
        Func<T, bool> valid,
        Action<List<T>> handleRun)
    {
        var firstGood = list.FindIndex(t => valid(t));
        if (firstGood == -1) return;
        var goodStartIndex = -1;
        
        
        for (var i = 0; i < list.Count; i++)
        {
            var val = list.Modulo(firstGood + i);
            if (valid(val) == false)
            {
                handle(goodStartIndex, i + firstGood - 1);
                goodStartIndex = -1;
            }
            else
            {
                if (goodStartIndex == -1)
                {
                    goodStartIndex = firstGood + i;
                }
            }

            if (i == list.Count - 1)
            {
                handle(goodStartIndex, i + firstGood);
            }
        }

        void handle(int from, int to)
        {
            if (from == -1) return;
            if (from <= to)
            {
                handleRun(list.GetRange(from, to - from + 1));
            }
            else
            {
                var run1 = list.GetRange(from, list.Count - 1);
                var run2 = list.GetRange(0, to + 1);
                run2.AddRange(run1);
                handleRun(run2);
            }
        }
    }
    
    public static void DoForRunIndices<T>(this List<T> list,
        Func<T, bool> valid,
        Action<Vector2I> handleRun)
    {
        var goodStartIndex = -1;
        for (var i = 0; i < list.Count; i++)
        {
            var val = list[i];
            if (valid(val) == false)
            {
                handle(goodStartIndex, i - 1);
                goodStartIndex = -1;
            }
            else
            {
                if (goodStartIndex == -1)
                {
                    goodStartIndex = i;
                }
            }

            if (i == list.Count - 1)
            {
                handle(goodStartIndex, i);
            }
        }

        void handle(int from, int to)
        {
            if (from == -1) return;
            handleRun(new Vector2I(from, to));
        }
    }

    public static Vector2I GetProportionIndicesOfList<T>(this List<T> list, 
        float startRatio, float endRatio)
    {
        if (list.Count == 0) return -Vector2I.One;
        if (endRatio > 1f) throw new Exception();
        var startIndex = startRatio * list.Count;
        startIndex = Mathf.FloorToInt(startIndex);
        var endIndex = endRatio * list.Count - 1;
        endIndex = Mathf.FloorToInt(endIndex);
        if (endIndex < startIndex) return -Vector2I.One;
        if (startIndex < 0 || startIndex >= list.Count
                           || endIndex < 0 || endIndex >= list.Count)
        {
            throw new Exception($"Bad split, count {list.Count}" +
                                $"start {startIndex} end {endIndex}");
        }

        return new Vector2I((int)startIndex, (int)endIndex);
    }
    
    
    
    public static List<T> GetFrontLeftToRight<T>(this T t, 
        Func<T, bool> hasLeft, 
        Func<T, T> getLeft,
        Func<T, bool> hasRight, 
        Func<T, T> getRight,
        Func<T, bool> valid)
    {
        var res = new List<T>();
        var furthestLeft = t;
        while (hasLeft(furthestLeft))
        {
            var nextLeft = getLeft(furthestLeft);
            if (nextLeft.Equals(t) || valid(nextLeft) == false) break;
            furthestLeft = nextLeft;
        }
        res.Add(furthestLeft);
        var curr = furthestLeft;
        while (hasRight(curr))
        {
            var nextRight = getRight(curr);
            if (nextRight.Equals(furthestLeft) || valid(nextRight) == false) break;
            res.Add(nextRight);
            curr = nextRight;
        }
        
        return res;
    }
}

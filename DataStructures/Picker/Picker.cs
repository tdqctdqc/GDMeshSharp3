
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Picker
{
    public static List<TSeed>[] PickSeeds<TSeed>(IEnumerable<TSeed> available, int[] seedNums)
    {
        var taken = new HashSet<TSeed>();
        var result = new List<TSeed>[seedNums.Length];
        for (var i = 0; i < seedNums.Length; i++)
        {
            var seeds = available.Except(taken).GetDistinctRandomElements(seedNums[i]);
            seeds.ForEach(s => taken.Add(s));
            result[i] = seeds;
        }
        return result;
    }
    public static HashSet<TPicked> PickInTurn<TPicker, TPicked>(IEnumerable<TPicked> notTakenSource, 
        IEnumerable<TPicker> openPickersSource,
        Func<TPicker, HashSet<TPicked>> getAdjacent, Action<TPicker, TPicked> take)
    {
        Func<TPicker, HashSet<TPicked>, TPicked> choose = (picker, avail) =>
        {
            var adj = getAdjacent(picker);
            foreach (var el in adj)
            {
                if (avail.Contains(el))
                {
                    return el;
                }
            }
            return default;
        };
        return Pick(notTakenSource, openPickersSource, getAdjacent, choose, take);
    }
    public static HashSet<TPicked> PickInTurnHeuristic<TPicker, TPicked>(IEnumerable<TPicked> notTakenSource, 
        IEnumerable<TPicker> openPickersSource,
        Func<TPicker, HashSet<TPicked>> getAdjacent, Action<TPicker, TPicked> take,
        Func<TPicked, TPicker, float> heuristic)
    {
        Func<TPicker, HashSet<TPicked>, TPicked> choose = (picker, avail) =>
        {
            float takeHeur = Mathf.Inf;
            bool found = false;
            TPicked chosen = default;
            var adj = getAdjacent(picker);
            foreach (var el in adj)
            {
                if (avail.Contains(el) == false) continue;
                var heur = heuristic(el, picker);
                if (heur < takeHeur)
                {
                    found = true;
                    takeHeur = heur;
                    chosen = el;
                }
            }

            return (TPicked)chosen;
        };

        return Pick(notTakenSource, openPickersSource, getAdjacent, choose, take);
    }

    public static HashSet<TPicked> PickInTurnToLimit<TPicker, TPicked>(IEnumerable<TPicked> notTakenSource, 
        IEnumerable<TPicker> openPickersSource,
        Func<TPicker, HashSet<TPicked>> getAdjacent, Action<TPicker, TPicked> take,
        int numPickedToLeave)
    {
        Func<TPicker, HashSet<TPicked>, TPicked> choose = (picker, avail) =>
        {
            if (avail.Count == numPickedToLeave) return default;
            var adj = getAdjacent(picker);
            foreach (var el in adj)
            {
                if (avail.Contains(el))
                {
                    return el;
                }
            }
            return default;
        };
        return Pick(notTakenSource, openPickersSource, getAdjacent, choose, take);
    }
    public static HashSet<TPicked> PickInTurnToLimitHeuristic<TPicker, TPicked>(IEnumerable<TPicked> notTakenSource, 
        IEnumerable<TPicker> openPickersSource,
        Func<TPicker, HashSet<TPicked>> getAdjacent, Action<TPicker, TPicked> take,
        Func<TPicked, TPicker, float> heuristic,
        int numPickedToLeave) where TPicked : class
    {
        Func<TPicker, HashSet<TPicked>, TPicked> choose = (picker, avail) =>
        {
            if (avail.Count <= numPickedToLeave) return null;
            float takeHeur = Mathf.Inf;
            bool found = false;
            TPicked chosen = null;
            var adj = getAdjacent(picker);
            foreach (var el in adj)
            {
                if (avail.Contains(el) == false) continue;
                var heur = heuristic(el, picker);
                if (heur < takeHeur)
                {
                    found = true;
                    takeHeur = heur;
                    chosen = el;
                }
            }

            return chosen;
        };
        return Pick(notTakenSource, openPickersSource, 
            getAdjacent, choose, take);
    }
    
    private static HashSet<TPicked> Pick<TPicker, TPicked>(IEnumerable<TPicked> notTakenSource, IEnumerable<TPicker> openPickersSource,
        Func<TPicker, HashSet<TPicked>> getAdjacent, 
        Func<TPicker, HashSet<TPicked>, TPicked> choose,
        Action<TPicker, TPicked> take)
    {
        //todo not valid for structs?
        var notTaken = new HashSet<TPicked>(notTakenSource);
        var openPickers = new LinkedList<TPicker>(openPickersSource);
        while (openPickers.Count > 0)
        {
            var picker = openPickers.First;
            openPickers.RemoveFirst();

            var adj = getAdjacent(picker.Value);
            TPicked toTake = choose(picker.Value, notTaken);
            
            if (toTake == null)
            {
                continue;
            }

            openPickers.AddLast(picker);
            notTaken.Remove(toTake);
            take(picker.Value, toTake);
        }
        return notTaken;
    }
}

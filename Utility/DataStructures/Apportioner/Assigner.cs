
using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;

public class Assigner<TPicker, TPicked>
{
    public static void Assign(IEnumerable<TPicker> pickers,
        Func<TPicker, float> getPriority,
        HashSet<TPicked> toPick,
        Action<TPicker, TPicked> assign,
        Func<TPicker, TPicked, float> ranker)
    {
        if (pickers.Count() == 0) return;
        var priorityQueue = new SimplePriorityQueue<TPicker, float>();
        foreach (var picker in pickers)
        {
            priorityQueue.Enqueue(picker, -getPriority(picker));
        }
        while (toPick.Count > 0)
        {
            var picker = priorityQueue.First;
            var preferred = toPick.MaxBy(pick => ranker(picker, pick));
            assign(picker, preferred);
            toPick.Remove(preferred);
            priorityQueue.UpdatePriority(picker, -getPriority(picker));
        }
    }
}
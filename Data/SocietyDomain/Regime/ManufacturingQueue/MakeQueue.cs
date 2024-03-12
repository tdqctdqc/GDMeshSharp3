using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MakeQueue
{
    public Queue<MakeProject> Queue { get; private set; }
    public static MakeQueue Construct()
    {
        return new MakeQueue(new Queue<MakeProject>());
    }
    [SerializationConstructor] private MakeQueue(Queue<MakeProject> queue)
    {
        Queue = queue;
    }

    public void Manufacture(float ip, Regime r, ProcedureWriteKey key)
    {
        // if (Queue.Count == 0) return;
        //
        // while (ip > 0 && Queue.Count > 0)
        // {
        //     var curr = Queue.Peek();
        //     var actual = Mathf.Min(curr.Remaining(key.Data), ip);
        //     if (actual < 0f) throw new Exception();
        //     
        //     ip -= actual;
        //     curr.Work(r, key, actual);
        //     if (curr.IsComplete(key.Data))
        //     {
        //         Queue.Dequeue();
        //     }
        //     else
        //     {
        //         break;
        //     }
        // }
    }

    public void Cancel(MakeProject project, Regime r, ProcedureWriteKey key)
    {
        // //todo make into an abstract func
        // var unfinishedRatio = 1f - project.IpProgress 
        //     / project.IndustrialCost(key.Data);
        // foreach (var kvp in project.ItemCosts(key.Data))
        // {
        //     var amt = kvp.Value * unfinishedRatio;
        //     r.Store.Add(kvp.Key, amt);
        // }
        // Queue = new Queue<MakeProject>(Queue.Where(q => q != project));
    }
}
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ManufacturingQueue
{
    public Queue<ManufactureProject> Queue { get; private set; }
    public static ManufacturingQueue Construct()
    {
        return new ManufacturingQueue(new Queue<ManufactureProject>());
    }
    [SerializationConstructor] private ManufacturingQueue(Queue<ManufactureProject> queue)
    {
        Queue = queue;
    }

    public void Manufacture(float ip, Regime r, ProcedureWriteKey key)
    {
        if (Queue.Count == 0) return;

        while (ip > 0 && Queue.Count > 0)
        {
            var curr = Queue.Peek();
            var actual = Mathf.Min(curr.Remaining(key.Data), ip);
            if (actual < 0f) throw new Exception();
            
            ip -= actual;
            curr.Work(r, key, actual);
            if (curr.IsComplete(key.Data))
            {
                Queue.Dequeue();
            }
            else
            {
                break;
            }
        }
    }
}
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MakeQueue
{
    public List<MakeProject> Queue { get; private set; }
    public static MakeQueue Construct()
    {
        return new MakeQueue(new List<MakeProject>());
    }
    [SerializationConstructor] private MakeQueue(List<MakeProject> queue)
    {
        Queue = queue;
    }
}
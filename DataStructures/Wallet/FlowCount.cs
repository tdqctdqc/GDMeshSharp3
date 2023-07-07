using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class FlowCount : ModelCount<Flow>
{
    public static FlowCount Construct()
    {
        return new FlowCount(new Dictionary<int, float>());
    }
    [SerializationConstructor] public FlowCount(Dictionary<int, float> contents) 
        : base(contents)
    {
    }
}

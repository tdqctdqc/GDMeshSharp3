using System;
using System.Collections.Generic;
using System.Linq;

public class FlowCount : ModelCount<Flow>
{
    public static FlowCount Construct()
    {
        return new FlowCount(new Dictionary<int, float>());
    }
    protected FlowCount(Dictionary<int, float> contents) : base(contents)
    {
    }
}

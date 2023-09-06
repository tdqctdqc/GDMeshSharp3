using System;
using System.Collections.Generic;
using System.Linq;

public class RegimeFlows
{
    public FlowData this[Flow flow] => Flows[flow.Id];
    public Dictionary<int, FlowData> Flows { get; private set; }

    public RegimeFlows(Dictionary<int, FlowData> flows)
    {
        Flows = flows;
    }
    

    public void AddFlowIn(Flow flow, float flowIn)
    {
        if(Flows.ContainsKey(flow.Id) == false) Flows.Add(flow.Id, new FlowData(0f, 0f));
        this[flow].AddFlowIn(flowIn);
    }

    public void AddFlowOut(Flow flow, float flowOut)
    {
        if(Flows.ContainsKey(flow.Id) == false) Flows.Add(flow.Id, new FlowData(0f, 0f));
        this[flow].AddFlowOut(flowOut);
    }

    public FlowCount GetNetCount()
    {
        var count = FlowCount.Construct();
        foreach (var kvp in Flows)
        {
            count.Add(kvp.Key, kvp.Value.Net());
        }

        return count;
    }
}

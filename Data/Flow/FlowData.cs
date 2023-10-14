using System;
using System.Collections.Generic;
using System.Linq;

public class FlowData
{
    public float FlowIn { get; private set; }
    public float FlowOut { get; private set; }
    public float Net() => FlowIn - FlowOut;

    public FlowData(float flowIn, float flowOut)
    {
        FlowIn = flowIn;
        FlowOut = flowOut;
    }

    public void AddFlowIn(float flowIn)
    {
        FlowIn += flowIn;
    }

    public void AddFlowOut(float flowOut)
    {
        FlowOut += flowOut;
    }

}

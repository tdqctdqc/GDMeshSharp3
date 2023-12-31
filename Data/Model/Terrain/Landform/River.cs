using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class River : Landform
{
    public static readonly float WidthFloor = 5f, 
        WidthCeil = 40f,
        FlowFloor = 10f,
        FlowCeil = 200f;
    public River()
        : base("River", Mathf.Inf, 0f, Colors.DeepSkyBlue, true)
    {
    }

    public static float GetWidthFromFlow(float flow)
    {
        if (flow < FlowFloor) return 0f;
        flow = Mathf.Min(flow, FlowCeil);
        var ratio = (flow - FlowFloor) / (FlowCeil - FlowFloor);
        var width = ratio * (WidthCeil - WidthFloor) + WidthFloor;
        return width;
        
        // var logBase = Mathf.Pow(River.FlowCeil, 1f / (River.WidthCeil - River.WidthFloor));
        // return Mathf.Max(0f, (float)Math.Log(flow, logBase));
    }
}
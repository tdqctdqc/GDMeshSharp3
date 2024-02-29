using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class River : Landform
{
    public static readonly float WidthFloor = 2f, 
        WidthCeil = 40f,
        FlowFloor = 10f,
        FlowCeil = 200f;
    public River() : base(nameof(River))
    {
    }

    public static float GetWidthFromFlow(float flow)
    {
        if (flow < FlowFloor) return 0f;
        flow = Mathf.Min(flow, FlowCeil);
        var ratio = (flow - FlowFloor) / (FlowCeil - FlowFloor);
        if (ratio > 1f || ratio < 0f) throw new Exception();
        var width = ratio * (WidthCeil - WidthFloor) + WidthFloor;
        return width;
        
        // var logBase = Mathf.Pow(River.FlowCeil, 1f / (River.WidthCeil - River.WidthFloor));
        // return Mathf.Max(0f, (float)Math.Log(flow, logBase));
    }
}
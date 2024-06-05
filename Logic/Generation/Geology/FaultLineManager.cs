using System;
using Godot;
using System.Collections.Generic;

public class FaultLineManager
{
    public List<FaultLine> FaultLines { get; private set; }
    private Dictionary<Vector2I, FaultLine> _faults;

    public FaultLineManager()
    {
        FaultLines = new List<FaultLine>();
        _faults = new Dictionary<Vector2I, FaultLine>();
    }

    public void AddFault(FaultLine fault)
    {
        _faults.Add(fault.HighId.GetIdEdgeKey(fault.LowId), fault);
        FaultLines.Add(fault);
    }
}

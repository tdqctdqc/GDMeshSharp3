using System;
using Godot;
using System.Collections.Generic;

public class FaultLineManager
{
    public List<FaultLine> FaultLines { get; private set; }
    private Dictionary<Edge<GenPlate>, FaultLine> _faults;

    public FaultLineManager()
    {
        FaultLines = new List<FaultLine>();
        _faults = new Dictionary<Edge<GenPlate>, FaultLine>();
    }

    public void AddFault(FaultLine fault)
    {
        var edge = new Edge<GenPlate>(fault.HighId, fault.LowId, (p1, p2) => p1.Id < p2.Id);
        _faults.Add(edge, fault);
        FaultLines.Add(fault);
    }

    public bool TryGetFault(GenPlate p1, GenPlate p2, out FaultLine fault)
    {
        var edge = new Edge<GenPlate>(p1, p2, (p, q) => p.Id < q.Id);
        return _faults.TryGetValue(edge, out fault);
    }
}

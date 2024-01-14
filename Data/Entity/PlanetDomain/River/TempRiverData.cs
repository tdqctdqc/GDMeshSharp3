using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class TempRiverData
{
    public ConcurrentDictionary<EdgeEndKey, Vector2> HiPivots { get; private set; }
    public TempRiverData()
    {
        HiPivots = new ConcurrentDictionary<EdgeEndKey, Vector2>();
    }
}



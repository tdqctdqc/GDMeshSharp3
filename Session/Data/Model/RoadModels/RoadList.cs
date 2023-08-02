using System;
using System.Collections.Generic;
using System.Linq;

public class RoadList : ModelList<RoadModel>
{
    public PavedRoad PavedRoad { get; private set; } = new PavedRoad();
    public DirtRoad DirtRoad { get; private set; } = new DirtRoad();
    public Railroad Railroad { get; private set; } = new Railroad();
}

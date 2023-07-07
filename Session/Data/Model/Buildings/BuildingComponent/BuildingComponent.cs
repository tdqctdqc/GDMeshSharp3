using System;
using System.Collections.Generic;
using System.Linq;

public abstract class BuildingComponent
{
    public abstract void Work(ProduceConstructProcedure proc, MapPolygon poly, float staffingRatio, Data data);
}

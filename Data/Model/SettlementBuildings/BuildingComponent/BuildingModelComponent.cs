using System;
using System.Collections.Generic;
using System.Linq;

public abstract class BuildingModelComponent
{
    public abstract void Work(Cell cell, float staffingRatio, ProcedureWriteKey key);
}

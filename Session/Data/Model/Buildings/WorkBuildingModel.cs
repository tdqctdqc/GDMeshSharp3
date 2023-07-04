using System;
using System.Collections.Generic;
using System.Linq;

public abstract class WorkBuildingModel : BuildingModel
{
    public int Income { get; private set; }
    public abstract Dictionary<PeepJob, int> JobLaborReqs { get; }
    protected WorkBuildingModel(BuildingType buildingType, string name, int numTicksToBuild, int laborPerTickToBuild, int income)
        : base(buildingType, name, numTicksToBuild, laborPerTickToBuild)
    {
        Income = income;
    }
    public abstract void Work(ProduceConstructProcedure proc, MapPolygon poly, 
        float staffingRatio, Data data);

    public int TotalLaborReq()
    {
        return JobLaborReqs.Sum(kvp => kvp.Value);
    }
}

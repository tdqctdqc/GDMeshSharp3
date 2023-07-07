using System;
using System.Collections.Generic;
using System.Linq;

public class Workplace : BuildingComponent
{
    public Dictionary<PeepJob, int> JobLaborReqs { get; private set; }

    public Workplace(Dictionary<PeepJob, int> jobLaborReqs)
    {
        JobLaborReqs = jobLaborReqs;
    }

    public override void Work(ProduceConstructProcedure proc, MapPolygon poly, 
        float staffingRatio, Data data)
    {
        // proc.EmploymentReports
    }

    public int TotalLaborReq() => JobLaborReqs.Sum(kvp => kvp.Value);
}

using System;
using System.Collections.Generic;
using System.Linq;

public class Workplace : BuildingModelComponent
{
    public Dictionary<PeepJob, int> JobLaborReqs { get; private set; }

    public Workplace(Dictionary<PeepJob, int> jobLaborReqs)
    {
        JobLaborReqs = jobLaborReqs;
    }


    public int TotalLaborReq() => JobLaborReqs.Sum(kvp => kvp.Value);
    public override void Work(Cell cell, float staffingRatio, ProcedureWriteKey key)
    {
        
    }
}

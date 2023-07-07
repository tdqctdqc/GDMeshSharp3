using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyEmploymentScratch
{
    public Dictionary<PeepJob, int> ByJob { get; private set; }
    public int Desired { get; set; }
    public int Total { get; private set; }
    public int Available { get; private set; }
    public PolyEmploymentScratch(MapPolygon poly, Data data)
    {
        ByJob = new Dictionary<PeepJob, int>();
        var peep = poly.GetPeep(data);
        Init(poly, data);
    }
    
    public void Init(MapPolygon poly, Data data)
    {
        var peep = poly.GetPeep(data);
        if (peep == null) throw new Exception();
        Total = peep.Size;
        Desired = 0;
        Available = Total;
        ByJob.Clear();
    }
    public int HandleFoodProdJobs(PolyFoodProd foodProd, Data data)
    {
        var totalLaborNeeded = foodProd.BaseLabor(data);
        if (totalLaborNeeded == 0) return 0;
        var ratio = (float)Available / totalLaborNeeded;
        if (ratio > 1f) ratio = 1f;
        var job = PeepJobManager.Farmer;
        foreach (var kvp in foodProd.Nums)
        {
            var technique = (FoodProdTechnique)data.Models[kvp.Key];
            if (Available == 0) break;
            var numBuildings = kvp.Value;
            var desiredLabor = technique.BaseLabor * numBuildings;
            Desired += desiredLabor;
            var numLabor = Mathf.CeilToInt(ratio * desiredLabor);
            numLabor = Mathf.Min(Available, numLabor);
            Available -= numLabor;
            ByJob.AddOrSum(job, numLabor);
        }

        return Mathf.FloorToInt(ratio * foodProd.BaseProd(data));
    }
    public float HandleBuildingJobs(IEnumerable<Workplace> work, Data data)
    {
        var effectiveRatio = (float)Available / work.Sum(w => w.TotalLaborReq());
        effectiveRatio = Mathf.Clamp(effectiveRatio, 0f, 1f);
        foreach (var model in work)
        {
            if (Available == 0) break;
            foreach (var jobReq in model.JobLaborReqs)
            {
                if (Available == 0) break;
                var job = jobReq.Key;
                var size = jobReq.Value;
                Desired += size;
                var num = Mathf.CeilToInt(effectiveRatio * size);
                num = Mathf.Min(Available, num);
                Available -= num;
                ByJob.AddOrSum(job, num);
            }
        }

        return effectiveRatio;
    }
}

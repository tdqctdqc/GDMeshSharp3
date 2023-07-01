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
    public float HandleBuildingJobs(IEnumerable<WorkBuildingModel> work, Data data)
    {
        var totalLaborNeeded = work.Sum(wb => wb.TotalLaborReq());
        if (totalLaborNeeded == 0) return 1f;
        var ratio = (float)Available / totalLaborNeeded;
        if (ratio > 1f) ratio = 1f;
        foreach (var model in work)
        {
            if (Available == 0) break;
            foreach (var jobReq in model.JobLaborReqs)
            {
                if (Available == 0) break;
                var job = jobReq.Key;
                var size = jobReq.Value;
                Desired += size;
                var num = Mathf.CeilToInt(ratio * size);
                num = Mathf.Min(Available, num);
                Available -= num;
                ByJob.AddOrSum(job, num);
            }
        }

        return ratio;
    }

    public int HandleConstructionJobs(Data data, int regimeUnemployedLaborerTotal, int regimeConstructNeedTotal,
        int regimeConstructNeedRunningTotal)
    {
        if (regimeConstructNeedRunningTotal <= 0) return 0;
        var builderJob = PeepJobManager.Builder;
        var unemployed = Available;
        var contribution = 0;
        if (regimeConstructNeedTotal > regimeUnemployedLaborerTotal)
        {
            contribution = unemployed;
            Available -= contribution;
            Desired += contribution;
        }
        else
        {
            var shareOfTotalUnemployed = (float)unemployed / (float)regimeUnemployedLaborerTotal;
            var shareOfNeed = shareOfTotalUnemployed * regimeConstructNeedTotal;
            contribution = Mathf.CeilToInt(shareOfNeed);
            contribution = Mathf.Min(regimeConstructNeedRunningTotal, contribution);
            contribution = Mathf.Min(Available, contribution);
            Available -= contribution;
            Desired += Mathf.CeilToInt(shareOfTotalUnemployed * regimeConstructNeedTotal);
        }
        ByJob.AddOrSum(builderJob, contribution);

        return contribution;
    }
}

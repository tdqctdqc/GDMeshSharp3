using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class PeepEmploymentReport
{
    public IdCount<PeepJob> Counts { get; private set; }
    public static PeepEmploymentReport Construct()
    {
        var counts = IdCount<PeepJob>.Construct();
        return new PeepEmploymentReport(counts);
    }
    [SerializationConstructor] private PeepEmploymentReport(
        IdCount<PeepJob> counts)
    {
        Counts = counts;
    }

    public void Copy(PeepEmploymentReport toCopy, ProcedureKey key)
    {
        Counts.Clear();
        foreach (var (peepJob, value) in toCopy.Counts.GetEnumModel(key.Data))
        {
            Counts.Set(peepJob, value);
        }
    }

    public float NumUnemployed(Data data)
    {
        return Counts.Get(data.Models.PeepJobs.Unemployed);
    }
    public void Clear()
    {
        
    }
}

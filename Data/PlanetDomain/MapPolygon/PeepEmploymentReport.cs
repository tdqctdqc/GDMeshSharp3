using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class PeepEmploymentReport
{
    public Dictionary<int, int> Counts { get; private set; }
    public static PeepEmploymentReport Construct()
    {
        return new PeepEmploymentReport(new Dictionary<int, int>());
    }
    [SerializationConstructor] private PeepEmploymentReport(Dictionary<int, int> counts)
    {
        Counts = new Dictionary<int, int>();
    }

    public void Copy(PeepEmploymentReport toCopy, ProcedureWriteKey key)
    {
        Counts.Clear();
        Counts.AddRange(toCopy.Counts);
    }

    public int NumUnemployed(Data data)
    {
        if (Counts.ContainsKey(data.Models.PeepJobs.Unemployed.Id) == false) return 0;
        return Counts[data.Models.PeepJobs.Unemployed.Id];
    }
    public void Clear()
    {
        
    }
}

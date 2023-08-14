using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class PolyEmploymentReport
{
    public Dictionary<int, int> Counts { get; private set; }
    public static PolyEmploymentReport Construct()
    {
        return new PolyEmploymentReport(new Dictionary<int, int>());
    }
    [SerializationConstructor] private PolyEmploymentReport(Dictionary<int, int> counts)
    {
        Counts = new Dictionary<int, int>();
    }

    public void Copy(PolyEmploymentReport toCopy, ProcedureWriteKey key)
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

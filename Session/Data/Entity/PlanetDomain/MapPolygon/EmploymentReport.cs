using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class EmploymentReport
{
    public Dictionary<int, int> Counts { get; private set; }
    public static EmploymentReport Construct()
    {
        return new EmploymentReport(new Dictionary<int, int>());
    }
    [SerializationConstructor] private EmploymentReport(Dictionary<int, int> counts)
    {
        Counts = new Dictionary<int, int>();
    }

    public void Copy(EmploymentReport toCopy, ProcedureWriteKey key)
    {
        Counts.Clear();
        Counts.AddRange(toCopy.Counts);
    }

    public int NumUnemployed()
    {
        if (Counts.ContainsKey(PeepJobManager.Unemployed.Id) == false) return 0;
        return Counts[PeepJobManager.Unemployed.Id];
    }
    public void Clear()
    {
        
    }
}

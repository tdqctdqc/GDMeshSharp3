using System.Collections.Generic;

public interface ITechReqed
{
    HashSet<Technology> Prereqs { get; }
}
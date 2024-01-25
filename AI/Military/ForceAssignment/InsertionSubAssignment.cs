
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class InsertionSubAssignment
{
    public Dictionary<int, (int prevGroupId, int nextGroupId)> Insertions { get; private set; }

    public static InsertionSubAssignment Construct()
    {
        return new InsertionSubAssignment(new Dictionary<int, (int prevGroupId, int nextGroupId)>());
    }

    [SerializationConstructor] private InsertionSubAssignment(Dictionary<int, (int prevGroupId, int nextGroupId)> insertions)
    {
        Insertions = insertions;
    }

    public void Handle(FrontSegmentAssignment seg, LogicWriteKey key)
    {
        foreach (var kvp in Insertions.ToList())
        {
            
        }
    }
}
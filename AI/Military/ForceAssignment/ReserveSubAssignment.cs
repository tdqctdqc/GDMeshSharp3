
using System;
using System.Collections.Generic;
using MessagePack;

public class ReserveSubAssignment
{
    public HashSet<int> GroupIds { get; private set; }
    public int CellId { get; private set; }

    public static ReserveSubAssignment Construct()
    {
        return new ReserveSubAssignment(
            new HashSet<int>(),
            -1);
    }
    [SerializationConstructor] private ReserveSubAssignment(HashSet<int> groupIds, int cellId)
    {
        GroupIds = groupIds;
        CellId = cellId;
    }

    public void Validate(FrontSegmentAssignment seg, LogicWriteKey key)
    {
        
    }
    public void DistributeAmong(IEnumerable<FrontSegmentAssignment> segs, LogicWriteKey key)
    {
        throw new NotImplementedException();
    }
}
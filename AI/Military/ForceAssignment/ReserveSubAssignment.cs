
using System;
using System.Collections.Generic;
using System.Linq;
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
        foreach (var groupId in GroupIds)
        {
            var groupCell = key.Data.Get<UnitGroup>(groupId)
                .GetCell(key.Data);
            var close = segs
                .MinBy(s =>
                    s.GetCharacteristicCell(key.Data)
                        .GetCenter()
                        .GetOffsetTo(groupCell.GetCenter(), key.Data));
            close.GroupIds.Add(groupId);
            close.Reserve.GroupIds.Add(groupId);
        }
        GroupIds.Clear();
    }
}
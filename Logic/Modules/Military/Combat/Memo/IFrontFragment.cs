
using System.Collections.Generic;

public interface IFrontFragment
{
    FrontFace GetFirst();
    FrontFace GetLast();
    List<FrontFace> GetFaces();
    (List<UnitGroup> lineGroups, List<UnitGroup> reserveGroups,
        List<UnitGroup> insertGroups) GetGroups();
}
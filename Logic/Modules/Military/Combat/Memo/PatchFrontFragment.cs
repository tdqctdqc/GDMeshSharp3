
using System.Collections.Generic;

public class PatchFrontFragment : IFrontFragment
{
    public List<FrontFace> Faces { get; private set; }
    public PatchFrontFragment(FrontFace start, HashSet<FrontFace> open,
        Data d)
    {
        Faces = new List<FrontFace> { start };
        Faces = start
            .GetFrontLeftToRight(open.Contains, d);
        open.ExceptWith(Faces);
    }

    public FrontFace GetFirst()
    {
        return Faces[0];
    }

    public FrontFace GetLast()
    {
        return Faces[^1];
    }

    public List<FrontFace> GetFaces() => Faces;

    public (List<UnitGroup> lineGroups, List<UnitGroup> reserveGroups,
        List<UnitGroup> insertGroups) GetGroups() => (new List<UnitGroup>(), new List<UnitGroup>(), new List<UnitGroup>());
}
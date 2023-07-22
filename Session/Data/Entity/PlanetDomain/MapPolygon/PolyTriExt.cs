
using System.Collections.Generic;
using Godot;

public static class PolyTriExt
{
    public static float GetFertility(this PolyTri t)
    {
        return t.Landform.FertilityMod * t.Vegetation.FertilityMod;
    }
    public static bool HasBuilding(this PolyTri t, Data data)
    {
        return data.Infrastructure.BuildingAux.ByTri[t.GetPosition()] != null;
    }
    public static MapBuilding GetBuilding(this PolyTri t, Data data)
    {
        return data.Infrastructure.BuildingAux.ByTri[t.GetPosition()];
    }
}

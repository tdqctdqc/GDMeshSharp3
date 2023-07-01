
using MessagePack;

public struct PolyTriPosition
{
    public byte TriIndex { get; private set; }
    public int PolyId { get; private set; }
    public MapPolygon Poly(Data data) => (MapPolygon)data[PolyId];
    [SerializationConstructor] public PolyTriPosition(int polyId, byte triIndex)
    {
        PolyId = polyId;
        TriIndex = triIndex;
    }

    public PolyTri Tri(Data data)
    {
        if(TriIndex != -1) return Poly(data).Tris.Tris[TriIndex];
        return null;
    }
}

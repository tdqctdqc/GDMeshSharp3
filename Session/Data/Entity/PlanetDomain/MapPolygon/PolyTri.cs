
using System;
using Godot;
using MessagePack;

public class PolyTri : Triangle
{
    public int PolyId { get; private set; }
    public byte Index { get; private set; }
    public int NeighborStartIndex { get; private set; }
    public byte NeighborCount { get; private set; }
    public byte LfMarker { get; private set; }
    public byte VMarker { get; private set; }
    public Landform Landform => LandformManager.ByMarker[LfMarker];
    public Vegetation Vegetation => VegetationManager.ByMarker[VMarker];


    public static PolyTri Construct(int polyId, Vector2 a, Vector2 b, Vector2 c, Landform landform,
        Vegetation vegetation)
    {
        return new PolyTri(polyId, a, b, c, landform.Marker, vegetation.Marker,
            (byte) 255, -1, 0);
    }

    [SerializationConstructor]
    public PolyTri(int polyId, Vector2 a, Vector2 b, Vector2 c, byte lfMarker, byte vMarker, byte index,
        int neighborStartIndex, byte neighborCount)
        : base(a, b, c)
    {
        PolyId = polyId;
        Index = index;
        LfMarker = lfMarker;
        VMarker = vMarker;
        NeighborCount = neighborCount;
        NeighborStartIndex = neighborStartIndex;
    }

    public void ForEachNeighbor(MapPolygon poly, Action<PolyTri> func)
    {
        for (var i = 0; i < NeighborCount; i++)
        {
            var n = poly.Tris.TriNeighbors[i + NeighborStartIndex];
            var nTri = poly.Tris.Tris[n];
            func(nTri);
        }
    }

    public bool AnyNeighbor(MapPolygon poly, Func<PolyTri, bool> func)
    {
        for (var i = 0; i < NeighborCount; i++)
        {
            var n = poly.Tris.TriNeighbors[i + NeighborStartIndex];
            var nTri = poly.Tris.Tris[n];
            if (func(nTri)) return true;
        }

        return false;
    }

    public PolyTriPosition GetPosition()
    {
        return new PolyTriPosition(PolyId, Index);
    }

public void SetLandform(Landform lf, GenWriteKey key)
    {
        LfMarker = lf.Marker;
    }
    public void SetVegetation(Vegetation v, GenWriteKey key)
    {
        VMarker = v.Marker;
    }

    public void SetNeighborStart(int start, GenWriteKey key)
    {
        NeighborStartIndex = start;
    }
    public void SetNeighborCount(byte count, GenWriteKey key)
    {
        NeighborCount = count;
    }
    public void SetIndex(byte index, GenWriteKey key)
    {
        Index = index;
    }
    public PolyTri Transpose(Vector2 offset)
    {
        return new PolyTri(PolyId, A + offset, B + offset, C + offset, 
            LfMarker, VMarker, Index,
            NeighborStartIndex, NeighborCount);
    }
}

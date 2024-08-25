using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VoronoiSandbox;

public class GenAuxiliaryData 
{
    public Dictionary<MapPolygon, GenCell> PolyGenCells { get; private set; }
    public HashSet<GenCell> Cells { get; private set; }
    public HashSet<GenMass> Masses { get; private set; }
    public HashSet<GenPlate> Plates { get; private set; }
    public HashSet<GenContinent> Continents { get; private set; }
    public Dictionary<MapPolygon, List<PreCell>> PreCellPolys { get; private set; }
    public Dictionary<MapPolygon, Vector2[]> PolyRelBoundaries { get; private set; }
    public FaultLineManager FaultLines { get; private set; }
    private FastNoiseLite _altNoise;
    public GenAuxiliaryData(GenData data)
    {
        _altNoise = new FastNoiseLite();
        _altNoise.Frequency = 1f;
        _altNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        _altNoise.FractalOctaves = 3;
        _altNoise.FractalLacunarity = 2;
        _altNoise.FractalGain = .5f;
        Cells = new HashSet<GenCell>();
        PolyGenCells = new Dictionary<MapPolygon, GenCell>();
        Masses = new HashSet<GenMass>();
        Plates = new HashSet<GenPlate>();
        Continents = new HashSet<GenContinent>();
        FaultLines = new FaultLineManager();
        PreCellPolys = new Dictionary<MapPolygon, List<PreCell>>();
        PolyRelBoundaries = new Dictionary<MapPolygon, Vector2[]>();
    }

    public float GetAltPerlin(Vector2 p)
    {
        return _altNoise.GetNoise2D(p.X, p.Y);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Landform : TerrainAspect
{
    public float MinRoughness { get; private set; }
    public float FertilityMod { get; private set; }
    public float DarkenFactor { get; private set; }
    public bool IsWater { get; private set; }
    public bool IsLand => IsWater == false;
    public Landform(string name)
    {
        var res = GD.Load<LandformRes>($"Data/Model/Terrain/Landform/{name}.tres");
        IsWater = res.IsWater;
        DarkenFactor = res.DarkenFactor;
        FertilityMod = res.FertilityMod;
        Name = res.Name;
        MinRoughness = res.MinRoughness;
        Color = res.Color;
    }
}
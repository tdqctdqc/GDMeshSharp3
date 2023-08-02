using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Landform : TerrainAspect
{
    public override string Name { get; protected set; }
    public float MinRoughness { get; private set; }
    public float FertilityMod { get; private set; }
    public float DarkenFactor { get; private set; }
    public override Color Color { get; protected set; }
    public bool IsWater { get; private set; }
    public override int Id { get; protected set; }
    public bool IsLand => IsWater == false;
    public Landform(string name, float minRoughness, float fertilityMod, 
        Color color, bool isWater, 
        float darkenFactor = 0f)
    {
        IsWater = isWater;
        DarkenFactor = darkenFactor;
        FertilityMod = fertilityMod;
        Name = name;
        MinRoughness = minRoughness;
        Color = color;
    }
}
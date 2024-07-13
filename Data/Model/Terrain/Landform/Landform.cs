using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Landform : TerrainAspect
{
    public float MinRoughness { get; private set; }
    public float FertilityMod { get; private set; }
    public float DarkenFactor { get; private set; }
    public float FrontLengthMult { get; private set; }
    public float MovementCostMult { get; private set; }
    public float EvasionMult { get; private set; }
    public bool IsWater { get; private set; }
    public bool IsLand() => IsWater == false;
    public Landform()
    {
        // var res = GD.Load<LandformRes>($"Data/Model/Terrain/Landform/{name}.tres");
        // IsWater = res.IsWater;
        // DarkenFactor = res.DarkenFactor;
        // FertilityMod = res.FertilityMod;
        // FrontLengthMult = res.FrontLengthMult;
        // Name = res.Name;
        // MinRoughness = res.MinRoughness;
        // MovementCostMult = res.MovementCostMult;
        // EvasionMult = res.EvasionMult;
        // Color = res.Color;
    }
}
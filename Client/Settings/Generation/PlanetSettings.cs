using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class PlanetSettings : Settings
{
    public BoolSettingsOption RetryGen { get; private set; }
    public FloatSettingsOption MapWidth { get; private set; }
    public FloatSettingsOption MapHeight { get; private set; }
    public FloatSettingsOption Seed { get; private set; }
    public FloatSettingsOption PreferredMinPolyEdgeLength { get; private set; }
    public static PlanetSettings Construct()
    {
        return new PlanetSettings();
    }
    [SerializationConstructor] private PlanetSettings() 
        : base("Planet")
    {
        RetryGen = new BoolSettingsOption("Retry Gen", false);
        MapWidth = new FloatSettingsOption("Map Width", 8000f, 4000f, 32000f, 1000f, true);
        MapHeight = new FloatSettingsOption("Map Height", 4000f, 2000f, 16000f, 1000f, true);
        Seed = new FloatSettingsOption("Seed", 0f, 0f, 1000f, 1f, true);
        PreferredMinPolyEdgeLength = new FloatSettingsOption("Preferred Min Poly Edge Length", 50f, 10f, 100f, 1f, false);
        SettingsOptions.Add(RetryGen);
        SettingsOptions.Add(MapWidth);
        SettingsOptions.Add(MapHeight);
        SettingsOptions.Add(Seed);
        SettingsOptions.Add(PreferredMinPolyEdgeLength);
    }
}

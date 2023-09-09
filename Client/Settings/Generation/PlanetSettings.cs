using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class PlanetSettings : Settings
{
    public BoolSettingsOption RetryGen { get; private set; }
        // = new BoolSettingsOption("Retry Gen", false);
    public FloatSettingsOption MapWidth { get; private set; }
        // = new FloatSettingsOption("Map Width", 16000f, 4000f, 32000f, 1000f, true);
    public FloatSettingsOption MapHeight { get; private set; }
        // = new FloatSettingsOption("Map Height", 8000f, 2000f, 16000f, 1000f, true);
    public FloatSettingsOption Seed { get; private set; }
        // = new FloatSettingsOption("Seed", 0f, 0f, 1000f, 1f, true);
    public FloatSettingsOption PreferredMinPolyEdgeLength { get; private set; }
        // = new FloatSettingsOption("Preferred Min Poly Edge Length", 50f, 10f, 100f, 1f, false);
    public static PlanetSettings Construct()
    {
        return new PlanetSettings("Planet",
            new BoolSettingsOption("Retry Gen", false),
            new FloatSettingsOption("Map Width", 16000f, 4000f, 32000f, 1000f, true),
            new FloatSettingsOption("Map Height", 8000f, 2000f, 16000f, 1000f, true),
            new FloatSettingsOption("Seed", 0f, 0f, 1000f, 1f, true),
            new FloatSettingsOption("Preferred Min Poly Edge Length", 50f, 10f, 100f, 1f, false)
        );
    }
    [SerializationConstructor] private PlanetSettings(string name, BoolSettingsOption retryGen, 
        FloatSettingsOption mapWidth, FloatSettingsOption mapHeight, FloatSettingsOption seed,
        FloatSettingsOption preferredMinPolyEdgeLength) 
        : base(name)
    {
        RetryGen = retryGen;
        MapWidth = mapWidth;
        MapHeight = mapHeight;
        Seed = seed;
        PreferredMinPolyEdgeLength = preferredMinPolyEdgeLength;
    }
}

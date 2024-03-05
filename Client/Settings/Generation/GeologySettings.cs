using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class GeologySettings : Settings
{
    public FloatSettingsOption RoughnessScale { get; private set; }
    public FloatSettingsOption FaultLineAltitudeScale { get; private set; }
    public FloatSettingsOption LandRatio { get; private set; }
    public FloatSettingsOption NumContinents { get; private set; }
    public FloatSettingsOption NumSeas { get; private set; }
    public FloatSettingsOption SeaLevel { get; private set; }
    public FloatSettingsOption FaultLineRange { get; private set; }
    public FloatSettingsOption FrictionAltEffect { get; private set; }
    public FloatSettingsOption FrictionRoughnessEffect { get; private set; }
    public FloatSettingsOption RoughnessErosionMult { get; private set; }
    public static GeologySettings Construct()
    {
        return new GeologySettings();
    }
    [SerializationConstructor] private GeologySettings() 
        : base("Geology")
    {
        RoughnessScale = new FloatSettingsOption("Roughness Scale", 1f, 0f, 2f, .1f, false);
        FaultLineAltitudeScale = new FloatSettingsOption("Fault Altitude Scale", 1.5f, 0f, 2f, .1f, false);
        LandRatio = new FloatSettingsOption("Land Ratio", .3f, .05f, 1f, .05f, false);
        NumContinents = new FloatSettingsOption("Num Continents", 5, 2, 10, 1, true);
        NumSeas = new FloatSettingsOption("Num Seas", 10, 0, 20, 1, true);
        SeaLevel = new FloatSettingsOption("Sea Level", .5f, 0f, 1f, .05f, false);
        FaultLineRange = new FloatSettingsOption("Fault Line Range", 75f, 0f, 1000f, 1f, false);
        FrictionAltEffect = new FloatSettingsOption("Friction Alt Effect", .1f, 0f, 1f, .01f, false);
        FrictionRoughnessEffect = new FloatSettingsOption("Friction Roughness Effect", 1.5f, 0f, 2f, .1f, false);
        RoughnessErosionMult = new FloatSettingsOption("Roughness Erosion Multiplier", 1f, 0f, 2f, .1f, false);
        SettingsOptions.Add(RoughnessScale);
        SettingsOptions.Add(FaultLineAltitudeScale);
        SettingsOptions.Add(LandRatio);
        SettingsOptions.Add(NumContinents);
        SettingsOptions.Add(NumSeas);
        SettingsOptions.Add(SeaLevel);
        SettingsOptions.Add(FaultLineRange);
        SettingsOptions.Add(FrictionAltEffect);
        SettingsOptions.Add(FrictionRoughnessEffect);
        SettingsOptions.Add(RoughnessErosionMult);
    }
}

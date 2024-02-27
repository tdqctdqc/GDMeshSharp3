using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class GeologySettings : Settings
{
    public FloatSettingsOption RoughnessScale { get; private set; }
        // = new FloatSettingsOption("Roughness Scale", 1f, 0f, 2f, .1f, false);
    
    public FloatSettingsOption FaultLineAltitudeScale { get; private set; }
        // = new FloatSettingsOption("Fault Altitude Scale", 1f, 0f, 2f, .1f, false);

    public FloatSettingsOption LandRatio { get; private set; }
        // = new FloatSettingsOption("Land Ratio", .3f, .05f, 1f, .05f, false);
    public FloatSettingsOption NumContinents { get; private set; }
        // = new FloatSettingsOption("Num Continents", 5, 2, 10, 1, true);
    public FloatSettingsOption NumSeas { get; private set; }
    public FloatSettingsOption SeaLevel { get; private set; }
    public FloatSettingsOption FaultLineRange { get; private set; }
    public FloatSettingsOption FrictionAltEffect { get; private set; }
    public FloatSettingsOption FrictionRoughnessEffect { get; private set; }
    public FloatSettingsOption RoughnessErosionMult { get; private set; }
    public static GeologySettings Construct()
    {
        return new GeologySettings("Geology",
            new FloatSettingsOption("Roughness Scale", 1f, 0f, 2f, .1f, false),
            new FloatSettingsOption("Fault Altitude Scale", 1.5f, 0f, 2f, .1f, false),
            new FloatSettingsOption("Land Ratio", .3f, .05f, 1f, .05f, false),
            new FloatSettingsOption("Num Continents", 5, 2, 10, 1, true),
            new FloatSettingsOption("Num Seas", 10, 0, 20, 1, true),
            new FloatSettingsOption("Sea Level", .5f, 0f, 1f, .05f, false),
            new FloatSettingsOption("Fault Line Range", 75f, 0f, 1000f, 1f, false),
            new FloatSettingsOption("Friction Alt Effect", .1f, 0f, 1f, .01f, false),
            new FloatSettingsOption("Friction Roughness Effect", 1.5f, 0f, 2f, .1f, false),
            new FloatSettingsOption("Roughness Erosion Multiplier", 1f, 0f, 2f, .1f, false)
        );
    }
    [SerializationConstructor] private GeologySettings(string name, 
        FloatSettingsOption roughnessScale, FloatSettingsOption faultLineAltitudeScale, 
        FloatSettingsOption landRatio, FloatSettingsOption numContinents, 
        FloatSettingsOption numSeas, FloatSettingsOption seaLevel, 
        FloatSettingsOption faultLineRange, FloatSettingsOption frictionAltEffect, 
        FloatSettingsOption frictionRoughnessEffect, FloatSettingsOption roughnessErosionMult) 
        : base(name)
    {
        RoughnessScale = roughnessScale;
        FaultLineAltitudeScale = faultLineAltitudeScale;
        LandRatio = landRatio;
        NumContinents = numContinents;
        NumSeas = numSeas;
        SeaLevel = seaLevel;
        FaultLineRange = faultLineRange;
        FrictionAltEffect = frictionAltEffect;
        FrictionRoughnessEffect = frictionRoughnessEffect;
        RoughnessErosionMult = roughnessErosionMult;
    }
}

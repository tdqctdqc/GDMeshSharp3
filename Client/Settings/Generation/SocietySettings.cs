using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class SocietySettings : Settings
{
    public FloatSettingsOption DevelopmentScale { get; private set; }

    public static SocietySettings Construct()
    {
        return new SocietySettings("Society", 
            new FloatSettingsOption("Development Scale", .5f, .1f, 1f, .1f, false));
    }
    [SerializationConstructor] private SocietySettings(string name, 
        FloatSettingsOption developmentScale) 
        : base(name)
    {
        DevelopmentScale = developmentScale;
    }
}

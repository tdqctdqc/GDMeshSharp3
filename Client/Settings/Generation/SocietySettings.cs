using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class SocietySettings : Settings
{
    public FloatSettingsOption DevelopmentScale { get; private set; }
    public static SocietySettings Construct()
    {
        return new SocietySettings();
    }
    [SerializationConstructor] private SocietySettings() 
        : base("Society")
    {
        DevelopmentScale = new FloatSettingsOption("Development Scale", .5f, .1f, 1f, .1f, false);
        SettingsOptions.Add(DevelopmentScale);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

public class SocietySettings : Settings
{
    public FloatSettingsOption DevelopmentScale { get; private set; }
        = new FloatSettingsOption("Development Scale", .5f, .1f, 1f, .1f, false);
    public SocietySettings() : base("Society")
    {
    }
}

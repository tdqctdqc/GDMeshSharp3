using System;
using System.Collections.Generic;
using System.Linq;

public class ClientSettings : Settings
{
    public SettingsOption<float> LargeIconSize { get; private set; }

    public SettingsOption<float> MedIconSize { get; private set; }
    public SettingsOption<float> SmallIconSize { get; private set; }
    public static ClientSettings Load()
    {
        return new ClientSettings("Client");
    }


    private ClientSettings(string name)
        : base(name)
    {
        LargeIconSize = new FloatSettingsOption("Large Icon Size",
            100f, 50f, 200f, 1f, true);
        MedIconSize = new FloatSettingsOption("Medium Icon Size",
            50f, 20f, 100f, 1f, true);
        SmallIconSize = new FloatSettingsOption("Small Icon Size",
            25f, 10f, 50f, 1f, true);
        
    }
}

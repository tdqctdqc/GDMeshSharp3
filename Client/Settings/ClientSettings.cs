using System;
using System.Collections.Generic;
using System.Linq;

public class ClientSettings : Settings
{
    public SettingsOption<float> MedIconSize { get; private set; }
    public static ClientSettings Load()
    {
        return new ClientSettings("Client");
    }


    private ClientSettings(string name)
        : base(name)
    {
        MedIconSize = new FloatSettingsOption("Medium Icon Size",
            50f, 25f, 100f, 1f, true);
    }
}

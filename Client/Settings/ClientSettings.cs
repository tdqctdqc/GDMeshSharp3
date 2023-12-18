using System;
using System.Collections.Generic;
using System.Linq;

public class ClientSettings : Settings
{

    public static ClientSettings Load()
    {
        return new ClientSettings("Client",
            new EnumSettingsOption<MapHighlighter.Modes>("Poly Highlight Mode", MapHighlighter.Modes.Simple));
    }

    public EnumSettingsOption<MapHighlighter.Modes> PolyHighlightMode { get; private set; }

    private ClientSettings(string name,
        EnumSettingsOption<MapHighlighter.Modes> polyHighlightMode)
        : base(name)
    {
        PolyHighlightMode = polyHighlightMode;
    }
}

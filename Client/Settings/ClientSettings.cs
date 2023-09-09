using System;
using System.Collections.Generic;
using System.Linq;

public class ClientSettings : Settings
{

    public static ClientSettings Load()
    {
        return new ClientSettings("Client",
            new EnumSettingsOption<PolyHighlighter.Modes>("Poly Highlight Mode", PolyHighlighter.Modes.Simple));
    }

    public EnumSettingsOption<PolyHighlighter.Modes> PolyHighlightMode { get; private set; }

    private ClientSettings(string name,
        EnumSettingsOption<PolyHighlighter.Modes> polyHighlightMode)
        : base(name)
    {
        PolyHighlightMode = polyHighlightMode;
    }
}

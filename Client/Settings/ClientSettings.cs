using System;
using System.Collections.Generic;
using System.Linq;

public class ClientSettings : Settings
{
    private static ClientSettings _settings;
    public static ClientSettings Load()
    {
        if(_settings == null) _settings = new ClientSettings();
        return _settings;
    }

    private ClientSettings() : base("Client")
    {
    }
    
    public EnumSettingsOption<PolyHighlighter.Modes> PolyHighlightMode { get; private set; } 
        = new EnumSettingsOption<PolyHighlighter.Modes>("Poly Highlight Mode", PolyHighlighter.Modes.Simple);
}

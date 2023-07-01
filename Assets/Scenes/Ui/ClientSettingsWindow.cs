using System;
using System.Collections.Generic;
using System.Linq;

public partial class ClientSettingsWindow : SettingsWindow
{
    public static ClientSettingsWindow Get()
    {
        var w = new ClientSettingsWindow();
        w.Setup(Game.I.Client.Settings);
        return w;
    }
    private ClientSettingsWindow()
    {
    }
}

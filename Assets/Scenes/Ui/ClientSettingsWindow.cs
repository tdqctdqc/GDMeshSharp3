using System;
using System.Collections.Generic;
using System.Linq;

public partial class ClientSettingsWindow : SettingsWindow
{
    public static void Open(ClientSettings s)
    {
        var w = new ClientSettingsWindow();
        w.Setup(s);
        Game.I.Client.WindowHolder.OpenWindow(w);
    }
    private ClientSettingsWindow()
    {
    }
}

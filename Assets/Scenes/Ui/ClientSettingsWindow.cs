using System;
using System.Collections.Generic;
using System.Linq;

public partial class ClientSettingsWindow : SettingsWindow
{
    public static ClientSettingsWindow Get(ClientSettings s)
    {
        var w = new ClientSettingsWindow();
        w.Setup(s);
        return w;
    }
    private ClientSettingsWindow()
    {
    }
}

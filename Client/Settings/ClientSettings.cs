using System;
using System.Collections.Generic;
using System.Linq;

public class ClientSettings : Settings
{

    public static ClientSettings Load()
    {
        return new ClientSettings("Client");
    }


    private ClientSettings(string name)
        : base(name)
    {
    }
}

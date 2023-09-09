using System;
using System.Collections.Generic;
using System.Linq;

public class Settings : ISettings
{
    public string Name { get; private set; }

    public List<ISettingsOption> Options() =>
        this.GetPropertiesOfType<ISettingsOption>().ToList();

    protected Settings(string name)
    {
        Name = name;
    }
}

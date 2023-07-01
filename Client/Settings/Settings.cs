using System;
using System.Collections.Generic;
using System.Linq;

public class Settings : ISettings
{
    public string Name { get; private set; }
    public IReadOnlyList<ISettingsOption> Options => _options;
    protected List<ISettingsOption> _options;

    protected Settings(string name)
    {
        Name = name;
        _options = GetType().GetPropertiesOfType<ISettingsOption>(this);
    }
}

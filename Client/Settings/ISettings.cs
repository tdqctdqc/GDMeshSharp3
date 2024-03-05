using System;
using System.Collections.Generic;
using System.Linq;

public interface ISettings
{
    string Name { get; }
    List<ISettingsOption> SettingsOptions { get; }
}

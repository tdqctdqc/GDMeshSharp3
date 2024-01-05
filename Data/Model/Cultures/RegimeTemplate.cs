using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;

public class RegimeTemplate : IModel
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public string Adjective { get; private set; }
    public string PrimaryColor { get; private set; }
    public string SecondaryColor { get; private set; }
    public string FlagPath { get; private set; }
    public Culture Culture { get; private set; }
    public Icon Flag { get; private set; }

    public RegimeTemplate(Culture culture, string json)
    {
        Culture = culture;
        var d = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        Name = d[nameof(Name)];
        Adjective = d[nameof(Adjective)];
        PrimaryColor = d[nameof(PrimaryColor)];
        SecondaryColor = d[nameof(SecondaryColor)];
        FlagPath = d[nameof(FlagPath)];
        Flag = Icon.Create(FlagPath, new Vector2I(3, 2));
    }
}

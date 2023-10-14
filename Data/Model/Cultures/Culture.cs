using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;

public class Culture : IModel
{
    public string Name { get; }
    public int Id { get; private set; }
    public List<string> SettlementNames { get; private set; }
    public List<RegimeTemplate> RegimeTemplates { get; private set; }
    
    public Culture(string json)
    {
        var d = JsonSerializer.Deserialize<JsonObject>(json);
        Name = (string)d[nameof(Name)];
        RegimeTemplates = JsonSerializer
            .Deserialize<JsonArray>(d[nameof(RegimeTemplates)])
            .Select(s => new RegimeTemplate(this, s.ToString()))
            .ToList();
        SettlementNames = JsonSerializer
            .Deserialize<List<string>>(d[nameof(SettlementNames)]);
    }
}

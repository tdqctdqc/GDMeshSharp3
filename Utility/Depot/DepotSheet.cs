using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;


public class DepotSheet
{
    public Dictionary<string, JsonObject> Columns { get; private set; }
    public JsonObject Sheet { get; private set; }
    public Dictionary<string, JsonObject> Lines { get; private set; }
    public Dictionary<string, Guid> LineGuids { get; private set; }
    public Type Type { get; set; }
    public string Name { get; private set; }
    public DepotSheet(JsonObject sheet, DepotImporter importer)
    {
        Name = JsonSerializer.Deserialize<string>(sheet["name"]);
        var sheetGuid = JsonSerializer.Deserialize<Guid>(sheet["guid"]);
        importer.Sheets.Add(Name, this);
        importer.SheetsByGuid.Add(sheetGuid, this);
        Columns = sheet["columns"].AsArray()
            .Select(a => a.AsObject())
            .ToDictionary(
                v => JsonSerializer.Deserialize<string>(v["name"]),
                v => v);
        Lines = new Dictionary<string, JsonObject>();
        LineGuids = new Dictionary<string, Guid>();
        foreach (var n in sheet["lines"].AsArray())
        {
            var line = n.AsObject();
            var lineName = JsonSerializer.Deserialize<string>(line["Name"]);
            var lineGuid = JsonSerializer.Deserialize<Guid>(line["guid"]);
            importer.LinesByGuid.Add(lineGuid, line);
            importer.LinesByName.Add(lineName, line);
            Lines.Add(lineName, line);
            LineGuids.Add(lineName, lineGuid);
        }
    }

    public void MakeObjectsDefault<T>(Func<T> get, DepotImporter importer)
    {
        foreach (var (lineName, line) in Lines)
        {
            var t = get();
            var lineGuid = LineGuids[lineName];
            importer.LineObjects.Add(lineGuid, t);
            importer.LineObjectsByName.Add(lineName, t);
        }
    }

    public void MakeObjectsModels<T>(IReadOnlyDictionary<string, object> models, 
        DepotImporter importer)
        where T : IModel
    {
        foreach (var (name, value) in models)
        {
            if (value is T == false)
            {
                GD.Print($"{name} is not {typeof(T).Name}");
                throw new Exception();
            }
            var guid = LineGuids[name];
            importer.LineObjects[guid] = value;
            importer.LineObjectsByName[name] = value;
        }
    }
}
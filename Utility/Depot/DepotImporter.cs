
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;
using Array = System.Array;

public class DepotImporter
{
    public Dictionary<string, DepotSheet> Sheets { get; private set; }
    public Dictionary<Guid, DepotSheet> SheetsByGuid { get; private set; }
    public Dictionary<Guid, JsonObject> LinesByGuid { get; private set; }
    public Dictionary<string, JsonObject> LinesByName { get; private set; }
    public Dictionary<Guid, object> LineObjects { get; private set; }
    public Dictionary<string, object> LineObjectsByName { get; private set; }

    public DepotImporter(string path)
    {
        Sheets = new Dictionary<string, DepotSheet>();
        SheetsByGuid = new Dictionary<Guid, DepotSheet>();
        LinesByGuid = new Dictionary<Guid, JsonObject>();
        LinesByName = new Dictionary<string, JsonObject>();
        LineObjects = new Dictionary<Guid, object>();
        LineObjectsByName = new Dictionary<string, object>();
        var depotString = GodotFileExt.ReadFileAsString(path);
        GetSheets(depotString);
    }
    public void GetSheets(string json)
    {
        var top = JsonSerializer
            .Deserialize<JsonObject>(json);
        var sheets = top["sheets"].AsArray();
        foreach (var n in sheets)
        {
            var sheet = n.AsObject();
            UnpackSheetJsonObjects(sheet);
        }
        
    }
    public void UnpackSheetJsonObjects(JsonObject sheetObject)
    {
        var sheetName = JsonSerializer.Deserialize<string>(sheetObject["name"]);
        var sheet = new DepotSheet(sheetObject, this);
    }

    public void MakeSheetObjectsDefault<T>(Func<T> get)
    {
        var sheetName = typeof(T).Name;
        var sheet = Sheets[sheetName];
        sheet.MakeObjectsDefault<T>(get, this);
    }

    public void MakeSheetObjectsModels<T>(IModelManager<T> manager)
        where T : IModel
    {
        
    }
    public void FillProperties<T>(string lineName, T t)
    {
        var type = typeof(T);
        var setPropMethod = this.GetType().GetMethod(
            nameof(FillProperty));
        
        while (type is not null)
        {
            var sheetName = type.Name;
            if (Sheets.TryGetValue(type.Name, out var sheet))
            {
                var line = sheet.Lines[lineName];
                var properties = type.GetProperties();
                foreach (var propertyInfo in properties)
                {
                    setPropMethod.InvokeGeneric(t, 
                        propertyInfo.PropertyType.Yield().ToArray(),
                        new object[]{sheet, line, propertyInfo, t});
                }
            }

            type = type.BaseType;
        }
    }

    private void FillProperty<TProperty>(
        DepotSheet sheet,
        JsonObject line,
        PropertyInfo propertyInfo,
        object o)
    {
        var propertyType = propertyInfo.PropertyType;
        bool found = false;
        var propertyName = propertyInfo.Name;
        if (propertyName == nameof(Entity.Id)) return;
        if (propertyName == nameof(IIconed.Icon)) return;
        if (sheet.Columns.TryGetValue(propertyName, out var column))
        {
            var columnType = JsonSerializer.Deserialize<string>
                (column["typeStr"]);
            var columnValue = line[propertyName];
            object value = null;
            if (columnType == "float")
            {
                value = UnpackFloat(columnValue);
            }
            else if (columnType == "int")
            {
                value = UnpackInt(columnValue);
            }
            else if (columnType == "text")
            {
                value = UnpackString(columnValue);
            }
            else if (columnType == "list")
            {
                value = UnpackList<TProperty>(column, columnValue.AsArray());
            }
            else if (columnType == "lineReference")
            {
                value = UnpackLineReference<TProperty>(columnValue);
            }
            else
            {
                throw new Exception();
            }
            propertyInfo.SetValue(o, value);
        }
        else
        {
             throw new Exception();
        }
        
    }

    private static string UnpackString(JsonNode columnValue)
    {
        return JsonSerializer.Deserialize<string>(columnValue);
    }

    private static int UnpackInt(JsonNode columnValue)
    {
        return JsonSerializer.Deserialize<int>(columnValue);
    }

    private static float UnpackFloat(JsonNode columnValue)
    {
        return JsonSerializer.Deserialize<float>(columnValue);
    }

    private object UnpackLineReference<TProperty>(JsonNode columnValue)
    {
        var refLineGuid = UnpackGuid(columnValue);
        return (TProperty)LineObjects[refLineGuid];
    }

    private static Guid UnpackGuid(JsonNode columnValue)
    {
        return JsonSerializer.Deserialize<Guid>(columnValue);
    }

    private TProperty UnpackList<TProperty>(JsonObject column,
        JsonArray list)
    {
        var propertyType = typeof(TProperty);
        if (typeof(IdCount<>).IsAssignableFrom(propertyType))
        {
            var idCountType = propertyType.GetGenericArguments()[0];
            return (TProperty)this.GetType().GetMethod(nameof(UnpackIdCount))
                .InvokeGeneric(this, idCountType.Yield().ToArray(),
                    new object[] { column, list });
        }
        else if (typeof(HashSet<>).IsAssignableFrom(propertyType))
        {
            var entryType = propertyType.GetGenericArguments()[0];
            return (TProperty) this.GetType().GetMethod(nameof(UnpackHashSet))
                .InvokeGeneric(this, entryType.Yield().ToArray(),
                    new object[] { list, "Value" });
        }
        else if (typeof(Array).IsAssignableFrom(propertyType))
        {
            var entryType = propertyType.GetGenericArguments()[0];
            return (TProperty) this.GetType().GetMethod(nameof(UnpackArray))
                .InvokeGeneric(this, entryType.Yield().ToArray(),
                    new object[] { list, "Value" });
        }
        else
        {
            throw new Exception("no way to import " + propertyType.Name);
        }

        return default;
    }

    private IdCount<TValue> UnpackIdCount<TValue>(JsonObject column,
        JsonArray list)
        where TValue : IIdentifiable
    {
        var res = IdCount<TValue>.Construct();
        for (var i = 0; i < list.Count; i++)
        {
            var entryName = UnpackString(list[i]["Name"]);
            var entryOb = (TValue)LineObjectsByName[entryName];
            var entryValue = UnpackFloat(list[i]["Value"]);
            res.Add(entryOb, entryValue);
        }

        return res;
    }

    private TValue[] UnpackArray<TValue>(JsonArray list,
        string columnName)
    {
        return UnpackEnumerable<TValue>(list, columnName)
            .ToArray();
    }
    private HashSet<TValue> UnpackHashSet<TValue>(JsonArray list,
        string columnName)
    {
        return UnpackEnumerable<TValue>(list, columnName)
            .ToHashSet();
    }
    
    private IEnumerable<TValue> UnpackEnumerable<TValue>(
        JsonArray list,
        string columnName)
    {
        Func<JsonNode, TValue> get;
        if (typeof(float).IsAssignableFrom(typeof(TValue)))
        {
            get = a => (TValue)JsonSerializer.Deserialize(a, typeof(float));
        }
        else if (typeof(int).IsAssignableFrom(typeof(TValue)))
        {
            get = a => (TValue)JsonSerializer.Deserialize(a, typeof(int));
        }
        else if (typeof(string).IsAssignableFrom(typeof(TValue)))
        {
            get = a => (TValue)JsonSerializer.Deserialize(a, typeof(string));
        }
        else
        {
            get = a => (TValue)LineObjects[UnpackGuid(a)];
        }

        for (var i = 0; i < list.Count; i++)
        {
            yield return get(list[i][columnName]);
        }
    }
    
}
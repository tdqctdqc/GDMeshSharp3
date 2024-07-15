
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
            if (sheet.ContainsKey("hidden")
                && JsonSerializer.Deserialize<bool>(sheet["hidden"]) == true)
            {
                continue;
            }
            UnpackSheetJsonObjects(sheet);
        }
        
    }
    public void UnpackSheetJsonObjects(JsonObject sheetObject)
    {
        var sheetName = JsonSerializer.Deserialize<string>(sheetObject["name"]);
        GD.Print($"adding sheet {sheetName}");
        var sheet = new DepotSheet(sheetObject, this);
    }

    public void MakeSheetObjectsDefault<T>(Func<T> get)
    {
        var sheetName = typeof(T).Name;
        var sheet = Sheets[sheetName];
        sheet.Type = typeof(T);
        sheet.MakeObjectsDefault<T>(get, this);
    }

    public void MakeSheetObjectsModels<T>(
        IReadOnlyDictionary<string, object> models,
        Func<T> defaultConstructor)
        where T : IModel
    {
        var name = typeof(T).Name;
        if (Sheets.ContainsKey(name) == false)
        {
            GD.Print("no sheet " + name);
            var baseType = typeof(T).BaseType;
            if (baseType == null 
                || typeof(IModel).IsAssignableFrom(baseType) == false)
            {
                GD.Print($"couldn't resolve {typeof(T).Name}");
                return;
            }
            var mi = GetType().GetMethod(nameof(MakeSheetObjectsModels));
            mi.InvokeGeneric(this, new Type[] { baseType },
                new object[] { models, defaultConstructor });
            return;
        }

        var sheet = Sheets[name];
        sheet.Type = typeof(T);
        sheet.MakeObjectsModels<T>(models, 
            defaultConstructor, this);
    }

    public void FillAllProperties()
    {
        var mi = this.GetType()
            .GetMethod(nameof(FillSheetProperties), BindingFlags.NonPublic | BindingFlags.Instance);
        if (mi is null) throw new Exception();
        foreach (var sheet in Sheets.Values)
        {
            if (sheet.Type is null)
            {
                throw new Exception($"no type for {sheet.Name}");
            }
            mi.InvokeGeneric(this, 
                new Type[] { sheet.Type },
                new object[] { sheet });
        }
    }

    private void FillSheetProperties<T>(DepotSheet sheet)
    {
        foreach (var (name, line) in sheet.LinesByName)
        {
            FillLineProperties<T>(name, (T)LineObjectsByName[name]);
        }
    }
    private void FillLineProperties<T>(string lineName, T t)
    {
        var type = typeof(T);
        var fillProperty = this.GetType().GetMethod(
            nameof(FillProperty),
            BindingFlags.NonPublic | BindingFlags.Instance);
        while (type is not null)
        {
            var sheetName = type.Name;
            if (Sheets.TryGetValue(type.Name, out var sheet))
            {
                var line = sheet.LinesByName[lineName];
                var properties = type.GetProperties();
                foreach (var propertyInfo in properties)
                {
                    fillProperty.InvokeGeneric(this, 
                        propertyInfo.PropertyType.Yield().ToArray(),
                        new object[]{sheet, line, 
                            propertyInfo, t});
                }

                break;
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
            if (propertyType == typeof(float))
            {
                value = UnpackFloat(columnValue);
            }
            else if (propertyType == typeof(int))
            {
                value = UnpackInt(columnValue);
            }
            else if (propertyType == typeof(string))
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
            else if (propertyType == typeof(bool))
            {
                value = UnpackBool(columnValue);
            }
            else if (propertyType == typeof(Color))
            {
                var colorString = UnpackString(columnValue);
                value = new Color(colorString);
            }
            else
            {
                GD.Print($"couldnt unpack column type {columnType}");
                throw new Exception();
            }

            try
            {
                propertyInfo.SetValue(o, value);
            }
            catch (Exception e)
            {
                GD.Print($"couldn't set {propertyName} for {sheet.Type.Name}");
                throw;
            }
        }
        else
        {
            GD.Print($"couldn't find column {propertyName} for {sheet.Type.Name}");
            throw new Exception();
        }
        
    }

    private static string UnpackString(JsonNode columnValue)
    {
        return JsonSerializer.Deserialize<string>(columnValue);
    }
    private static bool UnpackBool(JsonNode columnValue)
    {
        return JsonSerializer.Deserialize<bool>(columnValue);
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
        JsonArray value)
    {
        var propertyType = typeof(TProperty);
        if (propertyType.IsGenericType 
            && propertyType.GetGenericTypeDefinition() 
                == typeof(IdCount<>))
        {
            var idCountType = propertyType.GetGenericArguments()[0];
            var mi = this.GetType().GetMethod(nameof(UnpackIdCount),
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (TProperty)mi.InvokeGeneric(this, idCountType.Yield().ToArray(),
                    new object[] { column, value });
        }
        else if (propertyType.IsGenericType 
                 && propertyType.GetGenericTypeDefinition() 
                    == typeof(HashSet<>))
        {
            var entryType = propertyType.GetGenericArguments()[0];
            var mi = this.GetType().GetMethod(nameof(UnpackHashSet),
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (TProperty) mi
                .InvokeGeneric(this, entryType.Yield().ToArray(),
                    new object[] { value, "Value" });
        }
        else if (typeof(Array).IsAssignableFrom(propertyType))
        {
            var entryType = propertyType.GetElementType();
            var mi = this.GetType().GetMethod(nameof(UnpackArray),
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (TProperty) mi
                .InvokeGeneric(this, entryType.Yield().ToArray(),
                    new object[] { value, "Value" });
        }
        else
        {
            GD.Print($"no way to import {propertyType.Name} {UnpackString(column["name"])}");
            // GD.Print($"{propertyType.Name} assignable from {typeof(IdCount<>)} {typeof(IdCount<>).IsAssignableFrom(propertyType)}");
            var genericTypeDef = propertyType.GetGenericTypeDefinition();
            GD.Print($"is generic type of {typeof(IdCount<>).Name} {genericTypeDef == typeof(IdCount<>)}");
            throw new Exception();
        }

        return default;
    }

    private IdCount<TValue> UnpackIdCount<TValue>(
        JsonObject column,
        JsonArray value)
        where TValue : IIdentifiable
    {
        var res = IdCount<TValue>.Construct();
        for (var i = 0; i < value.Count; i++)
        {
            var entryGuid = UnpackGuid(value[i]["Name"]);
            var entryOb = (TValue)LineObjects[entryGuid];
            var entryValue = UnpackFloat(value[i]["Value"]);
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
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class GenerationMultiSettings : MultiSettings
{
    public PlanetSettings PlanetSettings { get; private set; }
    public GeologySettings GeologySettings { get; private set; }
    public MoistureSettings MoistureSettings { get; private set; }
    public SocietySettings SocietySettings { get; private set; }
    //todo make proper int settings option
    public Vector2I Dimensions => new Vector2I((int)PlanetSettings.MapWidth.Value, (int)PlanetSettings.MapHeight.Value);
    
    public static GenerationMultiSettings Construct()
    {
        return new GenerationMultiSettings("Generation",
            PlanetSettings.Construct(), GeologySettings.Construct(), MoistureSettings.Construct(),
            SocietySettings.Construct());
    }
    [SerializationConstructor] private GenerationMultiSettings(string name, 
        PlanetSettings planetSettings, GeologySettings geologySettings, 
        MoistureSettings moistureSettings, SocietySettings societySettings) : base(name,
        new List<ISettings>
        {
            planetSettings, geologySettings,
            moistureSettings, societySettings
        })
    {
        PlanetSettings = planetSettings;
        GeologySettings = geologySettings;
        MoistureSettings = moistureSettings;
        SocietySettings = societySettings;
        
    }

    public static GenerationMultiSettings Load(Data data)
    {
        //todo fix this
        try
        {
            var f = GodotFileExt.LoadFileAs<GenerationMultiSettings>("", "genSettings",
                ".stng", data);
            return f;
        }
        catch (Exception e)
        {
            return Construct();
        }
        
    }

    public void Save(Data data)
    {
        GodotFileExt.SaveFile(this, "", "genSettings", ".stng", data);
    }
}

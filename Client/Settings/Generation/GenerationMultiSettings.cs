using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GenerationMultiSettings : MultiSettings
{
    public PlanetSettings PlanetSettings { get; private set; }
        = new PlanetSettings();
    public GeologySettings GeologySettings { get; private set; }
        = new GeologySettings();
    public MoistureSettings MoistureSettings { get; private set; }
        = new MoistureSettings();
    public SocietySettings SocietySettings { get; private set; }
        = new SocietySettings();
    public Vector2 Dimensions => new Vector2(PlanetSettings.MapWidth.Value, PlanetSettings.MapHeight.Value);
    public GenerationMultiSettings() : base("Generation")
    {
        Settings.AddRange(new List<ISettings>
        {
            PlanetSettings, GeologySettings, MoistureSettings, SocietySettings
        });
    }
    
}

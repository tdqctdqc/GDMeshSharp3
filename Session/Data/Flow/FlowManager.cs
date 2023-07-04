using System;
using System.Collections.Generic;
using System.Linq;

public class FlowManager : IModelManager<Flow>
{
    public Dictionary<string, Flow> Models { get; }
    public static Income Income { get; private set; } = new ();
    public static IndustrialPower IndustrialPower { get; private set; } = new ();
    public static ConstructionCap ConstructionCap { get; private set; } = new ();
    
    public FlowManager()
    {
        var buildings = GetType().GetStaticPropertiesOfType<Flow>();
        Models = buildings.ToDictionary(b => b.Name, b => b);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MoistureSettings : Settings
{
    public FloatSettingsOption Scale { get; private set; }
    public FloatSettingsOption EquatorDistMoistureMultWeight { get; private set; }
    public FloatSettingsOption RiverFlowPerMoisture { get; private set; }
    public FloatSettingsOption BaseRiverFlowCost { get; private set; }
    public FloatSettingsOption RiverFlowCostRoughnessMult { get; private set; }
    public FloatSettingsOption MoistureFlowRoughnessCostMult { get; private set; }
    public static MoistureSettings Construct()
    {
        return new MoistureSettings("Moisture",
            new FloatSettingsOption("Scale", 1f, 0f, 2f, .1f, false),
            new FloatSettingsOption("Equator Dist Moisture Mult Weight", .5f, 0f, 1f, .1f, false),
            new FloatSettingsOption("River Flow Per Moisture", 10f, 0f, 50f, 1f, false),
            new FloatSettingsOption("Base River Flow Cost", 100f, 0f, 1000f, 10f, false),
            new FloatSettingsOption("River Flow Cost Roughness Mult", 1f, 0f, 10f, 1f, false),
            new FloatSettingsOption("Moisture Flow Roughness Cost Mult", .5f, 0f, 1f, .1f, false)
        );
    }
    [SerializationConstructor] private MoistureSettings(string name, 
        FloatSettingsOption scale, FloatSettingsOption equatorDistMoistureMultWeight, 
        FloatSettingsOption riverFlowPerMoisture, FloatSettingsOption baseRiverFlowCost, 
        FloatSettingsOption riverFlowCostRoughnessMult, FloatSettingsOption moistureFlowRoughnessCostMult) 
        : base(name)
    {
        Scale = scale;
        EquatorDistMoistureMultWeight = equatorDistMoistureMultWeight;
        RiverFlowPerMoisture = riverFlowPerMoisture;
        BaseRiverFlowCost = baseRiverFlowCost;
        RiverFlowCostRoughnessMult = riverFlowCostRoughnessMult;
        MoistureFlowRoughnessCostMult = moistureFlowRoughnessCostMult;
    }
}

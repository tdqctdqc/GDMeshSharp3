using Godot;
using System.Collections.Generic;
using System.Linq;

public class Troop : Item, IMakeable, IIconed
{
    public string DisplayName { get; private set; }
    public float HardAttack { get; private set; }
    public float SoftAttack { get; private set; }
    public float Hitpoints { get; private set; }
    public float Hardness { get; private set; }
    public float Accuracy { get; private set; }
    public float Evasion { get; private set; }
    public int Echelon { get; private set; }
    public int Range { get; private set; }
    public float[] TargetChance { get; private set; }
    public float FrontLength { get; private set; }
    public float BreakthroughMult { get; private set; }
    public MakeableAttribute Makeable { get; private set; }
    public TroopDomain Domain { get; private set; }
    public Troop(string name, 
        TroopDomain domain,
        Dictionary<string, IModel> modelsByName,
        Dictionary<string, Dictionary<string, string>> info)
            : base(name)
    {
        // var thisInfo = info[name];
        //
        // DisplayName = thisInfo[nameof(DisplayName)];
        // HardAttack = thisInfo[nameof(HardAttack)].ToFloat();
        // SoftAttack = thisInfo[nameof(SoftAttack)].ToFloat();
        // Hitpoints = thisInfo[nameof(Hitpoints)].ToFloat();
        // Hardness = thisInfo[nameof(Hardness)].ToFloat();
        // Echelon = thisInfo[nameof(Echelon)].ToInt();
        // Accuracy = thisInfo[nameof(Accuracy)].ToFloat();
        // Evasion = thisInfo[nameof(Evasion)].ToFloat();
        // FrontLength = thisInfo[nameof(FrontLength)].ToFloat();
        // Range = thisInfo[nameof(Range)].ToInt();;
        // BreakthroughMult = thisInfo[nameof(BreakthroughMult)].ToFloat();;
        // Domain = domain;
        //
        // TargetChance = new float[MilUtil.NumEchelons];
        // for (var i = 0; i < MilUtil.NumEchelons; i++)
        // {
        //     var value = thisInfo[nameof(TargetChance) + i];
        //     TargetChance[i] = value.ToFloat();
        // }
        //
        // var buildCosts = IdCount<IModel>.Construct();
        // var maintainCosts = IdCount<IModel>.Construct();
        //
        // var buildCostEntries = thisInfo.Where(kvp => kvp.Key.StartsWith("BuildCost"));
        // var maintainCostEntries = thisInfo.Where(kvp => kvp.Key.StartsWith("MaintainCost"));
        //
        // foreach (var (entryName, valueString) in buildCostEntries)
        // {
        //     var modelName = entryName.TrimPrefix("BuildCost");
        //     var model = modelsByName[modelName];
        //     var value = valueString.ToFloat();
        //     GD.Print($"{modelName} {value}");
        //
        //     buildCosts.Set(model, value);
        // }
        // foreach (var (entryName, valueString) in maintainCostEntries)
        // {
        //     var modelName = entryName.TrimPrefix("MaintainCost");
        //     var model = modelsByName[modelName];
        //     var value = valueString.ToFloat();
        //     maintainCosts.Set(model, value);
        // }
        //
        // Makeable = new MakeableAttribute(buildCosts, maintainCosts);

    }
}
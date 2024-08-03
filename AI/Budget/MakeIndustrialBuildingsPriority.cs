
using MessagePack;

public class MakeIndustrialBuildingsPriority
    : MakeProductionBuildingsPriority
{
    public static MakeIndustrialBuildingsPriority Construct(Data d)
    {
        return new MakeIndustrialBuildingsPriority(
            d.Models.Items.IndustrialPower.MakeRef<IModel>(),
            "Make Industrial Buildings");
    }
    [SerializationConstructor] private MakeIndustrialBuildingsPriority(ModelRef<IModel> model, 
        string name) : base(model, name)
    {
    }

    public override float GetWeight(Regime r, Data d)
    {
        return 1f;
    }
}
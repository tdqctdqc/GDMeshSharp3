using MessagePack;

public class MakeResearchBuildingsPriority
    : MakeProductionBuildingsPriority
{
    public static MakeResearchBuildingsPriority Construct(Data d)
    {
        return new MakeResearchBuildingsPriority(
            d.Models.Items.Research.MakeRef<IModel>(),
            "Make Research Buildings");
    }
    [SerializationConstructor] private MakeResearchBuildingsPriority(ModelRef<IModel> model, 
        string name) : base(model, name)
    {
    }

    public override float GetWeight(Regime r, Data d)
    {
        return .5f;
    }
}
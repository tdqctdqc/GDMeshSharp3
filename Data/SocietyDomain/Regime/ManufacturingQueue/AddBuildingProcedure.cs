
public class AddBuildingProcedure : Procedure
{
    public ERef<Settlement> Settlement { get; private set; }
    public ModelRef<SettlementBuildingModel> Building { get; private set; }

    public AddBuildingProcedure(ERef<Settlement> settlement, ModelRef<SettlementBuildingModel> building)
    {
        Settlement = settlement;
        Building = building;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        Settlement.Get(key.Data).Buildings.Add(Building.RefId, 1f);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}
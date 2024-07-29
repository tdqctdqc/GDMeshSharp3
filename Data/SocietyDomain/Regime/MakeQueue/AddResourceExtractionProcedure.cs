
public class AddResourceExtractionProcedure : Procedure
{
    public ModelRef<ResourceExtractionBuilding> Model { get; private set; }
    public ERef<ResourceDeposit> Deposit { get; private set; }

    public AddResourceExtractionProcedure(ModelRef<ResourceExtractionBuilding> model, ERef<ResourceDeposit> deposit)
    {
        Model = model;
        Deposit = deposit;
    }

    public override void Enact(ProcedureKey key)
    {
        Deposit.Get(key.Data).SetExtraction(Model, key);
    }

    public override bool Valid(Data data, out string error)
    {
        var model = Model.Get(data);
        var deposit = Deposit.Get(data);
        if (deposit.Item.RefId != model.Resource(data).Id)
        {
            error = "wrong resource type";
            return false;
        }

        error = "";
        return true;
    }
}
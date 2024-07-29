
public class AddExtractionProcedure : Procedure
{
    public ERef<ResourceDeposit> ResourceDeposit { get; private set; }
    public ModelRef<ResourceExtractionBuilding> Extraction { get; private set; }

    public AddExtractionProcedure(ERef<ResourceDeposit> resourceDeposit, ModelRef<ResourceExtractionBuilding> extraction)
    {
        ResourceDeposit = resourceDeposit;
        Extraction = extraction;
    }

    public override void Enact(ProcedureKey key)
    {
        ResourceDeposit.Get(key.Data)
            .SetExtraction(Extraction, key);
    }

    public override bool Valid(Data data, out string error)
    {
        var rd = ResourceDeposit.Get(data);
        var e = Extraction.Get(data);

        if (rd.Extraction.Fulfilled())
        {
            error = "already have extraction";
            return false;
        }

        if (rd.Item.Get(data) != e.Resource(data))
        {
            error = "not matching resource";
            return false;
        }

        error = "";
        return true;
    }
}
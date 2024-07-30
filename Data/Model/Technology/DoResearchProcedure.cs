
public class DoResearchProcedure : Procedure
{
    public override void Enact(ProcedureKey key)
    {
        var regimes = key.Data.GetAll<Regime>();
        foreach (var regime in regimes)
        {
            var researchPoints = regime.Stock.Stock.Get(key.Data.Models.Items.Research);
            regime.Technology.AddProgress(researchPoints, key);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}
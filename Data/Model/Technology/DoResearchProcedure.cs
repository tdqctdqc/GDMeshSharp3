
public class DoResearchProcedure : Procedure
{
    public override void Enact(ProcedureWriteKey key)
    {
        var regimes = key.Data.GetAll<Regime>();
        foreach (var regime in regimes)
        {
            var tech = regime.Technology;
            var researching = tech.CurrentResearch.Get(key.Data);
            if (researching is not null)
            {
                tech.ResearchProgresses[tech.CurrentResearch] 
                    += regime.Stock.Stock.Get(key.Data.Models.Items.Research);
                if (tech.ResearchProgresses[tech.CurrentResearch] >= researching.ResearchCost)
                {
                    tech.Technologies.Add(tech.CurrentResearch);
                    tech.SetResearch(null, key);
                }
            }
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}
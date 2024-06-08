
using System.Linq;

public class CleanUpArmyMissionsProcedure : Procedure
{
    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var army in key.Data.GetAll<Army>())
        {
            var alliance = army.Regime.Get(key.Data).GetAlliance(key.Data);
            army.LineMission.CleanUp(army, key);
            foreach (var mission in army.OtherOrders.ToArray())
            {
                var keep = mission.CleanUp(army, key);
                if (keep == false)
                {
                    army.OtherOrders.Remove(mission);
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

using System.Linq;

public class CleanUpArmyMissionsProcedure : Procedure
{
    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var army in key.Data.GetAll<Army>())
        {
            var alliance = army.Regime.Get(key.Data).GetAlliance(key.Data);
            army.OtherOrders.Clear();
            var lost = army.LineMission.LineCells
                .Where(c =>
                {
                    var cell = PlanetDomainExt.GetPolyCell(c, key.Data);
                    return alliance.Members.RefIds.Contains(cell.Controller.RefId)
                           == false;
                })
                .ToArray();
            army.LineMission.AdvanceInto.UnionWith(lost);
            army.LineMission.LineCells.ExceptWith(lost);
            var conquered = army.LineMission.AdvanceInto
                .Where(i =>
                {
                    var cell = PlanetDomainExt.GetPolyCell(i, key.Data);
                    return alliance.Members.RefIds.Contains(cell.Controller.RefId);
                })
                .ToArray();
            army.LineMission.LineCells.UnionWith(conquered);
            army.LineMission.AdvanceInto.ExceptWith(conquered);
            
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}
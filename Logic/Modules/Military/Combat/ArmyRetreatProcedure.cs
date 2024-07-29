
using System.Linq;

public class ArmyRetreatProcedure : Procedure
{
    public CellRef[] Froms { get; private set; }
    public CellRef[] Tos { get; private set; }
    public ERef<Army> Army { get; private set; }

    public ArmyRetreatProcedure(CellRef[] froms, CellRef[] tos, 
        ERef<Army> army)
    {
        Froms = froms;
        Tos = tos;
        Army = army;
    }

    public override void Enact(ProcedureKey key)
    {
        var army = Army.Get(key.Data);
        army.Cells.Remove(Froms, key);
        army.Cells.Add(Tos, key);
    }

    public override bool Valid(Data data, out string error)
    {
        var alliance = Army.Get(data)
            .Regime.Get(data).GetAlliance(data);
        
        foreach (var to in Tos)
        {
            if (to.Fulfilled()
                && to.Get(data).FriendlyControlled(alliance, data) == false)
            {
                error = "Retreating to non-friendly cell";
                return false;
            }
        }
        

        error = "";
        return true;
    }
}
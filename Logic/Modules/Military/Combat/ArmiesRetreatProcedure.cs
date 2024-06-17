
using System.Linq;

public class ArmiesRetreatProcedure : Procedure
{
    public CellRef From { get; private set; }
    public CellRef[] Tos { get; private set; }
    public ERef<Army>[] Armies { get; private set; }

    public ArmiesRetreatProcedure(CellRef from, CellRef[] tos, 
        ERef<Army>[] armies)
    {
        From = from;
        Tos = tos;
        Armies = armies;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var aRef in Armies)
        {
            var army = aRef.Get(key.Data);
            army.Cells.Remove(From, key);
            foreach (var to in Tos)
            {
                army.Cells.Add(to, key);
            }
        }
    }

    public override bool Valid(Data data, out string error)
    {
        if (Armies.Length == 0)
        {
            error = "No armies";
            return false;
        }
        var alliance = Armies.First().Get(data)
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
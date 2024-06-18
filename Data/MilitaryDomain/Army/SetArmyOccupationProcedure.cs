using System.Collections.Generic;

public class SetArmyOccupationProcedure : Procedure
{
    public RefSet<CellRef> NewOccupation { get; private set; }
    public ERef<Army> Army { get; private set; }

    public SetArmyOccupationProcedure(RefSet<CellRef> newOccupation, ERef<Army> army)
    {
        NewOccupation = newOccupation;
        Army = army;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var army = Army.Get(key.Data);
        army.SetCells(NewOccupation, key);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}
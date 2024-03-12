
using MessagePack;

public class ChangePolyCellControllerProcedure : Procedure
{
    public CellRef Cell { get; private set; }
    public ERef<Regime> NewControllerRegime { get; private set; }

    public static ChangePolyCellControllerProcedure Construct(Cell cell, Regime newControllerRegime)
    {
        return new ChangePolyCellControllerProcedure(cell.MakeRef(), 
            newControllerRegime.MakeRef());
    }
    [SerializationConstructor] private ChangePolyCellControllerProcedure(
        CellRef cell, 
        ERef<Regime> newControllerRegime)
    {
        Cell = cell;
        NewControllerRegime = newControllerRegime;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var cell = Cell.Get(key.Data);
        var newController = NewControllerRegime.Get(key.Data);
        var oldController = cell.Controller.IsEmpty() ? null : cell.Controller.Get(key.Data);
        cell.SetController(newController, key);
        key.Data.Notices.CellChangedController.Invoke((cell, oldController, newController));
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}

using MessagePack;

public class ChangePolyCellControllerProcedure : Procedure
{
    public int CellId { get; private set; }
    public int NewControllerRegimeId { get; private set; }

    public static ChangePolyCellControllerProcedure Construct(Cell cell, Regime newControllerRegime)
    {
        return new ChangePolyCellControllerProcedure(cell.Id, newControllerRegime.Id);
    }
    [SerializationConstructor] private ChangePolyCellControllerProcedure(int cellId, int newControllerRegimeId)
    {
        CellId = cellId;
        NewControllerRegimeId = newControllerRegimeId;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var cell = PlanetDomainExt.GetPolyCell(CellId, key.Data);
        var newController = key.Data.Get<Regime>(NewControllerRegimeId);
        var oldController = cell.Controller.IsEmpty() ? null : cell.Controller.Entity(key.Data);
        cell.SetController(newController, key);
        key.Data.Notices.CellChangedController.Invoke((cell, oldController, newController));
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}
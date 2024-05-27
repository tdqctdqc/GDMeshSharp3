
using System;

public class CreateArmyCommand : Command
{
    public CellRef Cell { get; private set; }
    public CreateArmyCommand(CellRef cell, Guid commandingPlayerGuid) : base(commandingPlayerGuid)
    {
        Cell = cell;
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        var player = data.BaseDomain.PlayerAux.ByGuid[CommandingPlayerGuid];
        var regime = player.Regime.Get(data);
        var cell = Cell.Get(data);
        if (regime.GetAlliance(data).Members.RefIds
                .Contains(cell.Controller.RefId) == false)
        {
            error = "regime alliance does not control cell";
            return false;
        }

        return true;
    }

    public override void Enact(LogicWriteKey key)
    {
        var player = key.Data.BaseDomain.PlayerAux.ByGuid[CommandingPlayerGuid];
        var regime = player.Regime.Get(key.Data);
        Army.Create(regime, Cell.Get(key.Data), new int[] { }, key);
    }
}
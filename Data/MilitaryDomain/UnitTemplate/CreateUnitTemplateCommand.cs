
using System;
using System.Collections.Generic;

public class CreateUnitTemplateCommand : Command
{
    public ModelRef<TroopDomain> TroopDomain { get; private set; }
    public CreateUnitTemplateCommand(ModelRef<TroopDomain> troopDomain,
        Guid commandingPlayerGuid) : base(commandingPlayerGuid)
    {
        TroopDomain = troopDomain;
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }

    public override void Enact(LogicWriteKey key)
    {
        var player = key.Data.BaseDomain.PlayerAux.ByGuid[CommandingPlayerGuid];
        var regime = player.Regime.Get(key.Data);
        UnitTemplate.Create(key, "New Unit Template",
            new Dictionary<TroopType, float>(),
            TroopDomain.Get(key.Data), regime);
    }
}
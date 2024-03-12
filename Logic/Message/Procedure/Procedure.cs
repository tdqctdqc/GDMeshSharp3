using Godot;
using System;


[MessagePack.Union(0, typeof(ChangePolyCellControllerProcedure))]
[MessagePack.Union(1, typeof(ReinforceUnitProcedure))]
[MessagePack.Union(2, typeof(DecideOnProposalProcedure))]
[MessagePack.Union(3, typeof(DeclareRivalProcedure))]
[MessagePack.Union(4, typeof(DeclareWarProcedure))]
[MessagePack.Union(5, typeof(FinishedTurnEndCalcProc))]
[MessagePack.Union(6, typeof(FinishedTurnStartCalcProc))]
[MessagePack.Union(7, typeof(FoodAndPopGrowthProcedure))]
[MessagePack.Union(8, typeof(FormUnitProcedure))]
[MessagePack.Union(9, typeof(GrowFinancialPowerProcedure))]
[MessagePack.Union(10, typeof(HandleUnitOrdersProcedure))]
[MessagePack.Union(11, typeof(MakeProposalProcedure))]
[MessagePack.Union(12, typeof(MoveUnitProcedure))]
[MessagePack.Union(13, typeof(PrepareNewHistoriesProcedure))]
[MessagePack.Union(14, typeof(ProduceConstructProcedure))]
[MessagePack.Union(15, typeof(RegimeUseItemsProcedure))]
[MessagePack.Union(16, typeof(RegimeUseTroopsProcedure))]
[MessagePack.Union(17, typeof(SetContextProcedure))]
[MessagePack.Union(18, typeof(SetUnitGroupProcedure))]
[MessagePack.Union(19, typeof(SetUnitOrderProcedure))]
[MessagePack.Union(20, typeof(StartMakeProjectProc))]
[MessagePack.Union(21, typeof(TickProcedure))]
[MessagePack.Union(22, typeof(TradeProcedure))]
[MessagePack.Union(23, typeof(TroopLossesProcedure))]
[MessagePack.Union(24, typeof(SetPlayerRegimeProcedure))]
public abstract class Procedure : Message, IPolymorph
{
    protected Procedure()
    {
        
    }
    public abstract void Enact(ProcedureWriteKey key);
    public abstract bool Valid(Data data, out string error);
}


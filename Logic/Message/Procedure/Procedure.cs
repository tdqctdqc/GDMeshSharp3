using Godot;
using System;


[MessagePack.Union(0, typeof(ConquerCellProcedure))]
[MessagePack.Union(1, typeof(ReinforceRegimeProcedure))]
[MessagePack.Union(2, typeof(DecideOnProposalProcedure))]
[MessagePack.Union(3, typeof(DeclareRivalProcedure))]
[MessagePack.Union(4, typeof(DeclareWarProcedure))]
[MessagePack.Union(5, typeof(FinishedTurnEndCalcProc))]
[MessagePack.Union(6, typeof(FinishedTurnStartCalcProc))]
[MessagePack.Union(7, typeof(MigrationProcedure))]
[MessagePack.Union(8, typeof(FormUnitProcedure))]
[MessagePack.Union(9 , typeof(HandleUnitMissionsProcedure))]
[MessagePack.Union(10, typeof(MakeProposalProcedure))]
[MessagePack.Union(11, typeof(SetPlayerRegimeProcedure))]
[MessagePack.Union(12, typeof(ProdResultProcedure))]
[MessagePack.Union(13, typeof(RegimeUseItemsProcedure))]
[MessagePack.Union(14, typeof(RegimeUseTroopsProcedure))]
[MessagePack.Union(15, typeof(ArmyRetreatProcedure))]
[MessagePack.Union(16, typeof(SetUnitArmyProcedure))]
[MessagePack.Union(17, typeof(SetUnitOrderProcedure))]
[MessagePack.Union(18, typeof(StartMakeProjectProc))]
[MessagePack.Union(19, typeof(TickProcedure))]
[MessagePack.Union(20, typeof(TradeProcedure))]
[MessagePack.Union(21, typeof(TroopLossesProcedure))]
[MessagePack.Union(22, typeof(CleanUpArmyMissionsProcedure))]
[MessagePack.Union(23, typeof(SetArmyOccupationProcedure))]
[MessagePack.Union(24, typeof(DestroyArmyProcedure))]
[MessagePack.Union(25, typeof(AddBuildingProcedure))]
[MessagePack.Union(26, typeof(CancelMakeProjectProcedure))]
[MessagePack.Union(27, typeof(DoClientCallbackProcedure))]
[MessagePack.Union(28, typeof(AddCombatHistoryProc))]
[MessagePack.Union(29, typeof(ReinforceUnitProcedure))]
[MessagePack.Union(30, typeof(ReinforceArmyProcedure))]
[MessagePack.Union(31, typeof(ReinforceUnitTroopProcedure))]
[MessagePack.Union(32, typeof(AggregateProcedure))]
[MessagePack.Union(33, typeof(ChangeMakeProjectPriorityProcedure))]

public abstract class Procedure : Message, IPolymorph
{
    protected Procedure()
    {
        
    }
    public abstract void Enact(ProcedureWriteKey key);
    public abstract bool Valid(Data data, out string error);
}


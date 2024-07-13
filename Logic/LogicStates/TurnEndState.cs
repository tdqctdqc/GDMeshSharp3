
using System;

public class TurnEndState : TurnState
{
    public TurnEndState(LogicWriteKey key, OrderHolder orders) 
        : base(key, orders)
    {
        _majorModules = new LogicModule[]
        {
            new DoTurnOrderProceduresModule(),
            new AllianceOrdersModule(),
            new DefaultLogicModule(() => new TickProcedure()),
            new ClearOrdersModule(orders)
        };
        _minorModules = new LogicModule[]
        {
            new DoTurnOrderProceduresModule(),
            new HandleUnitMissionsModule(),
            new CombatModule(),
            new DefaultLogicModule(() => new CleanUpArmyMissionsProcedure()),
            new DefaultLogicModule(() => new TickProcedure()),
            new ClearOrdersModule(orders)
        };
    }
}
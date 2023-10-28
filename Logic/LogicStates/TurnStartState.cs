
public class TurnStartState : TurnState
{
    public TurnStartState(Data data) : base(data)
    {
        _majorModules = new LogicModule[]
        {
            new DefaultLogicModule(() => new SetContextProcedure()),
            new TrimFrontsModule(),
            new DefaultLogicModule(() => new FinishedTurnStartCalcProc())
        };
        _minorModules = new LogicModule[]
        {
            new DefaultLogicModule(() => new SetContextProcedure()),
            new DefaultLogicModule(() => new FinishedTurnStartCalcProc())
        };
    }
}
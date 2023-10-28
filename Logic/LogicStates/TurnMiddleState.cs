
public class TurnMiddleState : TurnState
{
    public TurnMiddleState(Data data, OrderHolder holder) : base(data)
    {
        _majorModules = new LogicModule[]
        {
            new WaitForOrdersToBeSubmittedModule(holder)
        };
        _minorModules = new LogicModule[] 
        {
            new WaitForOrdersToBeSubmittedModule(holder)
        };
    }
}
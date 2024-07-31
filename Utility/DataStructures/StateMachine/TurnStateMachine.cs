
public class TurnStateMachine
{
    public TurnState Current { get; private set; }

    public TurnStateMachine(TurnState current)
    {
        Current = current;
        Current.Calculate();
    }

    public void Process()
    {
        if (Current.Calculating == false 
            && Current.ReadyForNext())
        {
            Current = Current.NextState;
            Current.Calculate();
        }
    }
}
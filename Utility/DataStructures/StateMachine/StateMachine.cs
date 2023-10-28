
public class StateMachine
{
    public State Current { get; private set; }

    public StateMachine(State current)
    {
        Current = current;
    }

    public void Process()
    {
        var newCurr = Current.Check();
        if (newCurr != Current)
        {
            Current = newCurr;
            Current.Enter();
        }
    }
}
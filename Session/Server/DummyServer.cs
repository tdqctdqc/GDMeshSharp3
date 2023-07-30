using Godot;

public partial class DummyServer : Node, IServer 
{
    public void QueueCommandLocal(Command c)
    {
        return;
    }
}


using Godot;

public partial class TickDisplay : Label
{
    private RefAction<int> _tick;

    public static TickDisplay Create(Data data)
    {
        var d = new TickDisplay();
        data.Notices.Ticked.Subscribe(d._tick);
        d.TreeExiting += () => data.Notices.Ticked.Unsubscribe(d._tick);
        return d;
    }
    private TickDisplay()
    {
        _tick = new RefAction<int>();
        _tick.Subscribe(n => Text = $"Tick: {n}");
    }

    private void Setup(Data data)
    {
        
    }
    public override void _ExitTree()
    {
        _tick.EndSubscriptions();
    }
}

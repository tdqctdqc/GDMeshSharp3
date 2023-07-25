
using Godot;

public partial class TickDisplay : Label
{

    public static TickDisplay Create(Data data)
    {
        var d = new TickDisplay();
        data.Notices.Ticked.SubscribeForNode(n => d.Text = $"Tick: {n}", d);
        return d;
    }
}


using Godot;

public partial class TickDisplay : Label
{

    public static TickDisplay Create(Client client, Data data)
    {
        var d = new TickDisplay();
        data.Notices.Ticked.SubscribeForNode(
            n => client.QueuedUpdates.Enqueue(
                () => d.Text = $"Tick: {n}"),
            d
        );
        return d;
    }
}

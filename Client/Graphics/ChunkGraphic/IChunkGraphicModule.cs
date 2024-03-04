using Godot;

public interface IChunkGraphicModule
{
    void Draw(Data d);
    Node2D Node { get; }
    void RegisterForRedraws(Data d);
}

public static class IChunkGraphicExt
{
    public static void RegisterDrawOnTick(this IChunkGraphicModule m,
        Data d)
    {
        d.Notices.Ticked.SubscribeForNode(i =>
        {
            m.Draw(d);
        }, m.Node);
    }
}


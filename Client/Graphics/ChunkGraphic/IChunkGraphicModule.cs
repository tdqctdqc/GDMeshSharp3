using System;
using System.Linq;
using Godot;

public interface IChunkGraphicModule
{
    string Name { get; }
    void Draw(Data d);
    Node2D Node { get; }
    void RegisterForRedraws(Data d);
    Settings GetSettings(Data d);
    ChunkGraphicModuleVisibility Visibility { get; }
}

public static class IChunkGraphicExt
{
    public static void RegisterDrawOnTick(this IChunkGraphicModule m,
        Data d)
    {
        d.Notices.Ticked.SubscribeForNode(i =>
        {
            Game.I.Client.QueuedUpdates.Enqueue(() => m.Draw(d));
        }, m.Node);
    }

    public static BoolSettingsOption MakeVisibilitySetting<TGraphic>(
        this TGraphic graphic,
        bool startVisible)
        where TGraphic : IChunkGraphicModule
    {
        var setting = new BoolSettingsOption("Visibility", startVisible);
        AddSettingEnforcement<bool, TGraphic>(graphic, setting,
            (v, g) => g.Visibility.VisibleOverride = v);
        return setting;
    }
    
    public static FloatSettingsOption MakeTransparencySetting<TGraphic>(
        this TGraphic graphic)
        where TGraphic : IChunkGraphicModule
    {
        var setting = new FloatSettingsOption("Transparency",
            1f, .1f, 1f, .1f, false);
        AddSettingEnforcement<float, TGraphic>(graphic, setting,
            (a, g) => g.Node.Modulate = new Color(Colors.White, a));
        return setting;
    }
    public static void AddSettingEnforcement<T, TGraphic>(this TGraphic graphic,
        SettingsOption<T> setting,
        Action<T, TGraphic> enforce) where TGraphic : IChunkGraphicModule
    {
        setting.SettingChanged.Subscribe(n =>
        {
            var newVal = n.newVal;
            var chunks = Game.I.Client.GetComponent<MapGraphics>()
                .GraphicLayerHolder.Chunks;
            foreach (var kvp in chunks)
            {
                var graphic = kvp.Value.GetModules()
                    .OfType<TGraphic>().First();
                enforce(newVal, graphic);
            }
        });
    }
}


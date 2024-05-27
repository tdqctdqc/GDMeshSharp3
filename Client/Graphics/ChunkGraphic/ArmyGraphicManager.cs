
public class ArmyGraphicManager : ISettinged
{
    public EntityGraphicReservoir<Army, ArmyGraphic> ArmyGraphics { get; private set; }

    public ArmyGraphicManager(Client c)
    {
        ArmyGraphics = new EntityGraphicReservoir<Army, ArmyGraphic>(
            a =>
            {
                var g = new ArmyGraphic();
                g.Draw(a, c);
                return g;
            }, c.Data);
        c.Data.Notices.Ticked.Subscribe(i =>
        {
            DrawAll(c);
        });
        c.Data.Notices.FinishedStateSync.Subscribe(() =>
        {
            DrawAll(c);
        });
        c.Data.Notices.Gen.FinishedGen.Subscribe(() =>
        {
            DrawAll(c);
        });
    }

    private void DrawAll(Client c)
    {
        foreach (var (army, graphic) in ArmyGraphics.Graphics)
        {
            graphic.Draw(army, c);
        }
    }
    
    public Settings GetSettings()
    {
        var settings = new Settings("Armies");
        var visibility = new BoolSettingsOption(
            "Visibility", true);
        visibility.SettingChanged.Subscribe(
            v =>
            {
                foreach (var graphic in ArmyGraphics.Graphics.Values)
                {
                    graphic.Visible = v.newVal;
                }
            });
        settings.SettingsOptions.Add(visibility);
        return settings;
    }
}
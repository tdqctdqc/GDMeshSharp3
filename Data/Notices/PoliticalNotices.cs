
public class PoliticalNotices
{
    public ValChangeAction<MapPolygon, Regime> ChangedOwnerRegime { get; private set; }
        = new();
    public ValChangeAction<MapPolygon, Regime> ChangedOccupierRegime { get; private set; }
        = new();
    public ValChangeAction<Cell, Regime> ChangedControllerRegime { get; private set; }
        = new();
    public RefAction<(Regime, Regime)> WarDeclared { get; private set; }
        = new();
    public RefAction<(Regime, Regime)> WarEnded { get; private set; }
        = new();
}
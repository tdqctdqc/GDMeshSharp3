
public class PoliticalNotices
{
    public ValChangeAction<MapPolygon, Regime> ChangedOwnerRegime { get; private set; }
        = new();
    public ValChangeAction<MapPolygon, Regime> ChangedOccupierRegime { get; private set; }
        = new();
    public ValChangeAction<Cell, Regime> ChangedControllerRegime { get; private set; }
        = new();
    public RefAction<(Alliance, Regime)> AllianceAddedRegime { get; private set; }
        = new();
    public RefAction<(Alliance, Regime)> AllianceRemovedRegime { get; private set; }
        = new();
    public RefAction<(Alliance, Alliance)> RivalryDeclared { get; private set; }
        = new();
    public RefAction<(Alliance, Alliance)> RivalryEnded { get; private set; }
        = new();
    public RefAction<(Alliance, Alliance)> WarDeclared { get; private set; }
        = new();
    public RefAction<(Alliance, Alliance)> WarEnded { get; private set; }
        = new();
    public RefAction<Alliance> AllianceDissolved { get; private set; }
        = new();
}
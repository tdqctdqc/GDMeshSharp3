
public class GenNotices
{
    public RefAction GeneratedRegimes { get; private set; }
        = new();
    public RefAction SetPolyShapes { get; private set; }
        = new();
    public RefAction MadeCells { get; private set; }
        = new();
    public RefAction SetLandAndSea { get; private set; }
        = new();
    public RefAction FinishedGen { get; private set; }
        = new();
    public RefAction ExitedGen { get; private set; }
        = new();
}
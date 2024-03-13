namespace GDMeshSharp3.Data.Notices;

public class PlayerNotices
{
    public ValChangeAction<Player, Regime> PlayerChangedRegime { get; private set; }
        = new();

    public RefAction SetLocalPlayer { get; private set; }
        = new();
}
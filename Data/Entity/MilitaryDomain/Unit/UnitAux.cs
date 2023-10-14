
public class UnitAux
{
    public EntityMultiIndexer<Regime, Unit> ByRegime { get; private set; }
    public EntityMultiIndexer<Regime, UnitTemplate> UnitTemplates { get; private set; }

    public UnitAux(Data d)
    {
        ByRegime = new EntityMultiIndexer<Regime, Unit>(d,
            u => u.Regime.Entity(d), new RefAction[] { },
            new ValChangeAction<Unit, Regime>[] { });
        UnitTemplates = new EntityMultiIndexer<Regime, UnitTemplate>(
            d, t => t.Regime.Entity(d), new RefAction[] { },
            new ValChangeAction<UnitTemplate, Regime>[] { });
    }
}
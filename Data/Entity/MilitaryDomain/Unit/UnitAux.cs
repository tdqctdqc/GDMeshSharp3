
public class UnitAux
{
    public EntityMultiIndexer<Regime, Unit> UnitByRegime { get; private set; }
    public EntityRefColIndexer<UnitGroup, Unit> UnitByGroup { get; private set; }
    public EntityMultiIndexer<Regime, UnitGroup> UnitGroupByRegime { get; private set; }
    public EntityMultiIndexer<Regime, UnitTemplate> UnitTemplates { get; private set; }
    public ValChangeAction<Unit, UnitGroup> UnitChangedGroup { get; private set; }
    public UnitAux(Data d)
    {
        UnitByRegime = new EntityMultiIndexer<Regime, Unit>(d,
            u => u.Regime.Entity(d), new RefAction[] { },
            new ValChangeAction<Unit, Regime>[] { });
        UnitGroupByRegime = new EntityMultiIndexer<Regime, UnitGroup>(d,
            g => g.Regime.Entity(d), new RefAction[] { },
            new ValChangeAction<UnitGroup, Regime>[] { });
        UnitTemplates = new EntityMultiIndexer<Regime, UnitTemplate>(
            d, t => t.Regime.Entity(d), new RefAction[] { },
            new ValChangeAction<UnitTemplate, Regime>[] { });
        UnitByGroup = new EntityRefColIndexer<UnitGroup, Unit>(
            g => g.Units.Items(d),  
            d.GetEntityMeta<UnitGroup>().GetRefColMeta<Unit>(nameof(UnitGroup.Units)),
            d);
        UnitChangedGroup = new ValChangeAction<Unit, UnitGroup>();
    }
}
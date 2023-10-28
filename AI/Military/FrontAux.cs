
public class FrontAux
{
    public EntityMultiIndexer<Regime, Front> Fronts { get; private set; }

    public FrontAux(Data data)
    {
        Fronts = new EntityMultiIndexer<Regime, Front>(data,
            f => f.Regime.Entity(data),
            new RefAction[] { },
            new ValChangeAction<Front, Regime>[] { });
    }
}
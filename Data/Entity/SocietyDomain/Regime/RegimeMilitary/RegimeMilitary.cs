
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimeMilitary
{
    public EntityRef<Regime> Regime { get; private set; }
    public IdCount<Troop> TroopReserve { get; private set; }
    public EntRefCol<Front> Fronts { get; private set; }
    public static RegimeMilitary Construct(int regimeId, Data data)
    {
        return new RegimeMilitary(new EntityRef<Regime>(regimeId), IdCount<Troop>.Construct(),
            EntRefCol<Front>.Construct("Fronts", regimeId, new HashSet<int>(), data));
    }

    [SerializationConstructor] 
    private RegimeMilitary(EntityRef<Regime> regime,
        IdCount<Troop> troopReserve,
        EntRefCol<Front> fronts)
    {
        Regime = regime;
        TroopReserve = troopReserve;
        Fronts = fronts;
    }

    
}
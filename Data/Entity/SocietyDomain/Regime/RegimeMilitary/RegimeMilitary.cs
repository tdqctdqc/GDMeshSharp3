
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimeMilitary
{
    public EntityRef<Regime> Regime { get; private set; }
    public IdCount<Troop> TroopReserve { get; private set; }
    public static RegimeMilitary Construct(int regimeId, Data data)
    {
        return new RegimeMilitary(new EntityRef<Regime>(regimeId), IdCount<Troop>.Construct());
    }

    [SerializationConstructor] 
    private RegimeMilitary(EntityRef<Regime> regime,
        IdCount<Troop> troopReserve)
    {
        Regime = regime;
        TroopReserve = troopReserve;
    }

    
}
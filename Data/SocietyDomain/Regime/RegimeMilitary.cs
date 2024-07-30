
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimeMilitary
{
    public ERef<Regime> Regime { get; private set; }
    public static RegimeMilitary Construct(int regimeId, Data data)
    {
        return new RegimeMilitary(new ERef<Regime>(regimeId));
    }

    [SerializationConstructor] 
    private RegimeMilitary(ERef<Regime> regime)
    {
        Regime = regime;
    }

    public Troop GetBestTroopOfType(TroopType type, Data d)
    {
        var regime = Regime.Get(d);
        return d.Models.GetModels<Troop>()
            .Where(t => t.TroopType == type 
                && regime.HasPrereqs(t))
            .OrderByDescending(t => t.GetPowerPoints())
            .FirstOrDefault();
    }
    
}
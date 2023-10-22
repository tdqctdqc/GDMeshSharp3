
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimeMilitary
{
    public EntityRef<Regime> Regime { get; private set; }
    public IdCount<Troop> TroopReserve { get; private set; }
    public HashSet<Front> Fronts { get; private set; }
    public static RegimeMilitary Construct(int regimeId)
    {
        return new RegimeMilitary(new EntityRef<Regime>(regimeId), IdCount<Troop>.Construct(),
            new HashSet<Front>());
    }

    [SerializationConstructor] 
    private RegimeMilitary(EntityRef<Regime> regime,
        IdCount<Troop> troopReserve,
        HashSet<Front> fronts)
    {
        Regime = regime;
        TroopReserve = troopReserve;
        Fronts = fronts;
    }

    public void TrimFronts(ProcedureWriteKey key)
    {
        var alliance = Regime.Entity(key.Data).GetAlliance(key.Data);
        var controlled = key.Data.HostLogicData.Context
            .ControlledAreas[alliance];
        
        foreach (var front in Fronts.ToList())
        {
            var valid = front.Trim(controlled, key);
            if (valid == false)
            {
                FixFront(front, key, controlled);
            }
        }
    }

    private void FixFront(Front front, ProcedureWriteKey key,
         HashSet<Waypoint> controlled)
    {
        Fronts.Remove(front);
        if (front.Frontline.Count() > 0)
        {
            var unions = UnionFind.Find(front.Frontline,
                (w, v) => controlled.Contains(w) && controlled.Contains(v),
                w => w.GetNeighboringWaypoints(key.Data));
            foreach (var wps in unions)
            {
                var newFront = Front.Construct();
                newFront.Frontline.AddRange(wps);
                Fronts.Add(newFront);
            }
        }
    }
}
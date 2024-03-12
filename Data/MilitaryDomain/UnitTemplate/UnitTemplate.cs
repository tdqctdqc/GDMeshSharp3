
using System.Collections.Generic;
using MessagePack;

public class UnitTemplate : Entity
{
    public string Name { get; private set; }
    public IdCount<Troop> TroopCounts { get; private set; }
    public ERef<Regime> Regime { get; private set; }
    public ModelRef<MoveType> MoveType { get; private set; }
    public TroopDomain Domain { get; private set; }
    public static UnitTemplate Create(ICreateWriteKey key, 
        string name,
        Dictionary<Troop, float> troopCounts,
        TroopDomain domain,
        MoveType moveType,
        Regime regime)
    {
        var costs = IdCount<IModel>.Construct();
        foreach (var kvp in troopCounts)
        {
            var troop = kvp.Key;
            var numTroop = kvp.Value;
            foreach (var cost in troop.Makeable.BuildCosts.Contents)
            {
                costs.Add(cost.Key, cost.Value * numTroop);
            }
        }
        var u = new UnitTemplate(name, IdCount<Troop>.Construct(troopCounts),
            moveType.MakeRef(), regime.MakeRef(),
            key.Data.IdDispenser.TakeId(),
            domain);
        key.Create(u);
        return u;
    }
    [SerializationConstructor] private UnitTemplate(string name,
        IdCount<Troop> troopCounts, ModelRef<MoveType> moveType,
        ERef<Regime> regime, int id, 
        TroopDomain domain) 
        : base(id)
    {
        MoveType = moveType;
        Name = name;
        TroopCounts = troopCounts;
        Regime = regime;
        Domain = domain;
    }

    public static void CreateDefaultTemplatesForRegime(Regime r, 
        ICreateWriteKey key)
    {
        var inf = Create(key, "Infantry Division",
            new Dictionary<Troop, float>
                {
                    {key.Data.Models.Troops.Rifle1, 100f}
                }, TroopDomain.Land, key.Data.Models.MoveTypes.InfantryMove,
            r);
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}
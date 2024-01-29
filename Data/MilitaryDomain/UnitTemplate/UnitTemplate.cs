
using System.Collections.Generic;
using MessagePack;

public class UnitTemplate : Entity, IMakeable
{
    public string Name { get; private set; }
    public IdCount<Troop> TroopCounts { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public MakeableAttribute Makeable { get; private set; }
    public ModelRef<MoveType> MoveType { get; private set; }
    public TroopDomain Domain { get; private set; }
    public static UnitTemplate Create(ICreateWriteKey key, 
        string name,
        Dictionary<Troop, float> troopCounts,
        TroopDomain domain,
        MoveType moveType,
        Regime regime)
    {
        var itemCosts = IdCount<Item>.Construct();
        var industrialCost = 0f;
        foreach (var kvp in troopCounts)
        {
            var troop = kvp.Key;
            var numTroop = kvp.Value;
            foreach (var itemCost in troop.Makeable.ItemCosts.Contents)
            {
                itemCosts.Add(itemCost.Key, itemCost.Value * numTroop);
            }

            industrialCost += troop.Makeable.IndustrialCost * numTroop;
        }
        var u = new UnitTemplate(name, IdCount<Troop>.Construct(troopCounts),
            moveType.MakeRef(), regime.MakeRef(),
            key.Data.IdDispenser.TakeId(),
            new MakeableAttribute(itemCosts, industrialCost),
            domain);
        key.Create(u);
        return u;
    }
    [SerializationConstructor] protected UnitTemplate(string name,
        IdCount<Troop> troopCounts, ModelRef<MoveType> moveType,
        EntityRef<Regime> regime, int id, 
        MakeableAttribute makeable,
        TroopDomain domain) 
        : base(id)
    {
        MoveType = moveType;
        Name = name;
        Makeable = makeable;
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
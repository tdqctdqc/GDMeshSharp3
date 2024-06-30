
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class UnitCombatInfo
{
    public int Id { get; private set; }
    public ERef<UnitTemplate> Template { get; private set; }
    public IdCount<Troop> Active { get; private set; }
    public IdCount<Troop> Initial { get; private set; }
    public IdCount<Troop> Kills { get; private set; }
    public float ActiveFrontSize { get; private set; }
    public UnitCombatInfo(Unit u, Data d)
    {
        Id = u.Id;
        Active = IdCount<Troop>.Construct(u.Troops);
        Initial = IdCount<Troop>.Construct(u.Troops);
        Kills = IdCount<Troop>.Construct();
        Template = u.Template;
        ActiveFrontSize = Active.GetEnumModel(d)
            .Sum(v => v.Key.FrontLength * v.Value);
    }
    public UnitCombatInfo(IdCount<Troop> troops,
        Data d)
    {
        Id = -1;
        Active = IdCount<Troop>.Construct(troops);
        Initial = IdCount<Troop>.Construct(troops);
        Kills = IdCount<Troop>.Construct();
        Template = new ERef<UnitTemplate>(-1);
        ActiveFrontSize = Active.GetEnumModel(d)
            .Sum(v => v.Key.FrontLength * v.Value);
    }
    [SerializationConstructor] private UnitCombatInfo(
        int id, IdCount<Troop> active, 
        IdCount<Troop> initial, 
        IdCount<Troop> kills,
        ERef<UnitTemplate> template,
        float activeFrontSize)
    {
        Id = id;
        Active = active;
        Initial = initial;
        Kills = kills;
        ActiveFrontSize = activeFrontSize;
        Template = template;
    }

    public float ProportionLosses(Data d)
    {
        var initial = Initial.GetEnumModel(d).Sum(
            v => v.Key.GetPowerPoints() * v.Value);

        var active = Active.GetEnumModel(d).Sum(
            v => v.Key.GetPowerPoints() * v.Value);
        return 1f - active / initial;
    }

    public float InitialPowerPoints(Data d)
    {
        return  Initial.GetEnumModel(d).Sum(
            v => v.Key.GetPowerPoints() * v.Value);
    }
    public float LostPowerPoints(Data d)
    {
        var lost = 0f;
        foreach (var (troop, value) in Initial.GetEnumModel(d))
        {
            var loss = value - Active.Get(troop);
            lost += loss * troop.GetPowerPoints();
        }

        return lost;
    }
    

    public void AddKill(Troop troop, float amt)
    {
        Kills.Add(troop, amt);
    }

    public void AddLoss(Troop troop, float amt)
    {
        Active.Remove(troop, amt);
        ActiveFrontSize -= troop.FrontLength * amt;
    }

    public IdCount<Troop> GetLosses()
    {
        var losses = IdCount<Troop>.Construct();
        foreach (var (key, value) in Initial.Contents)
        {
            var loss = value - Active.Get(key);
            if (loss != 0f)
            {
                losses.Add(key, loss);
            }
        }

        return losses;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class UnitCombatInfo
{
    public ERef<Unit> Unit { get; private set; }
    public ERef<UnitTemplate> Template { get; private set; }
    public IdCount<Troop> Active { get; private set; }
    public IdCount<Troop> Initial { get; private set; }
    public IdCount<Troop> Kills { get; private set; }
    public float ActiveFrontSize { get; private set; }

    public static UnitCombatInfo Sum(IEnumerable<UnitCombatInfo> infos,
        Data d)
    {
        var active = IdCount<Troop>.Sum(infos.Select(i => i.Active).ToArray());
        var initial = IdCount<Troop>.Sum(infos.Select(i => i.Initial).ToArray());
        var kills = IdCount<Troop>.Sum(infos.Select(i => i.Kills).ToArray());
        var activeFrontSize = active.GetEnumModel(d)
            .Sum(v => v.Key.FrontLength * v.Value);
        return new UnitCombatInfo(new ERef<Unit>(-1),
            active, initial, kills, new ERef<UnitTemplate>(-1),
            activeFrontSize);
    }
    public UnitCombatInfo(Unit u, Data d)
    {
        Unit = u.MakeRef();
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
        Unit = new ERef<Unit>(-1);
        Active = IdCount<Troop>.Construct(troops);
        Initial = IdCount<Troop>.Construct(troops);
        Kills = IdCount<Troop>.Construct();
        Template = new ERef<UnitTemplate>(-1);
        ActiveFrontSize = Active.GetEnumModel(d)
            .Sum(v => v.Key.FrontLength * v.Value);
    }
    [SerializationConstructor] public UnitCombatInfo(
        ERef<Unit> unit, IdCount<Troop> active, 
        IdCount<Troop> initial, 
        IdCount<Troop> kills,
        ERef<UnitTemplate> template,
        float activeFrontSize)
    {
        Unit = unit;
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

    public void ClearLossesKills()
    {
        Active.Clear();
        Kills.Clear();
        foreach (var (key, value) in Initial.Contents)
        {
            Active.Set(key, value);
        }
    }

    public void SetInitial(Troop troop, float amt, Data d)
    {
        Initial.Set(troop, amt);
        ClearLossesKills();
        ActiveFrontSize = Active.GetEnumModel(d)
            .Sum(v => v.Key.FrontLength * v.Value);
    }

    public float GetEchelonFrontage(int echelon, Data d)
    {
        var res = 0f;
        foreach (var (troop, amt) in Active.GetEnumModel(d))
        {
            if (troop.Echelon != echelon) continue;
            res += troop.FrontLength * amt;
        }
        return res;
    }

    public int GetMinEchelon(Data d)
    {
        var actives = Active.GetEnumModel(d)
            .Where(kvp => kvp.Value > 0f);
        if (actives.Any())
        {
            return actives.Min(kvp => kvp.Key.Echelon);
        }
        return -1;
    }
    public int GetMaxEchelon(Data d)
    {
        var actives = Active.GetEnumModel(d)
            .Where(kvp => kvp.Value > 0f);
        if (actives.Any())
        {
            return actives.Max(kvp => kvp.Key.Echelon);
        }

        throw new Exception();
    }
}
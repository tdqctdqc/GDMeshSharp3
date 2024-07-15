
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
    public float[] ActiveFrontSizes { get; private set; }

    public static UnitCombatInfo Sum(IEnumerable<UnitCombatInfo> infos,
        Data d)
    {
        var active = IdCount<Troop>.Sum(infos.Select(i => i.Active).ToArray());
        var initial = IdCount<Troop>.Sum(infos.Select(i => i.Initial).ToArray());
        var kills = IdCount<Troop>.Sum(infos.Select(i => i.Kills).ToArray());

        var activeFrontSizes = SetFrontSizes(active, d);
        
        return new UnitCombatInfo(new ERef<Unit>(-1),
            active, initial, kills, new ERef<UnitTemplate>(-1),
            activeFrontSizes);
    }
    public UnitCombatInfo(Unit u, Data d)
    {
        Unit = u.MakeRef();
        Active = IdCount<Troop>.Construct(u.Troops);
        Initial = IdCount<Troop>.Construct(u.Troops);
        Kills = IdCount<Troop>.Construct();
        Template = u.Template;

        ActiveFrontSizes = SetFrontSizes(u.Troops, d);
    }
    public UnitCombatInfo(IdCount<Troop> troops,
        Data d)
    {
        Unit = new ERef<Unit>(-1);
        Active = IdCount<Troop>.Construct(troops);
        Initial = IdCount<Troop>.Construct(troops);
        Kills = IdCount<Troop>.Construct();
        Template = new ERef<UnitTemplate>(-1);
        ActiveFrontSizes = SetFrontSizes(troops, d);
    }

    private static float[] SetFrontSizes(IdCount<Troop> troops, Data d)
    {
        var activeFrontSizes = new float[MilUtil.NumEchelons];
        foreach (var (key, value) in troops.GetEnumModel(d))
        {
            var echelon = key.Echelon;
            activeFrontSizes[echelon] += key.FrontLength * value;
        }

        return activeFrontSizes;
    }

    [SerializationConstructor] public UnitCombatInfo(
        ERef<Unit> unit, IdCount<Troop> active, 
        IdCount<Troop> initial, 
        IdCount<Troop> kills,
        ERef<UnitTemplate> template,
        float[] activeFrontSizes)
    {
        Unit = unit;
        Active = active;
        Initial = initial;
        Kills = kills;
        ActiveFrontSizes = activeFrontSizes;
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
        ActiveFrontSizes[troop.Echelon] -= troop.FrontLength * amt;
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

    public void ClearLossesKills(Data data)
    {
        Active.Clear();
        Kills.Clear();
        foreach (var (key, value) in Initial.Contents)
        {
            Active.Set(key, value);
        }

        ActiveFrontSizes = SetFrontSizes(Active, data);
    }

    public void SetInitial(Troop troop, float amt, Data d)
    {
        Initial.Set(troop, amt);
        ClearLossesKills(d);
    }


    public int GetMinEchelon(Data d)
    {
        for (var i = 0; i < ActiveFrontSizes.Length; i++)
        {
            if (ActiveFrontSizes[i] > 0f)
            {
                return i;
            }
        }

        return -1;
    }
    public int GetMaxEchelon(Data d)
    {
        for (var i = ActiveFrontSizes.Length - 1;
             i >= 0; i++)
        {
            if (ActiveFrontSizes[i] > 0f)
            {
                return i;
            }
        }

        return -1;
    }
}
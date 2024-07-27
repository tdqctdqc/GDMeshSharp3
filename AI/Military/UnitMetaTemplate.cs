using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public class UnitMetaTemplate 
{
    public string Name { get; private set; }
    public (List<(TroopType, float)>, float)[] Weights { get; private set; }
    public ERef<UnitTemplate> Current { get; private set; }
    public HashSet<ERef<UnitTemplate>> Obsolete { get; private set; }
    public UnitMetaTemplate(string name,
        (List<(TroopType, float)>, float)[] weights)
    {
        Name = name;
        Weights = weights;
        Current = new ERef<UnitTemplate>();
        Obsolete = new HashSet<ERef<UnitTemplate>>();
    }

    public void Check(Regime r, LogicWriteKey key)
    {
        if (Current.IsEmpty() || IsObsolete(Current.Get(key.Data), r, key.Data))
        {
            var template = UnitTemplate.Create(key, $"{Name} Division",
                Fill(r, key.Data),
                key.Data.Models.TroopDomains.Land,
                r);
            if (Current.Fulfilled())
            {
                Obsolete.Add(Current);
            }
            Current = template.MakeRef();
        }
    }
    
    private IdCount<TroopType> Fill(Regime regime,
        Data d)
    {
        if (Weights.Length != MilUtil.NumEchelons) throw new Exception();
        var availTroops = d.Models.GetModels<Troop>()
            .Where(t => regime.HasPrereqs(t)).ToHashSet();
        
        var res = IdCount<TroopType>.Construct();
        for (var i = 0; i < Weights.Length; i++)
        {
            var echelonWeights = Weights[i].Item1;
            var echelonFill = Weights[i].Item2;
            if (echelonFill == 0f) continue;

            if (echelonWeights.Any(e => e.Item1.Echelon != i))
            {
                throw new Exception();
            }

            var avail = echelonWeights
                .Where(v => availTroops.Any(t => t.TroopType == v.Item1))
                .ToArray();
            if (avail.Length == 0) continue;

            var totalWeight = avail.Sum(v => v.Item2);
            foreach (var (troopType, weight) in avail)
            {
                var proportion = weight / totalWeight;
                var num = Mathf.CeilToInt(
                    proportion * UnitTemplatesAi.SingleUnitFrontProportion 
                               * echelonFill
                               * MilUtil.BaseFrontLength / troopType.FrontLength);
                res.Add(troopType, num);
            }
        }
        
        return res;
    }

    private bool IsObsolete(UnitTemplate t, Regime r, Data d)
    {
        return false;
    }

    public float GetDistance(UnitTemplate t, Data d)
    {
        var test = Fill(t.Regime.Get(d), d);
        return t.Troops.GetEnumModel(d)
            .Sum(kvp =>
            {
                var type = kvp.Key;
                var amt = kvp.Value;
                var thisAmt = test.Get(type);
                return Mathf.Abs(thisAmt - amt);
            });
    }
    
    
    public static (List<(TroopType, float)>, float)[] GetInfantryTemplateWeights(Data d)
    {
        return new (List<(TroopType, float)>, float)[]
        {
            (new()
            {
                (d.Models.TroopTypes.Infantry, 1f)
            }, 1f),
            (new()
            {
                (d.Models.TroopTypes.MachineGun, 1f)
            }, .5f),
            (new()
            {
                (d.Models.TroopTypes.Artillery, 1f)
            }, .25f),
            (new()
            {

            }, 0f)
        };
    }
    
    
}
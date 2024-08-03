using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using EchelonWeights = (System.Collections.Generic.List<(ModelRef<TroopType>, float)>, float);


public class UnitMetaTemplate 
{
    public string Name { get; private set; }
    public UnitTemplatesAi.UnitTypeTag Tag { get; private set; }
    public EchelonWeights[] Weights { get; private set; }
    public ERef<UnitTemplate> Current { get; private set; }
    public HashSet<ERef<UnitTemplate>> Obsolete { get; private set; }
    public UnitMetaTemplate(string name,
        EchelonWeights[] weights,
        ERef<UnitTemplate> current, 
        HashSet<ERef<UnitTemplate>> obsolete,
        UnitTemplatesAi.UnitTypeTag tag)
    {
        Tag = tag;
        Name = name;
        Weights = weights;
        Current = current;
        Obsolete = obsolete;
    }

    public void Check(Regime r, LogicKey key)
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
            .Where(t => regime.HasPrereqs(t))
            .ToHashSet();
        
        var res = IdCount<TroopType>.Construct();
        for (var i = 0; i < Weights.Length; i++)
        {
            var echelonWeights = Weights[i].Item1;
            var echelonFill = Weights[i].Item2;
            if (echelonFill == 0f) continue;

            if (echelonWeights.Any(e => e.Item1.Get(d).Echelon != i))
            {
                throw new Exception();
            }

            var avail = echelonWeights
                .Where(v => availTroops.Any(t => t.TroopType.Id == v.Item1.RefId))
                .ToArray();
            if (avail.Length == 0) continue;

            var totalWeight = avail.Sum(v => v.Item2);
            foreach (var (troopTypeRef, weight) in avail)
            {
                var troopType = troopTypeRef.Get(d);
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
    
    
    public static EchelonWeights[] GetInfantryTemplateWeights(Data d)
    {
        return new EchelonWeights[]
        {
            (new()
            {
                (d.Models.TroopTypes.Infantry.MakeRef(), 1f)
            }, 1f),
            (new()
            {
                (d.Models.TroopTypes.MachineGun.MakeRef(), 1f)
            }, .5f),
            (new()
            {
                (d.Models.TroopTypes.Artillery.MakeRef(), 1f)
            }, .25f),
            (new()
            {

            }, 0f)
        };
    }
    
    
}

using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using MessagePack;

public class UnitTemplatesAi
{
    public enum UnitTypeTag
    {
        Infantry
    }
    public Dictionary<UnitTypeTag, UnitMetaTemplate> MetaTemplates { get; private set; }
    public ERef<Regime> Regime { get; private set; }
    public static float SingleUnitFrontProportion { get; private set; }
        = .35f;

    public static UnitTemplatesAi Construct(Regime r, Data d)
    {
        var ai = new UnitTemplatesAi(r.MakeRef(),
            new Dictionary<UnitTypeTag, UnitMetaTemplate>());

        var infantry = new UnitMetaTemplate(
            nameof(UnitTypeTag.Infantry),
            UnitMetaTemplate.GetInfantryTemplateWeights(d),
            new ERef<UnitTemplate>(),
            new HashSet<ERef<UnitTemplate>>(),
            UnitTypeTag.Infantry);

        ai.MetaTemplates.Add(UnitTypeTag.Infantry, infantry);
        
        return ai;
    }
    [SerializationConstructor] private UnitTemplatesAi(ERef<Regime> regime, 
        Dictionary<UnitTypeTag, UnitMetaTemplate> metaTemplates)
    {
        Regime = regime;
        MetaTemplates = metaTemplates;
    }

    public void Calculate(TimerTreeNode timer, LogicKey key)
    {
        HandleUnassociatedTemplates(key);
        CheckTemplates(key);
        UpgradeTemplates(key);
    }
    private void HandleUnassociatedTemplates(LogicKey key)
    {
        var allTemplates = Regime.Get(key.Data)
            .GetUnitTemplates(key.Data);
        if (allTemplates is null 
            || allTemplates.Count() == 0) return;
        var categorized = MetaTemplates
            .Values
            .SelectMany(t => t.Obsolete)
            .Concat(MetaTemplates.Values.Where(t => t.Current.Fulfilled()).Select(t => t.Current))
            .Select(t => t.Get(key.Data))
            .ToHashSet();
        
        var uncategorized = allTemplates
            .Except(categorized)
            .ToArray();
        foreach (var unitTemplate in uncategorized)
        {
            CategorizeTemplate(unitTemplate, key.Data);
        }
    }

    public UnitMetaTemplate CategorizeTemplate(UnitTemplate unitTemplate,
        Data d)
    {
        var min = MetaTemplates
            .Values
            .MinBy(m => m.GetDistance(unitTemplate, d));
        min.Obsolete.Add(unitTemplate.MakeRef());

        return min;
    }
    private void CheckTemplates(LogicKey key)
    {
        foreach (var mt in 
                 MetaTemplates.Values)
        {
            mt.Check(Regime.Get(key.Data), key);
        }
    }

    private void UpgradeTemplates(LogicKey key)
    {
        
    }
}
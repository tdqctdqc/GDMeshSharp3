
using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

public class UnitTemplatesAi
{
    public UnitMetaTemplate Infantry { get; private set; }
    public List<UnitMetaTemplate> MetaTemplates { get; private set; }
    private Regime _regime;
    public static float SingleUnitFrontProportion { get; private set; }
        = .35f;
    
    public UnitTemplatesAi(Regime regime, Data d)
    {
        _regime = regime;
        
        Infantry = new UnitMetaTemplate(nameof(Infantry),
            UnitMetaTemplate.GetInfantryTemplateWeights(d));

        MetaTemplates = new List<UnitMetaTemplate>
        {
            Infantry
        };
    }

    public void Calculate(LogicKey key)
    {
        HandleUnassociatedTemplates(key);
        CheckTemplates(key);
        UpgradeTemplates(key);
    }
    private void HandleUnassociatedTemplates(LogicKey key)
    {
        var allTemplates = _regime
            .GetUnitTemplates(key.Data);
        if (allTemplates is null 
            || allTemplates.Count() == 0) return;
        var categorized = MetaTemplates.SelectMany(t => t.Obsolete)
            .Concat(MetaTemplates.Where(t => t.Current.Fulfilled()).Select(t => t.Current))
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
        var min = MetaTemplates.MinBy(m => m.GetDistance(unitTemplate, d));
        min.Obsolete.Add(unitTemplate.MakeRef());

        return min;
    }
    private void CheckTemplates(LogicKey key)
    {
        foreach (var mt in MetaTemplates)
        {
            mt.Check(_regime, key);
        }
    }

    private void UpgradeTemplates(LogicKey key)
    {
        
    }
}
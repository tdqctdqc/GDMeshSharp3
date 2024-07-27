
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

    public void Calculate(LogicWriteKey key)
    {
        HandleUnassociatedTemplates(key);
        CheckTemplates(key);
        UpgradeTemplates(key);
    }
    private void HandleUnassociatedTemplates(LogicWriteKey key)
    {
        var categorized = MetaTemplates.SelectMany(t => t.Obsolete)
            .Concat(MetaTemplates.Where(t => t.Current.Fulfilled()).Select(t => t.Current))
            .Select(t => t.Get(key.Data))
            .ToHashSet();
        var uncategorized = _regime
            .GetUnitTemplates(key.Data)
            .Where(t => categorized.Contains(t) == false)
            .ToArray();
        foreach (var unitTemplate in uncategorized)
        {
            var min = MetaTemplates.MinBy(m => m.GetDistance(unitTemplate, key.Data));
            min.Obsolete.Add(unitTemplate.MakeRef());
        }
    }
    private void CheckTemplates(LogicWriteKey key)
    {
        foreach (var mt in MetaTemplates)
        {
            mt.Check(_regime, key);
        }
    }

    private void UpgradeTemplates(LogicWriteKey key)
    {
        
    }
}
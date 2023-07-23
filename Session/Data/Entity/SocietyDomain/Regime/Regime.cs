using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Regime : Entity
{
    public EntityRef<MapPolygon> Capital { get; protected set; }
    public ModelRef<Culture> Culture { get; private set; }
    public ModelRef<RegimeTemplate> Template { get; private set; }
    public Color PrimaryColor { get; protected set; }
    public Color SecondaryColor { get; protected set; }
    public ItemCount Items { get; protected set; }
    // public FlowCount Flows { get; private set; }
    public RegimeFlows Flows { get; private set; }
    public RegimeHistory History { get; private set; }
    public string Name { get; protected set; }
    public EntityRefCollection<MapPolygon> Polygons { get; protected set; }
    public RegimeFinance Finance { get; private set; }
    public bool IsMajor { get; private set; }

    [SerializationConstructor] private Regime(int id, string name, Color primaryColor, Color secondaryColor, 
        EntityRefCollection<MapPolygon> polygons, EntityRef<MapPolygon> capital,
        ItemCount items, RegimeHistory history, ModelRef<Culture> culture,
        ModelRef<RegimeTemplate> template, RegimeFinance finance, bool isMajor, 
        RegimeFlows flows) : base(id)
    {
        Items = items;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        Polygons = polygons;
        Name = name;
        Capital = capital;
        History = history;
        Culture = culture;
        Template = template;
        Finance = finance;
        IsMajor = isMajor;
        Flows = flows;
    }

    public static Regime Create(MapPolygon seed, RegimeTemplate regimeTemplate, bool isMajor, CreateWriteKey key)
    {
        var polygons = EntityRefCollection<MapPolygon>.Construct(nameof(Polygons), 
            -1, new HashSet<int>{seed.Id}, key.Data);
        var items = ItemCount.Construct();
        var flows = new RegimeFlows(new Dictionary<int, FlowData>());
        flows.AddFlowIn(FlowManager.Income, 0f);
        flows.AddFlowIn(FlowManager.ConstructionCap, 0f);
        flows.AddFlowIn(FlowManager.IndustrialPower, 0f);
        
        var r = new Regime(-1, regimeTemplate.Name, 
            new Color(regimeTemplate.PrimaryColor), 
            new Color(regimeTemplate.SecondaryColor), 
            polygons, new EntityRef<MapPolygon>(seed.Id),
            items,
            RegimeHistory.Construct(key.Data), 
            regimeTemplate.Culture.MakeRef(),
            regimeTemplate.MakeRef(),
            RegimeFinance.Construct(),
            isMajor,
            flows
        );
        key.Create(r);
        seed.SetRegime(r, key);
        Alliance.Create(r, key);
        return r;
    }

    public void SetIsMajor(bool isMajor, CreateWriteKey key)
    {
        IsMajor = isMajor;
    }

    public void SetFlows(RegimeFlows flows, ProcedureWriteKey key)
    {
        Flows = flows;
    }
    public override string ToString() => Name;
}
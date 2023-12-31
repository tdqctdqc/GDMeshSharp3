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
    public IdCount<Item> Items { get; protected set; }
    public RegimeFlows Flows { get; private set; }
    public RegimeHistory History { get; private set; }
    public string Name { get; protected set; }
    public RegimeFinance Finance { get; private set; }
    public bool IsMajor { get; private set; }
    public ManufacturingQueue ManufacturingQueue { get; private set; }
    public RegimeMilitary Military { get; private set; }

    [SerializationConstructor] private Regime(int id, string name, 
        Color primaryColor, Color secondaryColor, 
        EntityRef<MapPolygon> capital,
        IdCount<Item> items, RegimeHistory history, ModelRef<Culture> culture,
        ModelRef<RegimeTemplate> template, RegimeFinance finance, bool isMajor, 
        RegimeFlows flows, ManufacturingQueue manufacturingQueue,
        RegimeMilitary military) : base(id)
    {
        Items = items;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        Name = name;
        Capital = capital;
        History = history;
        Culture = culture;
        Template = template;
        Finance = finance;
        IsMajor = isMajor;
        Flows = flows;
        ManufacturingQueue = manufacturingQueue;
        Military = military;
    }

    public static Regime Create(MapPolygon seed, 
        RegimeTemplate regimeTemplate, bool isMajor, 
        ICreateWriteKey key)
    {
        var items = IdCount<Item>.Construct();
        var flows = new RegimeFlows(new Dictionary<int, FlowData>());
        flows.AddFlowIn(key.Data.Models.Flows.Income, 0f);
        flows.AddFlowIn(key.Data.Models.Flows.ConstructionCap, 0f);
        flows.AddFlowIn(key.Data.Models.Flows.IndustrialPower, 0f);
        var id = key.Data.IdDispenser.TakeId();
        var r = new Regime(id, regimeTemplate.Name, 
            new Color(regimeTemplate.PrimaryColor), 
            new Color(regimeTemplate.SecondaryColor), 
            new EntityRef<MapPolygon>(seed.Id),
            items,
            RegimeHistory.Construct(key.Data), 
            regimeTemplate.Culture.MakeRef(),
            regimeTemplate.MakeRef(),
            RegimeFinance.Construct(),
            isMajor,
            flows,
            ManufacturingQueue.Construct(),
            RegimeMilitary.Construct(id, key.Data)
        );
        key.Create(r);
        Alliance.Create(r, key);
        UnitTemplate.CreateDefaultTemplatesForRegime(r, key);
        return r;
    }

    public void SetIsMajor(bool isMajor, ICreateWriteKey key)
    {
        IsMajor = isMajor;
    }

    public void SetFlows(RegimeFlows flows, ProcedureWriteKey key)
    {
        Flows = flows;
    }
    public override string ToString() => Name;
}
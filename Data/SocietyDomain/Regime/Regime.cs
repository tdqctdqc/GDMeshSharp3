using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Regime : Entity
{
    public ERef<MapPolygon> Capital { get; protected set; }
    public ModelRef<Culture> Culture { get; private set; }
    public ModelRef<RegimeTemplate> Template { get; private set; }
    public Color PrimaryColor { get; protected set; }
    public Color SecondaryColor { get; protected set; }
    public IdCount<IModel> Store { get; protected set; }
    public RegimeHistory History { get; private set; }
    public string Name { get; protected set; }
    public RegimeFinance Finance { get; private set; }
    public bool IsMajor { get; private set; }
    public MakeQueue MakeQueue { get; private set; }
    public RegimeMilitary Military { get; private set; }

    [SerializationConstructor] private Regime(int id, string name, 
        Color primaryColor, Color secondaryColor, 
        ERef<MapPolygon> capital,
        IdCount<IModel> store, RegimeHistory history, ModelRef<Culture> culture,
        ModelRef<RegimeTemplate> template, RegimeFinance finance, bool isMajor, 
        MakeQueue makeQueue,
        RegimeMilitary military) : base(id)
    {
        Store = store;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        Name = name;
        Capital = capital;
        History = history;
        Culture = culture;
        Template = template;
        Finance = finance;
        IsMajor = isMajor;
        MakeQueue = makeQueue;
        Military = military;
    }

    public static Regime Create(MapPolygon seed, 
        RegimeTemplate regimeTemplate, bool isMajor, 
        ICreateWriteKey key)
    {
        var store = IdCount<IModel>.Construct();
        var id = key.Data.IdDispenser.TakeId();
        var r = new Regime(id, regimeTemplate.Name, 
            new Color(regimeTemplate.PrimaryColor), 
            new Color(regimeTemplate.SecondaryColor), 
            new ERef<MapPolygon>(seed.Id),
            store,
            RegimeHistory.Construct(key.Data), 
            regimeTemplate.Culture.MakeRef(),
            regimeTemplate.MakeRef(),
            RegimeFinance.Construct(),
            isMajor,
            MakeQueue.Construct(),
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

    public override void CleanUp(StrongWriteKey key)
    {
        var alliance = this.GetAlliance(key.Data);
        alliance.Members.Remove(this, key);
        if (alliance.Members.Count() == 0)
        {
            key.Data.RemoveEntity(alliance.Id, key);
        }
    }
}
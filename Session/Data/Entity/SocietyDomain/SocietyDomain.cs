using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SocietyDomain : Domain
{
    public Market Market => _market != null ? _market.Value : null;
    private SingletonAux<Market> _market;
    public RegimeAux RegimeAux { get; private set; }
    public PolyPeepAux PolyPeepAux { get; private set; }
    public AllianceAux AllianceAux { get; private set; }
    
    public SocietyDomain(Data data) : base(typeof(SocietyDomain), data)
    {
        
    }
    public override void Setup()
    {
        RegimeAux = new RegimeAux(Data);
        PolyPeepAux = new PolyPeepAux(Data);
        AllianceAux = new AllianceAux(Data);
        _market = new SingletonAux<Market>(Data);
    }
}
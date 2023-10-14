using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SocietyDomain
{
    public Market Market => _market != null ? _market.Value : null;
    private SingletonAux<Market> _market;
    
    public ProposalList Proposals => _proposals != null ? _proposals.Value : null;
    private SingletonAux<ProposalList> _proposals;
    public RegimeAux RegimeAux { get; private set; }
    public PolyPeepAux PolyPeepAux { get; private set; }
    public AllianceAux AllianceAux { get; private set; }
    
    public SocietyDomain()
    {
        
    }
    public void Setup(Data data)
    {
        RegimeAux = new RegimeAux(data);
        PolyPeepAux = new PolyPeepAux(data);
        AllianceAux = new AllianceAux(data);
        _market = new SingletonAux<Market>(data);
        _proposals = new SingletonAux<ProposalList>(data);
    }
}
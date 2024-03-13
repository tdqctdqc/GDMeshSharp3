using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SocietyDomain
{
    public Market Market => _market != null ? _market.Value : null;
    private SingletonCache<Market> _market;
    public ProposalList Proposals => _proposals != null ? _proposals.Value : null;
    private SingletonCache<ProposalList> _proposals;
    public RegimeAux RegimeAux { get; private set; }
    public PolyPeepAux PolyPeepAux { get; private set; }
    public AllianceAux AllianceAux { get; private set; }
    public DiplomacyGraph DiploGraph => _diploGraph.Value;
    private SingletonCache<DiplomacyGraph> _diploGraph;
    public SocietyDomain()
    {
        
    }
    public void Setup(Data data)
    {
        RegimeAux = new RegimeAux(data);
        PolyPeepAux = new PolyPeepAux(data);
        AllianceAux = new AllianceAux(data);
        _diploGraph = new SingletonCache<DiplomacyGraph>(data);
        _market = new SingletonCache<Market>(data);
        _proposals = new SingletonCache<ProposalList>(data);
    }
}
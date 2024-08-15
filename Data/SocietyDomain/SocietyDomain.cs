using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SocietyDomain
{
    public RegimeAux RegimeAux { get; private set; }
    public PolyPeepAux PolyPeepAux { get; private set; }
    public DiplomacyGraph DiploGraph => _diploGraph.Value;
    private SingletonCache<DiplomacyGraph> _diploGraph;
    public SocietyDomain()
    {
        
    }
    public void Setup(Data data)
    {
        RegimeAux = new RegimeAux(data);
        PolyPeepAux = new PolyPeepAux(data);
        _diploGraph = new SingletonCache<DiplomacyGraph>(data);
    }
}
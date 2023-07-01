
using System;

public class RegimeRelationAux : EntityAux<RegimeRelation>
{
    public EdgeAux<RegimeRelation, int> ByRegime { get; private set; }
    public RegimeRelationAux(Domain domain, Data data) : base(domain, data)
    {
        ByRegime = new EdgeAux<RegimeRelation, int>(data,
            r => r, rr => new Tuple<int, int>(rr.HighId.RefId, rr.LowId.RefId));
    }
}

using System.Collections.Generic;
using System.Linq;
using GDMeshSharp3.Utility;
using MessagePack;

public class DiplomacyGraph : Entity
{
    public ConcurrentIdMultiEdgeGraph<Regime, DiploRelation> Graph { get; private set; }

    public static DiplomacyGraph Create(GenKey key)
    {
        var g = new DiplomacyGraph(ConcurrentIdMultiEdgeGraph<Regime, DiploRelation>.Construct(),
            key.Data.IdDispenser.TakeId());
        key.Create(g);
        return g;
    }
    [SerializationConstructor] private DiplomacyGraph(
        ConcurrentIdMultiEdgeGraph<Regime, DiploRelation> graph,
        int id) : base(id)
    {
        Graph = graph;
    }
    
    public void AddEdge(Regime r1, Regime r2, 
        DiploRelation edge, IWriteKey key)
    {
        Graph.AddToEdge(r1, r2, edge);
    }

    public bool HasRelation(Regime a1, Regime a2, DiploRelation edge)
    {
        return Graph.TryGetEdges(a1, a2, out var edges)
            && edges.Any(e => e.Key == edge);
    }

    public IEnumerable<Regime> GetRelations(Regime a, DiploRelation edge, Data d)
    {
        return Graph.GetNeighborsWith(a, e => e == edge)
            .Select(n => d.Get<Regime>(n));
    }
    public void RemoveRegime(Regime a, IWriteKey key)
    {
        Graph.Remove(a);
    }
    public override void CleanUp(IWriteKey key)
    {
        
    }
}
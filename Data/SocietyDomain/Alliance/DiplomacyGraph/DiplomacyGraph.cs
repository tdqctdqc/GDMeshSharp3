using System.Collections.Generic;
using System.Linq;
using GDMeshSharp3.Utility;
using MessagePack;

public class DiplomacyGraph : Entity
{
    public ConcurrentIdMultiEdgeGraph<Alliance, DiploRelation> Graph { get; private set; }

    public static DiplomacyGraph Create(GenWriteKey key)
    {
        var g = new DiplomacyGraph(ConcurrentIdMultiEdgeGraph<Alliance, DiploRelation>.Construct(),
            key.Data.IdDispenser.TakeId());
        key.Create(g);
        return g;
    }
    [SerializationConstructor] private DiplomacyGraph(
        ConcurrentIdMultiEdgeGraph<Alliance, DiploRelation> graph,
        int id) : base(id)
    {
        Graph = graph;
    }
    
    public void AddEdge(Alliance a1, Alliance a2, 
        DiploRelation edge, StrongWriteKey key)
    {
        Graph.AddToEdge(a1, a2, edge);
    }

    public bool HasRelation(Alliance a1, Alliance a2, DiploRelation edge)
    {
        return Graph.TryGetEdges(a1, a2, out var edges)
            && edges.Any(e => e.Key == edge);
    }

    public IEnumerable<Alliance> GetRelations(Alliance a, DiploRelation edge, Data d)
    {
        return Graph.GetNeighborsWith(a, e => e == edge)
            .Select(n => d.Get<Alliance>(n));
    }

    public void MergeRelations(Alliance dissolve, Alliance into, StrongWriteKey key)
    {
        Graph.DoForEdges(dissolve, (n, r) =>
        {
            var other = key.Data.Get<Alliance>(n);
            Graph.AddToEdge(into, other, r);
        });
    }
    public void RemoveAlliance(Alliance a, StrongWriteKey key)
    {
        Graph.Remove(a);
    }
    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}
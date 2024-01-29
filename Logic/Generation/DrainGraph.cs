using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;
using Priority_Queue;

public class DrainGraph<T>
{
    private HashSet<T> _elements, _sources, _sinks;
    private Func<T, float> _getSupply;
    private Func<T, T, float> _getCost; 
    private Func<T, IEnumerable<T>> _getNeighbors;
    private Dictionary<T, DrainGraphNode<T>> _nodes;

    public static Graph<T, float> GetDrainGraph(IEnumerable<T> sources, IEnumerable<T> sinks, 
        Func<T, float> getSupply, Func<T, T, float> getCost, float maxCost, 
        Func<T, IEnumerable<T>> getNeighbors)
    {
        var drainGraph = new DrainGraph<T>(sources, sinks, getSupply, getCost, maxCost, getNeighbors);
        return drainGraph.MakeGraph();
    }

    public DrainGraph(IEnumerable<T> sources, IEnumerable<T> sinks, Func<T, float> getSupply, Func<T, T, float> getCost, float maxCost, Func<T, IEnumerable<T>> getNeighbors)
    {
        _sources = sources.ToHashSet();
        _sinks = sinks.ToHashSet();
        _elements = new HashSet<T>(_sources);
        _elements.UnionWith(_sinks);
        _getSupply = getSupply;
        _getCost = getCost;
        _getNeighbors = (t) => getNeighbors(t).Where(n => _elements.Contains(n) && getCost(t,n) <= maxCost);
    }

    private Graph<T, float> MakeGraph()
    {
        var sw = new Stopwatch();
        
        _nodes = new Dictionary<T, DrainGraphNode<T>>();
        
        foreach (var source in _sources)
        {
            _nodes.Add(source, new DrainGraphNode<T>(source));
        }
        
        DoUnion(_sources.Union(_sinks).ToList());

        var graph = new Graph<T, float>();
        foreach (var sink in _sinks)
        {
            graph.AddNode(sink);
        }
        foreach (var source in _sources)
        {
            graph.AddNode(source);
        }
        foreach (var kvp in _nodes)
        {
            graph.AddEdge(kvp.Value.Element, kvp.Value.DrainsTo, 0f);
        }
        
        
        foreach (var kvp in _nodes)
        {
            var curr = kvp.Value.Element;
            var supply = _getSupply(curr);
            var totFlow = supply;
            while (_nodes.TryGetValue(curr, out var node))
            {
                var flow = graph.GetEdge(curr, node.DrainsTo) + supply;
                graph.SetEdgeValue(curr, node.DrainsTo, flow);
                curr = node.DrainsTo;
                totFlow = flow;
            }
        }
        return graph;
    }

    private void DoUnion(List<T> union)
    {
        var queue = new SimplePriorityQueue<T, float>();
        var frontiers = new Dictionary<T, List<T>>();

        IEnumerable<T> getValidNs(T el)
        {
            return _getNeighbors(el)
                .Where(n => _sources.Contains(n))
                .Where(n => _nodes[n].DrainsTo == null 
                            || TotalDrainCost(n) > _getCost(el, n) + TotalDrainCost(el)
                );
        }
        
        foreach (var sink in _sinks)
        {
            frontiers.Add(sink, new List<T>{sink});
            queue.Enqueue(sink, 0f);
        }

        var add = new HashSet<T>();
        while (queue.Count > 0)
        {
            var sink = queue.First;
            var frontier = frontiers[sink];
            if (frontier.Count == 0)
            {
                queue.Remove(sink);
                continue;
            }
            
            var minScore = Mathf.Inf;
            
            for (var i = 0; i < frontier.Count; i++)
            {
                var frontierEl = frontier[i];
                var score = TotalDrainCost(frontierEl);
                var validNs = getValidNs(frontierEl);
                foreach (var validN in validNs)
                {
                    var nNode = _nodes[validN];
                    var cost = _getCost(frontierEl, validN);
                    nNode.DrainsTo = frontierEl;
                    nNode.DrainCost = cost;
                    add.Add(validN);
                    minScore = Mathf.Min(minScore, score + cost);
                }
            }
            frontier.Clear();
            frontier.AddRange(add.OrderBy(t => TotalDrainCost(t)));
            add.Clear();
            queue.UpdatePriority(sink, minScore);
        }
    }

    
    
    private float TotalDrainCost(T curr)
    {
        var res = 0f;
        while (_nodes.ContainsKey(curr))
        {
            var node = _nodes[curr];
            if (node.DrainsTo != null)
            {
                res += _getCost(curr, node.DrainsTo);
                curr = node.DrainsTo;
            }
            else break;
        }

        return res;
    }
    
}
public class DrainGraphNode<T>
{
    public T Element { get; set; }
    public T DrainsTo { get; set; }
    public float DrainCost { get; set; }

    public DrainGraphNode(T element)
    {
        Element = element;
        DrainCost = 0f;
    }
}
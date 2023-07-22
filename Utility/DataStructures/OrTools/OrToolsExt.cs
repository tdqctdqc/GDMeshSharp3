using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools;
using Google.OrTools.Graph;

public class OrToolsExt
{
    // private static Dictionary<int, string> _results = new Dictionary<int, string>()
    // {
    //     {MinCostFlowBase.OPTIMAL, "OPTIMAL"},
    //     {MinCostFlowBase.FEASIBLE, "FEASIBLE"},
    //     {MinCostFlowBase.BAD_RESULT, "BAD_RESULT"},
    //     {MinCostFlowBase.NOT_SOLVED, "NOT_SOLVED"},
    //     {MinCostFlowBase.INFEASIBLE, "INFEASIBLE"},
    //     {MinCostFlowBase.UNBALANCED, "UNBALANCED"},
    //     {MinCostFlowBase.BAD_COST_RANGE, "BAD_COST_RANGE"},
    // };
    
    public static Graph<T, int> SolveMinFlow<T>(List<T> nodes, Func<T, IEnumerable<T>> getNeighbors,
        Func<T, T, int> getCapacity, Func<T, T, int> getCost, Func<T, int> getSupply)
    {
        MinCostFlow minCostFlow = new MinCostFlow();
        var indices = new Dictionary<T, int>();
        for (var i = 0; i < nodes.Count; i++)
        {
            indices.Add(nodes[i], i);
        }
        
        
        var froms = new List<int>();
        var tos = new List<int>();
        var costs = new List<int>();
        var capacities = new List<int>();

        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var ns = getNeighbors(node);
            foreach (var n in ns)
            {
                if (indices.ContainsKey(n) == false) continue;
                froms.Add(i);
                tos.Add(indices[n]);
                costs.Add(Math.Max(0, getCost(node, n)));
                capacities.Add(getCapacity(node, n));
            }
        }

        int[] supplies = nodes.Select(getSupply).ToArray();

        for (int i = 0; i < froms.Count; ++i)
        {
            int arc =
                minCostFlow.AddArcWithCapacityAndUnitCost(froms[i], tos[i], capacities[i], costs[i]);
            if (arc != i)
                throw new Exception("Internal error");
        }

        for (int i = 0; i < supplies.Length; ++i)
        {
            minCostFlow.SetNodeSupply(i, supplies[i]);
        }
        var status = minCostFlow.Solve();
        GD.Print(status);
        
        var graph = new Graph<T, int>();
        nodes.ForEach(n => graph.AddNode(n));
        var arcs = minCostFlow.NumArcs();
        for (int i = 0; i < minCostFlow.NumArcs(); i++)
        {
            var fromIndex = minCostFlow.Tail(i);

            var toIndex = minCostFlow.Head(i);

            var from = nodes[fromIndex];
            var to = nodes[toIndex];
            graph.AddEdge(from, to, 100);
        }

        return graph;
    }
    
    
    
    
    
    
    
    
    
    
    
    
    public static void Test()
    {
        // Instantiate a SimpleMinCostFlow solver.
        MinCostFlow minCostFlow = new MinCostFlow();

        // Define four parallel arrays: sources, destinations, capacities, and unit costs
        // between each pair. For instance, the arc from node 0 to node 1 has a
        // capacity of 15.
        // Problem taken From Taha's 'Introduction to Operations Research',
        // example 6.4-2.
        int[] startNodes = { 0, 0, 1, 1, 1, 2, 2, 3, 4 };
        int[] endNodes = { 1, 2, 2, 3, 4, 3, 4, 4, 2 };
        int[] capacities = { 15, 8, 20, 4, 10, 15, 4, 20, 5 };
        int[] unitCosts = { 4, 4, 2, 2, 6, 1, 3, 2, 3 };

        // Define an array of supplies at each node.
        int[] supplies = { 20, 0, 0, -5, -15 };

        // Add each arc.
        for (int i = 0; i < startNodes.Length; ++i)
        {
            int arc =
                minCostFlow.AddArcWithCapacityAndUnitCost(startNodes[i], endNodes[i], capacities[i], unitCosts[i]);
            if (arc != i)
                throw new Exception("Internal error");
        }

        // Add node supplies.
        for (int i = 0; i < supplies.Length; ++i)
        {
            minCostFlow.SetNodeSupply(i, supplies[i]);
        }
        
        
        
        var status = minCostFlow.Solve();

        // Find the min cost flow.

        // if (status == MinCostFlow.Status.OPTIMAL)
        // {
        //     Console.WriteLine("Minimum cost: " + minCostFlow.OptimalCost());
        //     Console.WriteLine("");
        //     Console.WriteLine(" Edge   Flow / Capacity  Cost");
        //     for (int i = 0; i < minCostFlow.NumArcs(); ++i)
        //     {
        //         long cost = minCostFlow.Flow(i) * minCostFlow.UnitCost(i);
        //         Console.WriteLine(minCostFlow.Tail(i) + " -> " + minCostFlow.Head(i) + "  " +
        //                           string.Format("{0,3}", minCostFlow.Flow(i)) + "  / " +
        //                           string.Format("{0,3}", minCostFlow.Capacity(i)) + "       " +
        //                           string.Format("{0,3}", cost));
        //     }
        // }
        // else
        // {
        //     Console.WriteLine("Solving the min cost flow problem failed. Solver status: " + status);
        // }
    }
}

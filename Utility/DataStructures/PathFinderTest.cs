
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Dijkstra.NET;
using Dijkstra.NET.ShortestPath;

public static class PathFinderTest
{
    public static void Test()
    {
        var x = 100;
        var y = 100;
        var dic = new Dictionary<Vector2I, Token>();
        var graph = MakeGraph(x, y, dic);
        var testCount = 100;
        var dijkstraSw = new Stopwatch();
        var multiSw = new Stopwatch();
        var singleSw = new Stopwatch();
        for (var i = 0; i < testCount; i++)
        {
            var destKey = new Vector2I(Game.I.Random.RandiRange(0, x - 1),
                Game.I.Random.RandiRange(0, y - 1));

            var startKeys = new HashSet<Vector2I>();
            var numStarts = 20;
            while (startKeys.Count < numStarts)
            {
                var key = -Vector2I.One;
                while (key == -Vector2I.One 
                       || key == destKey
                       || startKeys.Contains(key))
                {
                    key = new Vector2I(Game.I.Random.RandiRange(0, x - 1),
                        Game.I.Random.RandiRange(0, y - 1));
                }
                startKeys.Add(key);
            }
            
            var destToken = dic[destKey];
            
            dijkstraSw.Start();
            foreach (var startKey in startKeys)
            {
                var startToken = dic[startKey];
                var path = graph.GetPath(startToken, destToken);
            }
            dijkstraSw.Stop();
            
            multiSw.Start();
            var paths = PathFinder<Token>
                .FindMultiplePaths(destToken,
                    startKeys.Select(k => dic[k]).ToHashSet(),
                    t => t.Neighbors.Keys,
                    (t, r) => t.Neighbors[r],
                    (t, r) => ((Vector2)t.Pos).DistanceTo((Vector2)r.Pos));
            
            multiSw.Stop();
            singleSw.Start();
            foreach (var startKey in startKeys)
            {
                var startToken = dic[startKey];
                var path = PathFinder<Token>
                    .FindPath(startToken, destToken,
                        t => t.Neighbors.Keys,
                        (t, r) => t.Neighbors[r],
                        (t, r) => ((Vector2)t.Pos).DistanceTo((Vector2)r.Pos));
                var multiPath = paths[startToken];
                
                var iter = 0;
                while (iter < multiPath.Count() && iter < path.Count)
                {
                    if (path[iter] != multiPath[iter])
                    {
                        break;
                    }
                    iter++;
                }
            }
            singleSw.Stop();
        }
        GD.Print("dijkstra path find test " + dijkstraSw.Elapsed.TotalMilliseconds);
        GD.Print("multi path find test " + multiSw.Elapsed.TotalMilliseconds);
        GD.Print("single path find test " + singleSw.Elapsed.TotalMilliseconds);
    }

    private class DijkstraGraph<TNode, TEdge>
        where TEdge : IEquatable<TEdge>
    {
        public Dijkstra.NET.Graph.Graph<TNode, TEdge> Graph;
        public Bijection<TNode, uint> Ids;

        public DijkstraGraph()
        {
            Graph = new Dijkstra.NET.Graph.Graph<TNode, TEdge>();
            Ids = new Bijection<TNode, uint>();
        }

        public void AddNode(TNode t)
        {
            var id = Graph.AddNode(t);
            Ids.Add(t, id);
        }

        public void Connect(TNode t, TNode r, TEdge edge, int cost)
        {
            var k1 = Ids[t];
            var k2 = Ids[r];
            Graph.Connect(k1, k2, cost, edge);
        }
        public List<TNode> GetPath(TNode t, TNode r)
        {
            var k1 = Ids[t];
            var k2 = Ids[r];
            var s = Graph.Dijkstra(k1, k2);
            var path = s.GetPath();
            return path.Select(id => Ids[id]).ToList();
        }
    }
    private static DijkstraGraph<Token, float> MakeGraph(int x, int y, 
        Dictionary<Vector2I, Token> dic)
    {
        var graph = new DijkstraGraph<Token, float>();
        for (var i = 0; i < x; i++)
        {
            for (var j = 0; j < y; j++)
            {
                var pos = new Vector2I(i, j);
                var token = new Token(pos);
                dic.Add(pos, token);
                graph.AddNode(token);
            }
        }
        foreach (var token in dic.Values)
        {
            for (int i = 0; i <= 1; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    var nKey = new Vector2I(i, j) + token.Pos;
                    if (dic.ContainsKey(nKey) == false) continue;
                    var nToken = dic[nKey];
                    var cost = Game.I.Random.RandiRange(1, 10);
                    graph.Connect(token, nToken, cost, cost);
                    token.Neighbors.Add(nToken, cost);
                    nToken.Neighbors.Add(token, cost);
                }
            }
        }
        return graph;
    }
    private class Token
    {
        public Vector2I Pos { get; private set; }
        public Dictionary<Token, float> Neighbors { get; private set; }
        public Token(Vector2I pos)
        {
            Pos = pos;
            Neighbors = new Dictionary<Token, float>();
        }
    }
}


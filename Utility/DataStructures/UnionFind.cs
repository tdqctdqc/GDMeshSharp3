using Godot;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public static class UnionFind
{
    public static List<List<T>> Find<T>(IReadOnlyGraph<T> graph, IEnumerable<T> elements, 
        Func<T,T,bool> compare)
    {
        return Find(elements, compare, t => graph.GetNeighbors(t));
    }
    public static List<List<T>> Find<T>(IEnumerable<T> elements, 
        Func<T,T,bool> compare)
        where T : IGraphNode<T>
    {
        return Find(elements, compare, t => t.Neighbors);
    }
    public static List<List<T>> Find<T>(IEnumerable<T> elements, 
        Func<T,T,bool> compare, 
        Func<T, IEnumerable<T>> neighborFunc)
    {
        var unionFind = new UnionFind<T>(compare);
        foreach (var element in elements)
        {
            unionFind.AddElement(element, neighborFunc(element));   
        }
        unionFind.CheckRoots();
        return unionFind.GetUnions();
    }
}
public class UnionFind<T>
{
    private Dictionary<T, T> _parents;
    private Dictionary<T, int> _ranks;
    private Func<T, T, bool> _compare;

    public UnionFind(Func<T, T, bool> compare)
    {
        _parents = new Dictionary<T, T>();
        _ranks = new Dictionary<T, int>();
        _compare = compare;
    }
    public void AddElement(T x, IEnumerable<T> neighbors)
    {
        MakeSet(x);
        foreach (var neighbor in neighbors)
        {
            if (_ranks.ContainsKey(neighbor))
            {
                if (_compare(neighbor, x)
                    && _parents[neighbor].Equals(_parents[x]) == false)
                {
                    Union(neighbor, x);
                }
            }
        }
    }
    

    public void CheckRoots()
    {
        foreach (var element in _ranks.Keys)
        {
            _parents[element] = FindRoot(element);
        }
    }
    public List<List<T>> GetUnions()
    {
        var unions = new Dictionary<T, List<T>>();
        
        foreach (var entry in _parents)
        {
            var element = entry.Key;
            var parent = entry.Value;
            if (unions.ContainsKey(parent) == false)
            {
                unions.Add(parent, new List<T>());
            }
            unions[parent].Add(element);
        }

        return unions.Values.ToList();
    }
    private void MakeSet(T x)
    {
        if (_parents.ContainsKey(x) == false)
        {
            _parents.Add(x,x);
            _ranks.Add(x,0);
        }
    }
    private void Union(T x, T y)
    {
        var xRoot = FindRoot(x);
        var yRoot = FindRoot(y);
        if (xRoot.Equals(yRoot)) return;
        else if (_ranks[xRoot] > _ranks[yRoot])
        {
            _parents[yRoot] = xRoot;
        }
        else
        {
            _parents[xRoot] = yRoot;
            _ranks[yRoot] += 1;
        }
    }
    
    private T FindRoot(T x)
    {
        if (_parents[x].Equals(x) == false)
        {
            return FindRoot(_parents[x]);
        }

        return x;
    }
}


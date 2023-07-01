using Godot;
using System;

public class PathFinderNode<T>
{
    public T Element { get; private set; }
    public PathFinderNode<T> Parent { get; set; }

    public PathFinderNode(T element)
    {
        Element = element;
    }
}

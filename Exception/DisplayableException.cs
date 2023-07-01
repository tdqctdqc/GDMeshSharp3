
using System;
using Godot;

public abstract class DisplayableException : Exception
{
    public abstract Node2D GetGraphic();
    public abstract Control GetUi();
}

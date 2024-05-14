using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Frontline
{
    public Alliance Alliance { get; private set; }
    public List<FrontFace> Faces { get; private set; }
    public HashSet<Cell> AdvanceInto { get; private set; }
    public Frontline(List<FrontFace> faces, 
        Alliance alliance)
    {
        Faces = faces;
        Alliance = alliance;
    }
}
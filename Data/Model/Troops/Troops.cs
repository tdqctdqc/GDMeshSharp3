
using System.Collections.Generic;
using System.IO;
using Godot;
using Microsoft.VisualBasic.FileIO;
using FileAccess = Godot.FileAccess;

public class Troops : ModelPredefs<Troop>
{
    public Troop Rifle1 { get; private set; }
        = new();
    public Troop Rifle2 { get; private set; }
        = new();
    public Troop Rifle3 { get; private set; }
        = new();
    public Troop Rifle4 { get; private set; }
        = new();
    public Troop Rifle5 { get; private set; }
        = new();
    public Troop Artillery1 { get; private set; }
        = new();
    public Troop Artillery2 { get; private set; }
        = new();
    public Troop Artillery3 { get; private set; }
        = new();
    public Troop MachineGun1 { get; private set; }
        = new();
    public Troop MachineGun2 { get; private set; }
        = new();
    public Troop MachineGun3 { get; private set; }
        = new();
    public Troop MachineGun4 { get; private set; }
        = new();
    public Troops()
    {
    }
}
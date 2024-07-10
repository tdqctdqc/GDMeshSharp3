
using System.Collections.Generic;
using System.IO;
using Godot;
using Microsoft.VisualBasic.FileIO;
using FileAccess = Godot.FileAccess;

public class Troops : ModelList<Troop>
{
    public Troop Rifle1 { get; private set; }
    public Troop Rifle2 { get; private set; }
    public Troop Rifle3 { get; private set; }
    public Troop Rifle4 { get; private set; }
    public Troop Rifle5 { get; private set; }
    public Troop Artillery1 { get; private set; }
    public Troop Artillery2 { get; private set; }
    public Troop Artillery3 { get; private set; }
    public Troop MachineGun1 { get; private set; }
    public Troop MachineGun2 { get; private set; }
    public Troop MachineGun3 { get; private set; }
    public Troop MachineGun4 { get; private set; }
    public Troops(Dictionary<string, IModel> modelsByName)
    {
        string filePath = Directory.GetCurrentDirectory();
        filePath += "\\Data\\Model\\Troops\\TroopsSource.csv";

        var info =
            GodotFileExt.ReadCsvGrid(filePath);
        Rifle1 = new Troop(nameof(Rifle1), TroopDomain.Land,
            modelsByName, info);
        Rifle2 = new Troop(nameof(Rifle2), TroopDomain.Land,
            modelsByName, info);
        Rifle3 = new Troop(nameof(Rifle3), TroopDomain.Land,
            modelsByName, info);
        Rifle4 = new Troop(nameof(Rifle4), TroopDomain.Land,
            modelsByName, info);
        Rifle5 = new Troop(nameof(Rifle5), TroopDomain.Land,
            modelsByName, info);
        Artillery1 = new Troop(nameof(Artillery1), TroopDomain.Land,
            modelsByName, info);
        Artillery2 = new Troop(nameof(Artillery2), TroopDomain.Land,
            modelsByName, info);
        Artillery3 = new Troop(nameof(Artillery3), TroopDomain.Land,
            modelsByName, info);
        MachineGun1 = new Troop(nameof(MachineGun1), TroopDomain.Land,
            modelsByName, info);
        MachineGun2 = new Troop(nameof(MachineGun2), TroopDomain.Land,
            modelsByName, info);
        MachineGun3 = new Troop(nameof(MachineGun3), TroopDomain.Land,
            modelsByName, info);
        MachineGun4 = new Troop(nameof(MachineGun4), TroopDomain.Land,
            modelsByName, info);
    }
}
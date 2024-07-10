
using GDMeshSharp3.Data.Model.Troops;

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
    public Troops(Items items, FlowList flows)
    {
        Rifle1 = new Rifle1(items, flows);
        Rifle2 = new Rifle2(items, flows);
        Rifle3 = new Rifle3(items, flows);
        Rifle4 = new Rifle4(items, flows);
        Rifle5 = new Rifle5(items, flows);
        Artillery1 = new Artillery1(items, flows);
        Artillery2 = new Artillery2(items, flows);
        Artillery3 = new Artillery3(items, flows);
        MachineGun1 = new MachineGun1(items, flows);
        MachineGun2 = new MachineGun2(items, flows);
        MachineGun3 = new MachineGun3(items, flows);
        MachineGun4 = new MachineGun4(items, flows);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using MessagePack;

public class DeploymentAi
{
    public ERef<Alliance> Alliance { get; private set; }
    public DeploymentRoot Root { get; private set; }
    public IdDispenser IdDispenser { get; private set; }
    public static DeploymentAi Construct(Alliance a, Data d)
    {
        var ai = new DeploymentAi(a.MakeRef(),
            null,
            new IdDispenser(0));
        return ai;
    }

    public DeploymentAi(ERef<Alliance> alliance, DeploymentRoot root, IdDispenser idDispenser)
    {
        Alliance = alliance;
        Root = root;
        IdDispenser = idDispenser;
    }

    public void Clear(LogicKey key)
    {
        IdDispenser = new IdDispenser(0);
        Root = null;
    }
    public void Calculate(Alliance alliance, LogicKey key)
    {
        Clear(key);
        Root = new DeploymentRoot(alliance.MakeRef(),
            key.Data.IdDispenser.TakeId(),
            new HashSet<DeploymentBranch>(), 
            new HashSet<ArmyAssignment>());
        Root.MakeTheaters(alliance.GetAi(key.Data).Military, key);
        Root.GrabUnassignedGroups(key);
        Root.ShiftGroups(this, key);
        Root.GiveOrders(this, key);
    }
    

    public DeploymentRoot GetRoot()
    {
        return Root;
    }
}
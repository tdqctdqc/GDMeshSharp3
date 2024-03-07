using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using MessagePack;

public class DeploymentAi
{
    public Alliance Alliance { get; private set; }
    private DeploymentRoot _root;
    private Data _data;
    public IdDispenser IdDispenser { get; private set; }
    public MilAiMemo Memo { get; set; }
    public static DeploymentAi Construct(Alliance a, Data d)
    {
        var ai = new DeploymentAi(a, d);
        
        return ai;
    }
    private DeploymentAi(Alliance a, 
        Data d)
    {
        _data = d;
        Alliance = a;
        IdDispenser = new IdDispenser(0);
    }

    public void Clear(LogicWriteKey key)
    {
        IdDispenser = new IdDispenser(0);
    }
    public void Calculate(AllianceMilitaryAi ai, LogicWriteKey key)
    {
        Clear(key);
        _root = new DeploymentRoot(this, key);
        _root.MakeTheaters(ai, key);
        if (Memo != null)
        {
            Memo.Finish(this, _root, key);
            Memo = null;
        }
        _root.GrabUnassignedGroups(key);
        _root.ShiftGroups(this, key);
        _root.GiveOrders(this, key);

        Memo = new MilAiMemo(Alliance, key.Data);
    }
    

    public DeploymentRoot GetRoot()
    {
        return _root;
    }
}
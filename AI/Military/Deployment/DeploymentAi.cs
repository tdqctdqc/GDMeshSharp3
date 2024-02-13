using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using MessagePack;

public class DeploymentAi
{
    public Regime Regime { get; private set; }
    private DeploymentRoot _root;
    private Data _data;
    public MilAiMemo Memo { get; set; }
    public static DeploymentAi Construct(Regime r, Data d)
    {
        var ai = new DeploymentAi(r, new IdRecycler(), d);
        
        return ai;
    }
    private DeploymentAi(Regime r, IdRecycler deploymentTreeIds, 
        Data d)
    {
        _data = d;
        Regime = r;
    }

    public void Clear(LogicWriteKey key)
    {
    }
    public void Calculate(Regime regime, LogicWriteKey key, MinorTurnOrders orders)
    {
        Clear(key);
        _root = new DeploymentRoot(this, key);
        _root.MakeTheaters(this, key);
        if (Memo != null)
        {
            Memo.Finish(this, _root, key);
            Memo = null;
        }
        _root.GrabUnassignedGroups(key);
        _root.ShiftGroups(this, key);
        _root.AdjustWithin(this, key);
        _root.GiveOrders(this, key);

        Memo = new MilAiMemo(Regime, key.Data);
    }
    

    public DeploymentRoot GetRoot()
    {
        return _root;
    }
}
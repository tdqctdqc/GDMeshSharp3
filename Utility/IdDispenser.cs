using Godot;
using System;
using System.Collections.Generic;

public class IdDispenser
{
    private int _index = 0;

    public IdDispenser()
    {
        
    }
    public int GetID()
    {
        _index++;
        if (_index == int.MaxValue) throw new Exception("Max Ids reached");
        int id = _index;
        return id;
    }

    public void SetMin(int taken)
    {
        _index = Mathf.Max(taken, _index);
    }
}
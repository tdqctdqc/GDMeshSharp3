using System;
using System.Collections.Generic;
using Godot;

public class ClientCallbacks
{
    private Dictionary<int, Action> _callbacks;
    private int _id;
    public ClientCallbacks()
    {
        _callbacks = new Dictionary<int, Action>();
        _id = 0;
    }

    public int AddCallback(Action cb)
    {
        var id = _id++;
        if (id == int.MaxValue) throw new Exception();
        _callbacks.Add(id, cb);
        return id;
    }

    public void CallBack(int id, ProcedureWriteKey key)
    {
        try
        {
            _callbacks[id]();
        }
        catch (Exception e)
        {
            GD.Print("callback failed");
            GD.Print(e.Message);
        }
        _callbacks.Remove(id);
    }
}
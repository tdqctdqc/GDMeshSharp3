using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WorldGenLogic : ILogic
{
    public bool Generating { get; private set; }
    private bool _justGenned = false;
    private int _tries = 0;
    private GameSession _session;
    public GenData Data => (GenData) _session.Data;
    public Action FinishedGenSuccessfully { get; set; }
    public bool Succeeded { get; private set; }

    public WorldGenLogic(GameSession session)
    {
        _session = session;
    }
    public void Process(float delta)
    {
        if (_justGenned)
        {
            Succeeded = true;
            _justGenned = false;
            FinishedGenSuccessfully?.Invoke();
        }
    }
    
    public void TryGenerate()
    {
        _tries = 0;
        Succeeded = false;
        Generating = true;
        var w = new WorldGenerator((GenData)_session.Data, _session,
            () => _justGenned = true);
        w.Generate();

        try
        {       
            GD.Print("TRYING GEN");
        }
        catch
        {
            if (Data.GenMultiSettings.PlanetSettings.RetryGen.Value)
            {
                GD.Print("RETRYING GEN");
                RetryGen();
            }
            else throw;
        }
        
        Generating = false;
    }

    private void RetryGen()
    {
        _session.ResetAsGenerator(this);
        try
        {
            _tries++;
            var w = new WorldGenerator((GenData)_session.Data, _session,
                () => _justGenned = true);
            w.Generate();
        }
        catch (Exception e)
        {
            if (Data.GenMultiSettings.PlanetSettings.RetryGen.Value)
            {
                GD.Print("RETRYING GEN FAILED");
                if (_tries > 10) throw e;
                else
                {
                    GD.Print("RE RETRYING GEN");
                    RetryGen();
                }
            }
            else throw;
        }
        Generating = false;
    }
}

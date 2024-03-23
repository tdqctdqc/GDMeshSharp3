using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class WorldGenerator
{
    public GenData Data { get; private set; }
    public bool Generating { get; private set; }
    private GenWriteKey _key;
    private Stopwatch _totalTime;
    private Action _succeeded;
    public WorldGenerator(GenData data, GameSession session, Action succeeded)
    {
        _succeeded = succeeded;
        Data = data;
        _key = new GenWriteKey(Data, session);
        _totalTime = new Stopwatch();
    }
    public GenReport Generate()
    {
        Generating = true;
        Game.I.Random.Seed = (ulong)Data.GenMultiSettings.PlanetSettings.Seed.Value;
        var report = new GenReport(GetType().Name);
        GenerateInner(report);
        Data.Generated = true;
        Generating = false;
        _succeeded.Invoke();
        return report;
    }

    private void GenerateInner(GenReport r)
    {
        _totalTime.Start();
    
        var sw = new Stopwatch();
        sw.Start();
        
        var polySize = 200f;
        var edgePointMargin = new Vector2(polySize, polySize);
        var dim = Data.GenMultiSettings.Dimensions;
        
        _key.Data.Logger.RunAndLogTime("Init", 
            LogType.Generation,
            () =>
             {
                 Data.CreateFirstTime(_key);
             }
        );
        RunGenerator(new PolygonGenerator(
            Data.GenMultiSettings.Dimensions, 
            true));
        RunGenerator(new GeologyGenerator());
        RunGenerator(new MoistureGenerator());
        RunGenerator(new PolyCellGenerator());
        // RunGenerator(new ResourceGenerator());
        RunGenerator(new RegimeGenerator());
        RunGenerator(new SocietyGenerator());
        RunGenerator(new InfrastructureGenerator());

        _totalTime.Stop();
        _key.Data.Logger.Log("Total entities genned: " 
                          + Data.EntitiesById.Count,
            LogType.Generation);
        _key.Data.Logger.Log("Total id count: " 
                          + Data.IdDispenser.Index,
            LogType.Generation);
        _key.Data.Logger.Log("Total gen time: " + _totalTime.Elapsed.TotalMilliseconds,
            LogType.Generation);
        Data.Notices.Gen.FinishedGen.Invoke();
    }

    private void RunGenerator(Generator gen)
    {
        _key.Data.Logger.RunAndLogTime(gen.GetType().Name, LogType.Generation, 
            () =>
            {
                var r = gen.Generate(_key);
                _key.Data.Logger.Log(r.GetTimes(), LogType.Generation);
            }
        );
    }
    
}
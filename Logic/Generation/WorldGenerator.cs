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
        
        List<Vector2> points = null;
        _key.Data.Logger.RunAndLogTime("Generating points", LogType.Generation, 
            () =>
            {
                points = PointsGenerator
                    .GenerateConstrainedSemiRegularPoints
                        (Data.GenMultiSettings.Dimensions - edgePointMargin, polySize, polySize * .75f, false, true)
                    .Select(v => v + edgePointMargin / 2f).ToList();
            
                foreach (var p in points)
                {
                    if (p != p.Intify()) throw new Exception("not int point");
                    if (p.X < 0 || p.X > dim.X || p.Y < 0 || p.Y > dim.Y) throw new Exception("point out of bounds");
                }
            }
        );
        
        RunGenerator(new PolygonGenerator(points, Data.GenMultiSettings.Dimensions, 
            true, polySize));
        RunGenerator(new GeologyGenerator());
        RunGenerator(new ResourceGenerator());

        _key.Data.Logger.RunAndLogTime("Edge disturb", LogType.Generation,
            () =>
            {
                var polys = Data.GetAll<MapPolygon>();
                EdgeDisturber.SplitEdges(polys, _key,
                    Data.GenMultiSettings.PlanetSettings.PreferredMinPolyEdgeLength.Value);
                EdgeDisturber.DisturbEdges(polys, _key);
            }
        );
        
        RunGenerator(new MoistureGenerator());
        RunGenerator(new PolyCellGenerator());
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
        Data.Notices.FinishedGen.Invoke();
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

using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class Game : Node
{
    public static Game I { get; private set; }
    public Serializer Serializer { get; private set; }
    public Logger Logger { get; private set; }
    // public Guid PlayerGuid { get; private set; } = Guid.NewGuid();
    public RandomNumberGenerator Random = new RandomNumberGenerator();
    private ISession _session;

    public RefFulfiller RefFulfiller => _session.RefFulfiller;
    public IClient Client => _session.Client;
    public override void _Ready()
    {
        if (I != null)
        {
            throw new Exception();
        }
        I = this;
        Logger = new Logger();
        Assets.Setup();
        SetSerializer();
        StartMainMenuSession();
        Test();
    }

    private void Test()
    {
        // var segCount = 100f;
        // var testIters = 1000;
        // var segs = new List<LineSegment>();
        // var arm = Vector2.One * 100f;
        // var points = new List<Vector2>();
        // for (var i = 0; i < segCount - 1; i++)
        // {
        //     var ratioFrom = i / segCount;
        //     var ratioTo = (i + 1) / segCount;
        //     var point = arm.Rotated(ratioFrom * Mathf.Pi * 2f);
        //     points.Add(point);
        // }
        //
        // for (var i = 0; i < points.Count - 1; i++)
        // {
        //     segs.Add(new LineSegment(points[i], points[i + 1]));
        // }
        //
        // segs = segs.OrderBy(s => Random.Randf()).ToList();
        // var sw = new Stopwatch();
        //
        // var chainedNew = segs.Chainify();
        // sw.Start();
        // for (var i = 0; i < testIters; i++)
        // {
        //     chainedNew = segs.ChainifyNew();
        // }
        // sw.Stop();
        // var newTime = sw.Elapsed.TotalMilliseconds;
        // sw.Reset();
        //
        //
        // var chainedOld = segs.Chainify();
        // sw.Start();
        // for (var i = 0; i < testIters; i++)
        // {
        //     chainedOld = segs.Chainify();
        // }
        // sw.Stop();
        // var oldTime = sw.Elapsed.TotalMilliseconds;
        // sw.Reset();
        //
        //
        //
        //
        //
        // GD.Print("old " + oldTime);
        // GD.Print("new " + newTime);
    }
    public void SetSerializer()
    {
        Serializer = new Serializer();
    }
    public void StartMainMenuSession()
    {
        SetSession(new MainMenuSession());
    }
    public void StartGeneratorSession()
    {
        SetSession(new GeneratorSession());
    }
    public void StartClientSession()
    {
        var session = new GameSession();
        SetSession(session);
        session.StartAsRemote();

    }
    public void StartHostSession(GenData data, MapGraphics graphics = null)
    {
        var session = new GameSession();
        SetSession(session);
        session.StartAsHost(data, graphics);
    }

    public void StartSandbox()
    {
        SetSession(new SandboxSession());
    }

    private void SetSession(Node session)
    {
        if(_session != null) RemoveChild((Node) _session);
        _session?.QueueFree();
        session.Name = "Session";
        _session = (ISession)session;
        _session.Setup();
        AddChild(session);
    }
}

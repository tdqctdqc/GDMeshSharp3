using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class DataNotices
{
    public RefAction FinishedStateSync { get; private set; }
    public RefAction GeneratedRegimes { get; private set; }
    public RefAction PopulatedWorld { get; private set; }
    public RefAction MadeResources { get; private set; }
    public RefAction<int> Ticked { get; private set; }
    public RefAction FinishedTurnStartCalc { get; private set; }
    public RefAction SetPolyShapes { get; private set; }
    public RefAction MadeWaypoints { get; private set; }
    public RefAction SetLandAndSea { get; private set; }
    public RefAction ExitedGen { get; private set; }
    public RefAction FinishedAiCalc { get; set; }
    public DataNotices()
    {
        PopulatedWorld = new RefAction();
        GeneratedRegimes = new RefAction();
        MadeResources = new RefAction();
        FinishedStateSync = new RefAction();
        Ticked = new RefAction<int>();
        FinishedTurnStartCalc = new RefAction();
        SetPolyShapes = new RefAction();
        SetLandAndSea = new RefAction();
        ExitedGen = new RefAction();
        MadeWaypoints = new RefAction();
        FinishedAiCalc = new RefAction();
    }
}


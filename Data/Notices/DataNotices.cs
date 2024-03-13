using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GDMeshSharp3.Data.Notices;
using Godot;

public class DataNotices
{
    public RefAction FinishedStateSync { get; private set; }
    public RefAction<int> Ticked { get; private set; }
    public RefAction FinishedTurnStartCalc { get; private set; }
    public RefAction FinishedTurnEndCalc { get; private set; }
    public RefAction FinishedAiCalc { get; set; }
    public RefAction<(Cell c, Regime oldRegime, Regime newRegime)> 
        CellChangedController { get; private set; }

    public GenNotices Gen { get; private set; }
    public PlayerNotices Player { get; private set; }
    public PoliticalNotices Political { get; private set; }
    public MilitaryNotices Military { get; private set; }
    public InfrastructureNotices Infrastructure { get; private set; }
    public DataNotices()
    {
        Gen = new GenNotices();
        Player = new PlayerNotices();
        Political = new PoliticalNotices();
        Military = new MilitaryNotices();
        Infrastructure = new InfrastructureNotices();
        
        FinishedStateSync = new RefAction();
        Ticked = new RefAction<int>();
        FinishedTurnStartCalc = new RefAction();
        FinishedTurnEndCalc = new RefAction();
        FinishedAiCalc = new RefAction();
        CellChangedController = new RefAction<(Cell, Regime, Regime)>();
        
    }
}


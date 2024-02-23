using Godot;
using System;
using System.Collections.Generic;

public class GenData : Data
{
    public bool Generated { get; set; }
    public GenAuxiliaryData GenAuxData { get; private set; }
    public GenerationMultiSettings GenMultiSettings { get; private set; }
    public GenData() : base()
    {
        GenMultiSettings = GenerationMultiSettings.Load(this);
    }

    protected override void Init()
    {
        GenAuxData = new GenAuxiliaryData(this);
        base.Init();
    }
    public void CreateFirstTime(GenWriteKey key)
    {
        EntityIds.Create(key);
        GameClock.Create(key);
        PlanetInfo.Create(GenMultiSettings.Dimensions, key);
        Market.Create(key);
        RuleVars.CreateDefault(key);
        CurrentConstruction.Create(key);
        ProposalList.Create(key);
        ClientPlayerData.SetLocalPlayerGuid(new Guid());
        Player.Create(ClientPlayerData.LocalPlayerGuid, "Doot", key);
        DiplomacyGraph.Create(key);
    }
    public void ClearAuxData()
    {
        GenAuxData = null;
    }
}

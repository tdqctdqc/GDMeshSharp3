using Godot;
using System;
using System.Collections.Generic;

public class GenData : Data
{
    public bool Generated { get; set; }
    public GenAuxiliaryData GenAuxData { get; private set; }
    public MapGenInfo GenInfo { get; set; }
    public GenerationMultiSettings GenMultiSettings { get; private set; }
    public GenData()
    {
        GenMultiSettings = GenerationMultiSettings.Load(this);
    }

    protected override void Init()
    {
        GenAuxData = new GenAuxiliaryData(this);
        base.Init();
    }
    public void ClearAuxData()
    {
        GenAuxData = null;
    }
}

using Godot;
using System;
using System.Collections.Generic;

public class GenData : Data
{
    public GenAuxiliaryData GenAuxData { get; private set; }
    public MapGenInfo GenInfo { get; set; }
    public GenerationMultiSettings GenMultiSettings { get; private set; }
    public GenData(GenerationMultiSettings genMultiSettings)
    {
        GenMultiSettings = genMultiSettings;
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

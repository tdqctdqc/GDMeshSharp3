using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SettlementTier : IModel, IIconed
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public int MinSize { get; private set; }
    public Icon Icon { get; private set; }

    public SettlementTier()
    {
    }

    public void CreateIcon()
    {
        Icon = Icon.Create(Name, Vector2I.One);
    }
}

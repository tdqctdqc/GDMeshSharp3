
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class GoToCellGroupOrder : UnitGroupOrder
{
    public int DestId { get; private set; }
    public static GoToCellGroupOrder Construct(Cell destWp,
        Alliance alliance, UnitGroup g, Data d)
    {
        var currWp = g.GetCell(d);
        var moveType = g.MoveType(d);
        
        if (moveType.Passable(destWp, alliance, d) == false)
        {
            throw new Exception($"{moveType.GetType().Name} cant go to {destWp.GetType().Name}" +
                                $" alliance {alliance.Leader.Entity(d).Id}" +
                                $" occupier {destWp.Controller.RefId} ");
        }
        
        
        return new GoToCellGroupOrder(destWp.Id);
    }
    [SerializationConstructor] private GoToCellGroupOrder(
        int destId)
    {
        DestId = destId;
    }
    
    public override void Handle(UnitGroup g, LogicWriteKey key, 
        HandleUnitOrdersProcedure proc)
    {
        var d = key.Data;
        var alliance = g.Regime.Entity(d).GetAlliance(d);
        var dest = PlanetDomainExt.GetPolyCell(DestId, d);
        foreach (var unit in g.Units.Items(d))
        {
            var pos = unit.Position.Copy();
            var moveType = unit.Template.Entity(d).MoveType.Model(d);
            var movePoints = moveType.BaseSpeed;
            var moveData = new MoveData(unit.Id, moveType, movePoints, alliance);
            pos.MoveToCell(moveData, dest, key);
            proc.NewUnitPosesById.TryAdd(unit.Id, pos);
        }
    }
    public override void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data d)
    {
        var dest = PlanetDomainExt.GetPolyCell(DestId, d);
        var alliance = group.Regime.Entity(d).GetAlliance(d);
        foreach (var unit in group.Units.Items(d))
        {
            var from = unit.Position.GetCell(d);
            var moveType = unit.Template.Entity(d).MoveType.Model(d);
            var path = d.Context.PathCache
                .GetOrAdd((moveType, alliance, from, dest));
            mb.DrawCellPath(relTo, path, group.Color, 2f, d);
        }
    }

    public override void RegisterCombatActions(UnitGroup group, CombatCalculator combat, LogicWriteKey key)
    {
        
    }

    public override string GetDescription(Data d)
    {
        return $"Going to cell {DestId}";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapUiElements
{
    private PolyGrid<IUiCollidable> _collidables;
    private Dictionary<int, List<IUiCollidable>> _order;
    public MapUiElements(Client c)
    {
        c.Data.Notices.Ticked.SubscribeForNode(
            i => Setup(c),
            c
        );
        Setup(c);
    }

    public void Setup(Client c)
    {
        c.Data.Logger.Log("setting up map ui elements",
            LogType.Ui);
        _order = new Dictionary<int, List<IUiCollidable>>();
        _collidables = new PolyGrid<IUiCollidable>(c.Data.Planet.Dim,
            100f,
            c => c.RelPolygonBoundaries,
            c => c.RelTo
        );
    }

    public void Add(IUiCollidable c)
    {
        // Game.I.Client.Data.Logger.Log("adding map ui element",
        //     LogType.Ui);
        _order.AddOrUpdate(c.Z, c);
        _collidables.AddElement(c);
    }

    public void MoveToTop(IUiCollidable c)
    {
        var have = _order[c.Z].Remove(c);
        if (have == false) throw new Exception();
        _order[c.Z].Add(c);
    }
    public bool HandleInput(InputEvent e, Vector2 mapPos, Client c)
    {
        if (e is InputEventMouseButton mb
            && mb.ButtonIndex == MouseButton.Left)
        {
            c.Data.Logger.Log("Searching for map ui element" +
                              $"out of {_order.Count}",
                LogType.Ui);
        }
        
        var cs = _collidables
            .GetAllElementsAtPointWhere(
                mapPos,
                c => c.IsCapturing(),
                c.Data).ToHashSet();
        if (cs.Any() == false)
        {
            return false;
        }
        c.Data.Logger.Log("Found map ui element",
            LogType.Ui);


        var z = cs.Max(c => c.Z);
        
        var top = _order[z]
            .Intersect(cs)
            .MaxBy(c =>
            {
                var i = _order[z].IndexOf(c);
                if (i == -1) throw new Exception();
                return i;
            });
        if (top.Captures(e) == false) return false;
        top.Handle(e, mapPos, c);
        return true;
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ArmyGraphicManager : ISettinged
{
    public EntityGraphicCache<Army, ArmyAreaGraphic> ArmyAreaGraphics { get; private set; }
    public EntityGraphicCache<Army, ArmyIconGraphic> ArmyIconGraphics { get; private set; }

    public EntityGraphicCache<Army, ArmyHistoryGraphic> ArmyHistoryGraphics { get; private set; }
    private ConcurrentBag<Cell> _redraw;
    public Dictionary<Cell, List<Army>> ArmiesInOrder { get; private set; }
    public ArmyGraphicManager(Client c)
    {
        _redraw = new ConcurrentBag<Cell>();
        ArmyAreaGraphics = new EntityGraphicCache<Army, ArmyAreaGraphic>(
            a =>
            {
                var g = new ArmyAreaGraphic();
                c.QueuedUpdates.Enqueue(() =>
                {
                    g.Initialize();
                    g.Draw(a, c);
                });
                return g;
            }, c.Data);
        ArmyIconGraphics = new EntityGraphicCache<Army, ArmyIconGraphic>(
            a =>
            {
                var g = new ArmyIconGraphic();
                c.QueuedUpdates.Enqueue(() =>
                {
                    g.Initialize();
                    g.Draw(a, c);
                });
                var cell = a.GetHomeCell(c.Data);
                ArmiesInOrder.AddOrUpdate(cell, a);
                _redraw.Add(cell);
                return g;
            }, c.Data);
        ArmyHistoryGraphics = new EntityGraphicCache<Army, ArmyHistoryGraphic>(
            a =>
            {
                var g = new ArmyHistoryGraphic();
                c.QueuedUpdates.Enqueue(() =>
                {
                    g.Initialize();
                    g.Draw(a, c);
                });
                return g;
            },
            c.Data);

        ArmiesInOrder = new Dictionary<Cell, List<Army>>();
        c.Data.Notices.Ticked.Subscribe(i =>
        {
            DrawAll(c);
        });
        c.Data.Notices.FinishedStateSync.Subscribe(() =>
        {
            DrawAll(c);
        });
        c.Data.Notices.Gen.FinishedGen.Subscribe(() =>
        {
            DrawAll(c);
        });
        c.UiTick.Subscribe(() =>
        {
            Redraw(c);
        });

        var armyMode = c.UiController.ModeOption.Options.OfType<ArmyMode>().First();
        armyMode.Army.SettingChanged.Subscribe(v =>
        {
            
        });
    }

    private void DrawAll(Client c)
    {
        PositionAllIcons(c);
        foreach (var (army, graphic) in ArmyAreaGraphics.Graphics)
        {
            graphic.Draw(army, c);
        }
        foreach (var (army, graphic) in ArmyIconGraphics.Graphics)
        {
            graphic.Draw(army, c);
        }
        foreach (var (army, graphic) in ArmyHistoryGraphics.Graphics)
        {
            graphic.Draw(army, c);
        }
    }

    
    private void PositionAllIcons(Client c)
    {
        SetArmiesInCellOrder(c);
        foreach (var (cell, armies) in ArmiesInOrder)
        {
            RedrawCell(cell, c);
        }
    }

    private void SetArmiesInCellOrder(Client c)
    {
        ArmiesInOrder.Clear();
        foreach (var army in c.Data.GetAll<Army>())
        {
            var homeCell = army.GetHomeCell(c.Data);
            ArmiesInOrder.AddOrUpdate(homeCell, army);
        }
    }
    private void Redraw(Client c)
    {
        var redraw = _redraw.ToArray();
        _redraw.Clear();
        var segmenter = c.GetComponent<MapGraphics>()
            .Segmenter;
        c.QueuedUpdates.Enqueue(() =>
        {
            for (var i = 0; i < redraw.Length; i++)
            {
                var cell = redraw[i];
                RedrawCell(cell, c);
            }
        });
    }

    private void RedrawCell(
        Cell cell, 
        Client c)
    {
        var segmenter = c.GetComponent<MapGraphics>()
            .Segmenter;
        var armies = ArmiesInOrder[cell];
        var center = cell.GetCenter();
        if (armies.Count == 1)
        {
            var army = armies[0];
            var icon = ArmyIconGraphics.Graphics[army];
            c.QueuedUpdates.Enqueue(
                () => segmenter.AddElement(icon, center));
        }
        else
        {
            var length = Mathf.Min(10f, armies.Count * 2f);
            var from = center + Vector2.One * length / 2f;
            var to = center - Vector2.One * length / 2f;

            for (var j = armies.Count - 1; j >= 0; j--)
            {
                var army = armies[j];
                var pos = from.Lerp(to, (float)j / (armies.Count - 1));
                var icon = ArmyIconGraphics.Graphics[army];
                c.QueuedUpdates.Enqueue(
                    () => segmenter.AddElement(icon, pos));
            }
        }
    }
    public void CycleArmies(Cell cell, Client c)
    {
        if (ArmiesInOrder.TryGetValue(cell, out var list) == false
            || list.Count < 2) return;
        var first = list[0];
        list.RemoveAt(0);
        list.Add(first);
        RedrawCell(cell, c);
        var newFirst = list[0];
        var area = ArmyAreaGraphics.Graphics[newFirst];
        area.Draw(newFirst, c);
        var history = ArmyAreaGraphics.Graphics[newFirst];
        history.Draw(newFirst, c);
    }

    public void SetArmyToTop(Army army, Client c)
    {
        var cell = army.GetHomeCell(c.Data);
        var list = ArmiesInOrder[cell];
        var remove = list.Remove(army);
        if (remove == false) throw new Exception();
        list.Insert(0, army);
        RedrawCell(cell, c);
        var area = ArmyAreaGraphics.Graphics[army];
        area.Draw(army, c);
    }
    public Settings GetSettings()
    {
        var settings = new Settings("Armies");
        var visibility = new BoolSettingsOption(
            "Visibility", true);
        visibility.SettingChanged.Subscribe(
            v =>
            {
                foreach (var graphic in ArmyAreaGraphics.Graphics.Values)
                {
                    graphic.Visible = v.newVal;
                }
            });
        settings.SettingsOptions.Add(visibility);
        return settings;
    }
}
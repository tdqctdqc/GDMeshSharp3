
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GDMeshSharp3.Client.Settings;
using Godot;

public class ArmyGraphicManager : ISettinged
{
    public EntityGraphicCache<Army, ArmyAreaGraphic> Areas { get; private set; }
    public EntityGraphicCache<Army, ArmyIconGraphic> Icons { get; private set; }
    
    public EntityGraphicCache<Army, ArmyHistoryGraphic> HistoryGraphics { get; private set; }
    private ConcurrentBag<Cell> _redraw;
    private Settings _settings;
    private EnumSettingsOption<HistoryShowEnum> _showHistorySetting;
    private BoolSettingsOption _visibilitySetting;
    public List<Army> ArmyGraphicOrder { get; private set; }
    public ArmyGraphicManager(Client c)
    {
        _redraw = new ConcurrentBag<Cell>();
        MakeSettings();
        Areas = new EntityGraphicCache<Army, ArmyAreaGraphic>(
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
        Icons = new EntityGraphicCache<Army, ArmyIconGraphic>(
            a =>
            {
                var g = new ArmyIconGraphic();
                c.QueuedUpdates.Enqueue(() =>
                {
                    g.Initialize(c);
                    g.Draw(a, c);
                });
                var cell = a.GetHomeCell(c.Data);
                ArmyGraphicOrder.Add(a);
                _redraw.Add(cell);
                return g;
            }, c.Data);
        HistoryGraphics = new EntityGraphicCache<Army, ArmyHistoryGraphic>(
            a =>
            {
                var g = new ArmyHistoryGraphic();
                c.QueuedUpdates.Enqueue(() =>
                {
                    g.Initialize();
                    g.Draw(a, c);
                    g.Visible = ShouldArmyHistBeVisible(a);
                });
                return g;
            },
            c.Data);

        ArmyGraphicOrder = new List<Army>();
        c.Data.Notices.JustTicked.Subscribe(i =>
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

        c.Notices.Selecting.Subscribe(v =>
        {
            if (v is Army a)
            {
                SelectOrCycleByIcon(a, c);
            }
            else
            {
                SelectOrCycleByArea(v.GetCell(c.Data), c);
            }
        });
        var am = c.UiController.ModeOption.Options.OfType<ArmyMode>()
            .First();
        am.Army.SettingChanged.Subscribe(v =>
        {
            if (v.oldVal != null)
            {
                HistoryGraphics.Graphics[v.oldVal].Visible = false;
            }
            if (v.newVal != null)
            {
                HistoryGraphics.Graphics[v.newVal].Visible = true;
            }
        });
        
        c.Data.SubscribeForDestruction<Army>(n =>
        {
            ArmyGraphicOrder.Remove((Army)n.Entity);
        });
    }

    private void DrawAll(Client c)
    {
        var iconIter = new Dictionary<Cell, int>();
        var segmenter = c.GetComponent<MapGraphics>().Segmenter;
        
        c.Data.Logger.RunAndLogTime("drawing army graphics",
            LogType.Graphics,
            () =>
            {
                for (var i = 0; i < ArmyGraphicOrder.Count; i++)
                {
                    var army = ArmyGraphicOrder[i];
                    Areas.Graphics[army].Draw(army, c);
                    HistoryGraphics.Graphics[army].Draw(army, c);
                    Icons.Graphics[army].Draw(army, c);
                    positionIcon(army);
                }
            });
        
        


        void positionIcon(Army army)
        {
            var homeCell = army.GetHomeCell(c.Data);
            var center = homeCell.GetCenter();
            var armies = c.Data.Military.UnitAux.ArmiesByHomeCell[homeCell];
            
            if (armies.Count == 1)
            {
                var icon = Icons.Graphics[army];
                c.QueuedUpdates.Enqueue(
                    () => segmenter.AddElement(icon, center));
            }
            else
            {
                if (iconIter.ContainsKey(homeCell) == false)
                {
                    iconIter[homeCell] = 0;
                }

                var index = iconIter[homeCell];
                iconIter[homeCell]++;
                
                var length = Mathf.Min(10f, armies.Count * 2f);
                var from = center + Vector2.One * length / 2f;
                var to = center - Vector2.One * length / 2f;

                var pos = from.Lerp(to, (float)index / (armies.Count - 1));
                var icon = Icons.Graphics[army];
                c.QueuedUpdates.Enqueue(
                    () => segmenter.AddElement(icon, pos));
            }
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
                ReorderForArmiesWithIconInCell(cell, c);
            }
        });
    }

    
    
    public void SelectOrCycleByIcon(Army army, Client client)
    {
        if (client.UiController.Mode is not ArmyMode am) return;
        var cell = army.GetHomeCell(client.Data);
        var armiesWithIconInCell = client.Data.Military.UnitAux.ArmiesByHomeCell[cell];
        var mapGraphics = client.GetComponent<MapGraphics>();
        var segmenter = mapGraphics.Segmenter;
        var uiElements = mapGraphics.UiElements;
        ArmyGraphicOrder.Remove(army);
        if (am.Army.Value == army)
        {
            ArmyGraphicOrder.Insert(0, army);
        }
        else
        {
            ArmyGraphicOrder.Add(army);
        }

        ReorderForArmiesWithIconInCell(cell, client);

        var newTop = ArmyGraphicOrder.Last(armiesWithIconInCell.Contains);
        am.Army.Set(newTop);
    }
    public void SelectOrCycleByArea(Cell cell, Client client)
    {
        if (client.UiController.Mode is not ArmyMode am) return;
        
        var armiesWithAreaInCell = client.Data.Military.UnitAux
            .ArmiesByOccupancy[cell];
        if (armiesWithAreaInCell.Count == 0)
        {
            am.Army.Set(null);
            return;
        }

        var top = ArmyGraphicOrder.Last(armiesWithAreaInCell.Contains);
        var homeCell = top.GetHomeCell(client.Data);
        ArmyGraphicOrder.Remove(top);
        if (am.Army.Value == top)
        {
            ArmyGraphicOrder.Insert(0, top);
        }
        else
        {
            ArmyGraphicOrder.Add(top);
        }

        var newTop = ArmyGraphicOrder.Last(armiesWithAreaInCell.Contains);
        am.Army.Set(newTop);
        ReorderForArmiesWithIconInCell(homeCell, client);
        ReorderForArmiesWithIconInCell(newTop.GetHomeCell(client.Data), client);
        ReorderForArmiesWithAreaInCell(cell, client);
    }
    
    private void ReorderForArmiesWithIconInCell(
        Cell cell, 
        Client c)
    {
        var mapGraphics = c.GetComponent<MapGraphics>();
        var segmenter = mapGraphics.Segmenter;
        var uiElements = mapGraphics.UiElements;
        var armiesWithIconInCell = 
            c.Data.Military.UnitAux.ArmiesByHomeCell[cell];
        
        var center = cell.GetCenter();
        
        int iter = 0;

        for (var i = 0; i < ArmyGraphicOrder.Count; i++)
        {
            var army = ArmyGraphicOrder[i];
            if (armiesWithIconInCell.Contains(army) == false)
            {
                continue;
            }
            var area = Areas.Graphics[army];
            segmenter.AddElement(area, area.RelTo);
            uiElements.MoveToTop(area);
            var history = HistoryGraphics.Graphics[army];
            segmenter.AddElement(history, history.RelTo);
            uiElements.MoveToTop(history);

            if (armiesWithIconInCell.Count == 1)
            {
                var icon = Icons.Graphics[army];
                c.QueuedUpdates.Enqueue(
                    () => segmenter.AddElement(icon, center));
            }
            else
            {
                var length = Mathf.Min(10f, armiesWithIconInCell.Count * 2f);
                var to = center + Vector2.One * length / 2f;
                var from = center - Vector2.One * length / 2f;
                var pos = from.Lerp(to, (float)iter / (armiesWithIconInCell.Count - 1));
                var icon = Icons.Graphics[army];
                c.QueuedUpdates.Enqueue(
                    () => segmenter.AddElement(icon, pos));
                iter++;
            }
        }
    }
    
    
    private void ReorderForArmiesWithAreaInCell(
        Cell cell, 
        Client c)
    {
        var mapGraphics = c.GetComponent<MapGraphics>();
        var segmenter = mapGraphics.Segmenter;
        var uiElements = mapGraphics.UiElements;
        var armiesWithAreaInCell = 
            c.Data.Military.UnitAux.ArmiesByOccupancy[cell];
        
        var center = cell.GetCenter();
        
        for (var i = 0; i < ArmyGraphicOrder.Count; i++)
        {
            var army = ArmyGraphicOrder[i];
            if (armiesWithAreaInCell.Contains(army) == false)
            {
                continue;
            }
            var area = Areas.Graphics[army];
            segmenter.AddElement(area, area.RelTo);
            uiElements.MoveToTop(area);
            var history = HistoryGraphics.Graphics[army];
            segmenter.AddElement(history, history.RelTo);
            uiElements.MoveToTop(history);
        }
    }

    private void MakeSettings()
    {
        _settings = new Settings("Armies");
        _visibilitySetting = new BoolSettingsOption(
            "Visibility", true);
        _visibilitySetting.SettingChanged.Subscribe(
            v =>
            {
                foreach (var graphic in Areas.Graphics.Values)
                {
                    graphic.Visible = v.newVal;
                }
                foreach (var graphic in Icons.Graphics.Values)
                {
                    graphic.Visible = v.newVal;
                }
                foreach (var (army, graphic) in HistoryGraphics.Graphics)
                {
                    graphic.Visible = ShouldArmyHistBeVisible(army);
                }
            });
        _settings.SettingsOptions.Add(_visibilitySetting);

        _showHistorySetting = new EnumSettingsOption<HistoryShowEnum>("Show History");
        _showHistorySetting.SettingChanged.Subscribe(v =>
        {
            foreach (var (army, graphic) in HistoryGraphics.Graphics)
            {
                graphic.Visible = ShouldArmyHistBeVisible(army);
            }
        });
        _settings.SettingsOptions.Add(_showHistorySetting);
    }
    private bool ShouldArmyHistBeVisible(Army a)
    {
        if (_visibilitySetting.Value == false) return false;
        if (_showHistorySetting.Value == HistoryShowEnum.All)
        {
            return true;
        }
        if (_showHistorySetting.Value == HistoryShowEnum.None)
        {
            return false;
        }

        if (_showHistorySetting.Value == HistoryShowEnum.Selected)
        {
            var selected = Game.I.Client.UiController.ModeOption.Options
                .OfType<ArmyMode>().First().Army.Value;
            return a == selected;
        }

        throw new Exception();
    }
    public Settings GetSettings()
    {
        return _settings;
    }
    enum HistoryShowEnum
    {
        Selected,
        None, 
        All
    }
}
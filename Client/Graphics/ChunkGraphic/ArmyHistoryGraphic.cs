
using System.Linq;
using Godot;

public partial class ArmyHistoryGraphic : CustomClickArea
{
    private MeshInstance2D _mesh;
    public ArmyHistoryGraphic() 
        : base(MouseButtonMask.Left, Vector2.Zero,
            LayerOrder.ArmyHistory)
    {
        ZAsRelative = false;
        ZIndex = (int)LayerOrder.ArmyHistory;
    }

    public void Initialize()
    {
        _mesh = new MeshInstance2D();
        AddChild(_mesh);
    }
    public void Draw(Army army, Client client)
    {
        Clear();
        var mapGraphics = client.GetComponent<MapGraphics>();
        var segmenter = mapGraphics.Segmenter;
        var uiElements = mapGraphics.UiElements;
        SetRelTo(army.GetHomeCell(client.Data).RelTo);
        client.QueuedUpdates.Enqueue(
            () => segmenter.AddElement(this, RelTo));

        var tick = client.Data.BaseDomain.GameClock.Tick;
        var histories = client.Data.Military.CombatHistories.Value;
        var mb = new MeshBuilder();
        if (histories.Histories.TryGetValue(tick, out var history))
        {
            if (history.ArmyCombatHistories.TryGetValue(army.MakeRef(), out var armyHistory))
            {
                DrawHistory(history, armyHistory, client, mb);
            }
        }
        else if (histories.Histories.TryGetValue(tick - 1, out var prevHistory))
        {
            if (prevHistory.ArmyCombatHistories.TryGetValue(army.MakeRef(), out var prevArmyHistory))
            {
                DrawHistory(prevHistory, prevArmyHistory, client, mb);
            }
        }
        else
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                _mesh.Mesh = null;
            });
        }
        uiElements.Add(this);

        if (mb.TriVertices.Count == 0)
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                _mesh.Mesh = null;
            });
            // return;
        }
        else
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                _mesh.Mesh = mb.GetMesh();
            });
        }
    }

    private void DrawHistory(CombatHistory history,
        ArmyCombatHistory armyHistory,
        Client client, MeshBuilder mb)
    {
        foreach (var cellRef in armyHistory.Attacked)
        {
            var cell = cellRef.Get(client.Data);
            var cellHist = history.CellCombatHistories[cellRef];
            var attackedFrom = armyHistory.Occupied
                .Where(c => cell.Neighbors.Contains(c.RefId))
                .Select(c => c.Get(client.Data));
            foreach (var from in attackedFrom)
            {
                if (cellHist.ForcedBack)
                {
                    var arrow = ShapeBuilder.GetArrow(
                        RelTo.Offset(from.GetCenter(), client.Data),
                        RelTo.Offset(cell.GetCenter(), client.Data),
                        3f);
                    mb.DrawPolygon(arrow, Colors.Green);
                    mb.AddArrowRel(from.GetCenter(),
                        cell.GetCenter(),
                        3f, Colors.Green, RelTo, client.Data);
                    Add(arrow, () => Open(cell, client));
                }
                else
                {
                    var mid = from.GetCenter() 
                              + from.GetCenter().Offset(cell.GetCenter(), client.Data) / 2f;
                    var arrow = ShapeBuilder.GetArrow(
                        RelTo.Offset(from.GetCenter(), client.Data),
                        RelTo.Offset(mid, client.Data),
                        3f);
                    mb.DrawPolygon(arrow, Colors.Blue);
                    Add(arrow, () => Open(cell, client));
                }
            }
        }

        foreach (var cellRef in armyHistory.Defended)
        {
            var cell = cellRef.Get(client.Data);
            var cellHist = history.CellCombatHistories[cellRef];
            var color = cellHist.ForcedBack ? Colors.Red : Colors.Orange;
            var center = RelTo.Offset(cell.GetCenter(), client.Data);
            var square = new Vector2[]
            {
                center + Vector2.Left * 5f + Vector2.Up * 5f,
                center - Vector2.Left * 5f + Vector2.Up * 5f,
                center - Vector2.Left * 5f - Vector2.Up * 5f,
                center + Vector2.Left * 5f - Vector2.Up * 5f
            };
            mb.DrawPolygon(square, color);
            Add(square, () => Open(cell, client));
        }
    }


    private void Open(Cell cell, Client client)
    {
        GD.Print("opening cell " + cell.Id);
    }
}
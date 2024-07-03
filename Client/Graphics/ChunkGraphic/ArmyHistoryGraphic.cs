
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
        
        var history = GetHistory(client);
        if (history is not null)
        {
            if (history.NodesById.ContainsKey(army.Id))
            {
                DrawHistory(army, history, client, mb);
            }
        }
        
        uiElements.Add(this);

        if (mb.TriVertices.Count == 0)
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                _mesh.Mesh = null;
            });
        }
        else
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                _mesh.Mesh = mb.GetMesh();
            });
        }
    }

    private void DrawHistory(Army army, CombatGraph graph,
        Client client, MeshBuilder mb)
    {
        var neighbors = graph
            .GetNeighbors(army);
        
        foreach (var atkNode in neighbors.OfType<CellAttackNode>())
        {
            var from = atkNode.From.Get(client.Data);
            var target = atkNode.Target.Get(client.Data);
            var def = (CellDefenseNode)graph.NodesById[graph.CellDefNodes[target.MakeRef()]];
            
            if (def.DefendersForcedBack)
            {
                var arrow = ShapeBuilder.GetArrow(
                    RelTo.Offset(from.GetCenter(), client.Data),
                    RelTo.Offset(target.GetCenter(), client.Data),
                    3f);
                mb.DrawPolygon(arrow, Colors.Green);
                mb.AddArrowRel(from.GetCenter(),
                    target.GetCenter(),
                    3f, Colors.Green, RelTo, client.Data);
                Add(arrow, () => Open(target, client));
            }
            else
            {
                var mid = from.GetCenter() 
                          + from.GetCenter().Offset(target.GetCenter(), client.Data) / 2f;
                var arrow = ShapeBuilder.GetArrow(
                    RelTo.Offset(from.GetCenter(), client.Data),
                    RelTo.Offset(mid, client.Data),
                    3f);
                mb.DrawPolygon(arrow, Colors.Blue);
                Add(arrow, () => Open(target, client));
            }
        }

        foreach (var defNode in neighbors.OfType<CellDefenseNode>())
        {
            var cell = defNode.Cell.Get(client.Data);
            var color = defNode.DefendersForcedBack ? Colors.Red : Colors.Orange;
            var center = RelTo.Offset(cell.GetCenter(), client.Data);
            var size = 10f;
            var square = new Vector2[]
            {
                center + Vector2.Left * size / 2f + Vector2.Up * size / 2f,
                center - Vector2.Left * size / 2f + Vector2.Up * size / 2f,
                center - Vector2.Left * size / 2f - Vector2.Up * size / 2f,
                center + Vector2.Left * size / 2f - Vector2.Up * size / 2f
            };
            // mb.DrawPolygon(square, color);
            // Add(square, () => Open(cell, client));
        }
    }


    private CombatGraph GetHistory(Client client)
    {
        var tick = client.Data.BaseDomain.GameClock.Tick;
        var histories = client.Data.Military.CombatHistories.Value;
        var mb = new MeshBuilder();
        if (histories.Graphs.TryGetValue(tick, out var history))
        {
            return history;
        }
        else if (histories.Graphs.TryGetValue(tick - 1, out var prevHistory))
        {
            return prevHistory;
        }

        return null;
    }
    private void Open(Cell cell, Client client)
    {
        CellCombatHistoryWindow.Open(cell, GetHistory(client), client);
    }
}
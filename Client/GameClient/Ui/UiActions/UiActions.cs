
using System;
using System.Linq;
using Godot;

public static class UiActions
{
    public static void TryOpenRegimeOverview(this Client client, 
        Cell cell)
    {
        if (cell == null)
        {
            throw new Exception();
        }
        if (cell.Controller.Fulfilled())
        {
            var r = cell.Controller.Get(client.Data);
            var w = Game.I.Client.GetComponent<WindowManager>().OpenWindow<RegimeOverviewWindow>();
            w.Setup(r, client);
        }
    }
    
    public static void HighlightPoly(this Client client, MapPolygon poly, float thickness)
    {
        var highlighter = client.GetComponent<MapGraphics>().Highlighter;
        if (poly != null)
        {
            highlighter.Draw(mb => mb.DrawPolygonOutline(
                poly.BoundaryPoints, 
                thickness, Colors.White), poly.Center);
        }
    }
    public static void HighlightCell(this Client client, Cell cell,
        float thickness)
    {
        var highlighter = client.GetComponent<MapGraphics>().Highlighter;
        if (cell != null)
        {
            highlighter.Draw(mb => mb.DrawPolygonOutline(
                cell.RelBoundary, thickness,
                Colors.White), cell.RelTo);

            foreach (var n in cell.GetNeighbors(client.Data))
            {
                highlighter.Draw(mb => mb.DrawPolygonOutline(
                    n.RelBoundary, thickness / 2f,
                    Colors.Red), n.RelTo);
            }
        }
    }
}
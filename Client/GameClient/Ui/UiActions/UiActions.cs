
using System;
using System.Linq;
using Godot;

public static class UiActions
{
    public static void TryOpenRegimeOverview(this Client client, MapPolygon poly)
    {
        if (poly == null)
        {
            throw new Exception();
        }
        if (poly.OwnerRegime.Fulfilled())
        {
            var r = poly.OwnerRegime.Entity(client.Data);
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
                .25f, Colors.White), poly.Center);
        }
    }
    public static void HighlightCell(this Client client, PolyCell cell,
        float thickness)
    {
        var highlighter = client.GetComponent<MapGraphics>().Highlighter;
        if (cell != null)
        {
            highlighter.Draw(mb => mb.DrawPolygonOutline(
                cell.RelBoundary, 1f,
                Colors.White), cell.RelTo);

            foreach (var n in cell.GetNeighbors(client.Data))
            {
                highlighter.Draw(mb => mb.DrawPolygonOutline(
                    n.RelBoundary, .25f,
                    Colors.Red), n.RelTo);
            }
        }
    }
}
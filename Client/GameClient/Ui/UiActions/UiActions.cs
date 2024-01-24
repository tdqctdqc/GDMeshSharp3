
using System;
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
    
    public static void HighlightPoly(this Client client, MapPolygon poly)
    {
        var highlighter = client.GetComponent<MapGraphics>().Highlighter;
        if (poly != null)
        {
            highlighter.Draw(mb => mb.DrawPolyBorders(poly.Center, poly, client.Data), poly.Center);
        }
    }
    public static void HighlightCell(this Client client, PolyCell cell)
    {
        var highlighter = client.GetComponent<MapGraphics>().Highlighter;
        if (cell != null)
        {
            highlighter.Draw(mb => mb.DrawCellBorders(cell.GetCenter(), cell, client.Data), cell.GetCenter());
        }
    }
}
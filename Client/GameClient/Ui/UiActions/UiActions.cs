
using System;
using System.Linq;
using Godot;

public static class UiActions
{
    public static void HighlightPoly(this Client client, 
        MapPolygon poly, 
        MapOverlayDrawer overlay,
        float thickness)
    {
        if (poly != null)
        {
            overlay.Draw(mb => mb.DrawPolygonOutline(
                poly.BoundaryPoints, 
                thickness, Colors.White), poly.Center);
        }
    }
    public static void HighlightCell(this Client client, 
        Cell cell,
        MapOverlayDrawer overlay,
        float thickness)
    {
        if (cell != null)
        {
            overlay.Draw(mb => mb.DrawPolygonOutline(
                cell.RelBoundary, thickness,
                Colors.White), cell.RelTo);

        }
    }

    public static void HighlightCellNeighbors(this Client client, 
        Cell cell,
        MapOverlayDrawer overlay,
        float thickness)
    {
        foreach (var n in cell.GetNeighbors(client.Data))
        {
            overlay.Draw(mb => mb.DrawPolygonOutline(
                n.RelBoundary, thickness,
                Colors.White.Tint(.5f)), n.RelTo);
        }
    }
}
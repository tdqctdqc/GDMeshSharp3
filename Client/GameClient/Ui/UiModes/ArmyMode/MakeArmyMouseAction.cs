using System;
using Godot;

namespace Ui.ArmyMode;

public class MakeArmyMouseAction : CellMousePressAction
{
    public MakeArmyMouseAction(
        MouseOverHandler mouseOverHandler, 
        Client client) 
        : base(MouseButtonMask.Right, 
            mouseOverHandler, c => Valid(c, client))
    {
        MouseReleased += cell =>
        {
            var localPlayer = client.Data.BaseDomain.PlayerAux.LocalPlayer;
            var command = new CreateArmyCommand(cell.MakeRef(), localPlayer.PlayerGuid);
            client.HandleCommand(command);
        };
    }

    private static bool Valid(Cell c, Client client)
    {
        if (c is LandCell == false) return false;
        var localPlayer = client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var localAlliance = localPlayer.Regime.Get(client.Data).GetAlliance(client.Data);
        return localAlliance.Members.Contains(c.Controller);
    }
}
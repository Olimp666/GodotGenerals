using Godot;
using SharedObjects.Network;
using System;

public partial class Global : Node
{
    public string IPAddress;
    public string Port;
    public string PlayerName;
    public NetworkClient NetworkClient;
    public bool IsOverHUD()
    {
        Rect2 hud = GetNode<PanelContainer>("/root/RootNode/HUD/CellInfo").GetGlobalRect();
        var mousePos = GetViewport().GetMousePosition();
        return hud.HasPoint(mousePos);
    }
}

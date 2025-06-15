using Godot;
using SharedObjects.Network;
using System;

namespace MainMenu;
public partial class MainMenu : Control
{
    private TextEdit nameInput;
    private TextEdit IPInput;
    private TextEdit portInput;

    public override void _Ready()
    {
        IPInput = GetNode<TextEdit>("VBoxContainer/IPEdit");
        portInput = GetNode<TextEdit>("VBoxContainer/PortEdit");
        nameInput = GetNode<TextEdit>("VBoxContainer/PlayerNameEdit");

        GetNode<Button>("VBoxContainer/StartButton").Pressed += OnButtonPressed;
    }
    private void OnButtonPressed()
    {
        string playerName = nameInput.Text;
        string ip = IPInput.Text;
        string port = portInput.Text;
        NetworkClient netClient = new NetworkClient();

        Global globals = GetNode<Global>("/root/Global");

        globals.NetworkClient = netClient;
        globals.PlayerName = playerName;
        globals.IPAddress = ip;
        globals.Port = port;
        GetNode<Button>("VBoxContainer/StartButton").Disabled = true;

        GetTree().ChangeSceneToFile("res://Scenes/tile_map.tscn");
    }
}

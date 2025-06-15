using Godot;
using SharedObjects.Network;
using System;

public partial class ExitButton : TextureButton
{
    public override void _Ready()
	{
        Pressed += OnButtonPressed;
	}
    private void OnButtonPressed()
    {
        Global globals = GetNode<Global>("/root/Global");

        globals.NetworkClient.Disconnect();
        globals.NetworkClient = null;

        GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
    }
}

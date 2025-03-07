using System.Threading;
using System;
using Godot;
using LiteNetLib;
using LiteNetLib.Utils;
using SharedObjects.Network;
using SharedObjects.GameObjects;

public partial class Client : Node
{
    NetworkClient networkClient;
    public override void _Ready()
    {
        Console.WriteLine("Hello, World!");


        Console.Write("Please provide player name: ");

        const string playerName = "Godot";
        networkClient = new NetworkClient();
        networkClient.Connect(playerName);
    }
    public override void _Process(double delta)
    {
        networkClient.Update();
        networkClient.OnGameStateUpdateEvent += UpdateGameState;

    }
    private void UpdateGameState(PlayerReceiveUpdatePacket packet)
    {
        TileMap parentNode = (TileMap)GetParent();
        int idx = 0;
        foreach (var cell in packet.state.Grid.cells)
        {
            Vector2I v = new Vector2I(cell.XCoord, cell.YCoord);
            Vector2I atlasCoords = new Vector2I(2, 0);
            parentNode.SetCell(idx, v, 0,atlasCoords);
        }
    }
}

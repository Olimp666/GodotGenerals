using Godot;
using SharedObjects.Network;
using System;
using System.Diagnostics;

namespace TileMap;
public partial class RootNode : Node
{
    Client client;
    TileMapLayer mapLayer;
    public override void _Ready()
    {
        client = GetNode<Client>(new NodePath("ClientNode"));
        mapLayer = GetNode<TileMapLayer>(new NodePath("TileMapLayer"));

        Debugger.Log(0, "Msg", "Root node initialized");
        EmitSignal(SignalName.Ready);
    }

    public TileMapLayer GetMapLayer()
    {
        return mapLayer;
    }
    public Client GetClientNode()
    {
        return client;
    }
}

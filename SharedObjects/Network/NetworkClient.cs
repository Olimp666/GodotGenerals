using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using SharedObjects.Commands;
using SharedObjects.GameObjects;

namespace SharedObjects.Network;


public class NetworkClient : INetEventListener {
    public static readonly NetworkClient Instance = new();

    public bool IsPLayerJoined;

    private NetManager _client;
    private NetPeer _server;

    private NetDataWriter writer;
    private NetPacketProcessor packetProcessor;
    private ClientPlayer clientPlayer;
    private Dictionary<int, ServerPlayer> serverPlayers = new();
    public Queue<ICommand> commands = new();
    public string LastPackageType { get; set; } = "";
    public DateTime LastPackageTimeStamp { get; set; } = new(0);

    public delegate void JoinHandler(JoinAcceptPacket packet);
    public event JoinHandler OnJoinEvent = (packet) => {};

    public delegate void GameStateUpdateHandler(PlayerReceiveUpdatePacket packet);
    public event GameStateUpdateHandler OnGameStateUpdateEvent = (packet) => {};

    public List<ServerPlayer> GetServerPlayers() {
        return serverPlayers.Values.ToList();
    }

    public ClientPlayer GetClientPlayer() {
        return clientPlayer;
    }

    public void Connect(string username) {
        clientPlayer.username = username;
        writer = new NetDataWriter();
        packetProcessor = new NetPacketProcessor();

        packetProcessor.RegisterNestedType(() => new GameState());
        packetProcessor.RegisterNestedType<ClientPlayer>();
        packetProcessor.RegisterNestedType<MoveCommand>();
        packetProcessor.RegisterNestedType<OrderUnitCommand>();


        packetProcessor.SubscribeReusable<JoinAcceptPacket>(OnJoinAccept);
        packetProcessor.SubscribeReusable<PlayerJoinedGamePacket>(OnPlayerJoin);
        packetProcessor.SubscribeReusable<PlayerReceiveUpdatePacket>(OnReceiveUpdate);
        packetProcessor.SubscribeReusable<PlayerLeftGamePacket>(OnPlayerLeave);
        packetProcessor.SubscribeReusable<PlayerAwaitPacket>(OnPlayerAwait);

        _client = new NetManager(this) { AutoRecycle = true };

        _client.Start();
        Console.WriteLine("Connecting to server");
        _client.Connect("localhost", 12345, "");
    }

    public void Disconnect() {
        _client.Stop();
    }

    public void OnPeerConnected(NetPeer peer) {
        Console.WriteLine("Connected to server");
        _server = peer;

        SendPacket(
            new JoinPacket { username = clientPlayer.username }, DeliveryMethod.ReliableOrdered
        );
    }

    public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new() {
        Console.WriteLine($"Trying to send packet: {packet.GetType()}");
        writer.Reset();
        packetProcessor.Write(writer, packet);
        _server.Send(writer, deliveryMethod);
    }

    public void OnJoinAccept(JoinAcceptPacket packet) {
        Console.WriteLine($"Join accepted by server (pid: {packet.state})");
        OnJoinEvent.Invoke(packet);
        // TODO!!!
        // gameState = packet.state;
        // clientPlayer = packet.player; 
        IsPLayerJoined = true;
        
    }

    public void OnReceiveUpdate(PlayerReceiveUpdatePacket packet) {
        OnGameStateUpdateEvent.Invoke(packet);
    }

    public void OnPlayerJoin(PlayerJoinedGamePacket packet) {
        Console.WriteLine(
            $"Player '{packet.player.username}' (pid: {packet.player.playerId}) joined the game"
        );
        ServerPlayer newServerPlayer =
            new ServerPlayer
                { username = packet.player.username, playerId = packet.player.playerId };
        serverPlayers.Add(newServerPlayer.playerId, newServerPlayer);
    }   

    public void OnPlayerLeave(PlayerLeftGamePacket packet) {
        Console.WriteLine($"Player (pid: {packet.playerId}) left the game");
        serverPlayers.Remove(packet.playerId);
    }

    public void Update() {
        _client.PollEvents();

        if (commands.Count != 0) {
            BaseCommandPacket packet = commands.Dequeue().ToPacket();

            if (packet.GetType() == typeof(MoveCommandPacket))
                SendPacket((MoveCommandPacket)packet, DeliveryMethod.ReliableOrdered);

            if (packet.GetType() == typeof(OrderUnitPacket))
                SendPacket((OrderUnitPacket)packet, DeliveryMethod.ReliableOrdered);
        }
    }

    public void OnPlayerAwait(PlayerAwaitPacket packet) { }

    public void OnConnectionRequest(ConnectionRequest request) { }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber,
        DeliveryMethod deliveryMethod) {
        packetProcessor.ReadAllPackets(reader);
        LastPackageTimeStamp = DateTime.Now;
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
        UnconnectedMessageType messageType) { }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) { }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }

}

using LiteNetLib;
using SharedObjects.GameObjects;

namespace SharedObjects.Network;

public class JoinPacket {
    public string username { get; set; }
}

public class JoinAcceptPacket {
    public GameState state { get; set; }
    public ClientPlayer player { get; set; }
}

public class ServerPlayer {
    public NetPeer peer;
    public string username;
    public int playerId;
}



public class PlayerReceiveUpdatePacket {
    public GameState state { get; set; }
}

public class PlayerJoinedGamePacket {
    public ClientPlayer player { get; set; }
}

public class PlayerLeftGamePacket {
    public int playerId { get; set; }
}

public class PlayerAwaitPacket { }


using LiteNetLib.Utils;

namespace SharedObjects.Network;

public struct ClientPlayer : INetSerializable {
    public int playerId;
    public string username;

    public void Serialize(NetDataWriter writer) {
        writer.Put(playerId);
        writer.Put(username);
    }

    public void Deserialize(NetDataReader reader) {
        playerId = reader.GetInt();
        username = reader.GetString();
    }
}
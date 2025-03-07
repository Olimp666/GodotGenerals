using System;
using LiteNetLib.Utils;
using SharedObjects.Network;

namespace SharedObjects.Commands;

public struct MoveCommand : ICommand {
    public int unitId;
    public int x;
    public int y;

    public void Serialize(NetDataWriter writer) {
        writer.Put(unitId);
        writer.Put(x);
        writer.Put(y);
    }

    public void Deserialize(NetDataReader reader) {
        unitId = reader.GetInt();
        x = reader.GetInt();
        y = reader.GetInt();
    }

    public BaseCommandPacket ToPacket() {
        return new MoveCommandPacket { Command = this };
    }

    public void Execute() {
        throw new NotImplementedException();
    }
}

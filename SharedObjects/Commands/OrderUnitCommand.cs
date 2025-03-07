using LiteNetLib.Utils;
using SharedObjects.GameObjects.Units;
using SharedObjects.Network;

namespace SharedObjects.Commands;


public struct OrderUnitCommand(int userId, UnitType unitType) : ICommand {
    public int UserId = userId;
    public UnitType UnitType = unitType;

    public void Serialize(NetDataWriter writer) {
        writer.Put(UserId);
        writer.Put((int)UnitType);
    }

    public void Deserialize(NetDataReader reader) {
        UserId = reader.GetInt();
        UnitType = (UnitType)reader.GetInt();
    }

    public BaseCommandPacket ToPacket() {
        return new OrderUnitPacket { Command = this };
    }

    public void Execute() {
        throw new System.NotImplementedException();
    }
}

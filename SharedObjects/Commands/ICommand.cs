using LiteNetLib.Utils;
using SharedObjects.Network;

namespace SharedObjects.Commands;
    
public interface ICommand : INetSerializable {
    public BaseCommandPacket ToPacket();
    
    public void Execute();
}
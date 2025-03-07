
using SharedObjects.Commands;

namespace SharedObjects.Network;

public abstract class BaseCommandPacket { 
}


public class MoveCommandPacket : BaseCommandPacket {
    public MoveCommand Command { get; set; }
}

public class OrderUnitPacket : BaseCommandPacket {
    public OrderUnitCommand Command { get; set; }
}

using LiteNetLib.Utils;

namespace SharedObjects.GameObjects.Orders;

public interface IOrder : INetSerializable {
    OrderStatus Update(GameState state);
    public OrderType OrderType { get; }
}

public enum OrderStatus {
    Executing,
    Finished
}

public enum OrderType {
    Move,
}
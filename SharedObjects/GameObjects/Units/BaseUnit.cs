using LiteNetLib.Utils;
using SharedObjects.GameObjects.Orders;

namespace SharedObjects.GameObjects.Units;



public abstract class BaseUnit(int unitId, int playerId, int x, int y, string nickname)
    : INetSerializable {
    public int Health { get; set; }
    public string Nickname { get; } = nickname;
    public virtual UnitType Type { get; set; }
    public bool CanMove { get; set; }
    public bool CanAttack { get; set; }
    public bool HasAbility { get; set; }
    public int UnitId { get; set; } = unitId;
    public int PlayerId { get; set; } = playerId;
    public float MovementSpeed { get; set; }
    public float AttackSpeed { get; set; }
    public int AttackDamage { get; set; }
    public int VisibleRadius { get; set; }
    public IOrder? CurrentOrder { get; }
    public int X { get; set; } = x;
    public int Y { get; set; } = y;


    public void UpdatePosition(HexCell newPosition) {
        X = newPosition.XCoord;
        Y = newPosition.YCoord;
        newPosition.CellUnitId = unitId;
    }

    public static BaseUnit CreateUnitByType(
        int unitType, int unitId, int ownerId, int x, int y, string nickname
        ) {
        if (unitType == 0) {
            return new InfantryUnit(unitId, ownerId, x, y);
        }

        if (unitType == 1) {
            return new ArtilleryUnit(unitId, ownerId, x, y);
        }

        return new InfantryUnit(unitId, ownerId, x, y);
    }

    public void Serialize(NetDataWriter writer) {
        writer.Put((int)Type);
        writer.Put(UnitId);
        writer.Put(PlayerId);
        writer.Put(Health);
        writer.Put(X);
        writer.Put(Y);
    }

    public virtual void Deserialize(NetDataReader reader) {
        Type = (UnitType)reader.GetInt();
        UnitId = reader.GetInt();
        PlayerId = reader.GetInt();
        Health = reader.GetInt();
        X = reader.GetInt();
        Y = reader.GetInt();
    }
}

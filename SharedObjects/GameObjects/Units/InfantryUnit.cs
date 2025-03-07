
namespace SharedObjects.GameObjects.Units;

public class InfantryUnit : BaseUnit {
    public InfantryUnit(int unitId = 0, int playerId = 0, int x = 0, int y = 0, string nickname = "") : base(
        unitId, playerId, x, y, nickname
    ) {
        CanMove = true;
        CanAttack = true;
        HasAbility = false;
        Health = 100;
        MovementSpeed = 5f;
        AttackSpeed = 1;
        AttackDamage = 20;
        VisibleRadius = 3;
    }

    public override UnitType Type => UnitType.InfantryUnit;
}

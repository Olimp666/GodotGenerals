
namespace SharedObjects.GameObjects.Units;

public class AirUnit : BaseUnit {
    public AirUnit(
        int unitId = 0, int playerId = 0, int x = 0, int y = 0, string nickname = "") : base(
        unitId, playerId, x, y, nickname
    ) {
        CanMove = false;
        CanAttack = false;
        HasAbility = true;
        Health = 250;
        MaxHealth = 250;
        MovementSpeed = 0.5f;
        AttackSpeed = 3;
        AttackDamage = 50;
        VisibleRadius = 2;
    }


    public void AttackCell(GameState state, HexCell cell) {
        // cell.GetCellUnit().Health -= 100;
    }

    public override UnitType Type => UnitType.AirUnit;
}

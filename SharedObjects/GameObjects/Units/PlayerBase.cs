namespace SharedObjects.GameObjects.Units;

public class PlayerBase : BaseUnit {
    public PlayerBase(
        int unitId = 0, int playerId = 0, int x = 0, int y = 0, string nickname = ""
    ) : base(unitId, playerId, x, y, nickname) {
        CanMove = false;
        CanAttack = true;
        MovementSpeed = 0;
        AttackSpeed = 1;
        AttackDamage = 10;
        VisibleRadius = 1;
    }

    public override UnitType Type => UnitType.PlayerBase;
}

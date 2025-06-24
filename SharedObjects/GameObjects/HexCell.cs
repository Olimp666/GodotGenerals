using System;
using System.Collections.Generic;
using LiteNetLib.Utils;
using SharedObjects.GameObjects.Units;

namespace SharedObjects.GameObjects;

public class HexCell(int xCoord = 1, int yCoord = 1) : INetSerializable
{
    public List<int> CellUnitIds = [];
    public int XCoord = xCoord;
    public int YCoord = yCoord;

    //Cost from start cell to current
    public int g;

    //Cost from current cell to destination tile
    public int h;
    public int F => g + h;
    public string Name => $"Координаты {YCoord} {XCoord}";

    private readonly object _lock = new object();

    public void ProcessFight(GameState gs, TimeSpan timeDelta)
    {
        lock (_lock)
        {
            List<BaseUnit> p1Units = [];
            List<BaseUnit> p2Units = [];
            List<int> deadUnitsIds = [];
            foreach (int unitId in CellUnitIds)
            {
                var curUnit = gs.GetUnitById(unitId);
                if (curUnit.PlayerId == 0)
                    p1Units.Add(curUnit);
                else
                    p2Units.Add(curUnit);
            }
            AttackEnemies(p1Units, p2Units, deadUnitsIds, timeDelta);
            foreach (var deadId in deadUnitsIds)
            {
                RemoveCellUnit(deadId);
                gs.RemoveUnit(deadId);
            }
        }
    }

    private void AttackEnemies(List<BaseUnit> allies, List<BaseUnit> enemies, List<int> dead, TimeSpan timeDelta)
    {
        int p = 0;
        for (int i = 0; p < enemies.Count && i < allies.Count; i++)
        {
            var curUnit = allies[i];
            var curEnemy = enemies[p];
            if (curUnit.OnCooldown)
            {
                curUnit.Elapsed += timeDelta;
                if (curUnit.Elapsed >= TimeSpan.FromSeconds(curUnit.AttackSpeed))
                {
                    curUnit.Elapsed = TimeSpan.Zero;
                    curUnit.OnCooldown = false;
                }
            }
            else
            {
                curEnemy.Health -= curUnit.AttackDamage;
                curUnit.OnCooldown = true;
            }

            if (curEnemy.Health <= 0)
            {
                dead.Add(curEnemy.UnitId);
                p++;
            }
        }
    }

    public void UpdateCellUnit(int unitId)
    {
        lock (_lock)
        {
            CellUnitIds.Add(unitId);
        }
    }

    public void RemoveCellUnit(int unitId)
    {
        lock (_lock)
        {
            CellUnitIds.Remove(unitId);
        }
    }

    public void Serialize(NetDataWriter writer)
    {
        lock (_lock)
        {
            writer.Put(CellUnitIds.Count);
            for (int i = 0; i < CellUnitIds.Count; i++)
            {
                writer.Put(CellUnitIds[i]);
            }
            writer.Put(XCoord);
            writer.Put(YCoord);
            writer.Put(g);
            writer.Put(h);
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        lock (_lock)
        {
            int cnt = reader.GetInt();
            for (int i = 0; i < cnt; i++)
            {
                CellUnitIds.Add(reader.GetInt());
            }
            XCoord = reader.GetInt();
            YCoord = reader.GetInt();
            g = reader.GetInt();
            h = reader.GetInt();
        }
    }
}

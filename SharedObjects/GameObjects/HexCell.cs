using System.Collections.Generic;
using LiteNetLib.Utils;

namespace SharedObjects.GameObjects;

public class HexCell(int xCoord = 1, int yCoord = 1) : INetSerializable {
    public List<int> CellUnitIds = [];
    public int XCoord = xCoord;
    public int YCoord = yCoord;

    //Cost from start cell to current
    public int g;

    //Cost from current cell to destination tile
    public int h;
    public int F => g + h;
    public string Name => $"Координаты {YCoord} {XCoord}";

    public void UpdateCellUnit(int unitId) {
        CellUnitIds.Add(unitId);
    }

    public void RemoveCellUnit(int unitId) {
        CellUnitIds.Remove(unitId);
    }

    public void Serialize(NetDataWriter writer) {
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

    public void Deserialize(NetDataReader reader) {
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

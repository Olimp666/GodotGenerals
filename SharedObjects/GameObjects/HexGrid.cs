using System;
using LiteNetLib.Utils;

namespace SharedObjects.GameObjects;

public class HexGrid : INetSerializable {
    public int Width;
    public int Height;

    public HexCell[,] cells;

    public HexGrid(int gridHeight, int gridWidth) {
        Height = gridHeight;
        Width = gridWidth;
        cells = new HexCell[Height, Width];

        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                cells[y, x] = new HexCell(x, y);
            }
        }
    }

    public HexCell GetRandomCell() {
        Random random = new Random();
        return cells[random.Next(0, Height), random.Next(0, Width)];
    }

    public HexCell GetCell(int x, int y) {
        return cells[y, x];
    }

    public void Serialize(NetDataWriter writer) {
        writer.Put(Height);
        writer.Put(Width);
        for (var y = 0; y < Height; y++) {
            for (var x = 0; x < Width; x++) {
                cells[y, x].Serialize(writer);
            }
        }
    }

    public void Deserialize(NetDataReader reader) {
        Height = reader.GetInt();
        Width = reader.GetInt();
        cells = new HexCell[Height, Width];

        for (var y = 0; y < Height; y++) {
            for (var x = 0; x < Width; x++) {
                cells[y, x] = reader.Get(() => new HexCell());
            }
        }
    }
}

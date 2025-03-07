using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LiteNetLib.Utils;
using SharedObjects.GameObjects.Units;

namespace SharedObjects.GameObjects.Orders;
    public class MoveOrder(int destinationCellX = -1, int destinationCellY = -1, int unitId = -1)
        : IOrder {
        private bool isPathFound;
        private Stack<HexCell> pathCells = new();
        private DateTime t1;
        private DateTime t2;
        public OrderType OrderType => OrderType.Move;
        public int UnitId = unitId;

        public OrderStatus Update(GameState state) {
            t1 = DateTime.Now;
            if (!isPathFound) {
                t2 = t1.AddSeconds(1);
                pathCells = GetPath(state);
            }

            BaseUnit currentUnit = state.GetUnitById(UnitId);
            HexCell currentCell =
                state.Grid.cells[currentUnit.Y, currentUnit.X];

            if (pathCells.Count == 0) {
                return OrderStatus.Finished;
            }

            if (t1 > t2) {
                t2 = DateTime.Now.AddSeconds(1);
                currentCell.RemoveCellUnit();
                currentCell = pathCells.Pop();
                currentUnit.UpdatePosition(currentCell);
            }

            return OrderStatus.Executing;
        }


        private Stack<HexCell> GetPath(GameState state) {
            isPathFound = true;
            List<HexCell> openPathCells = [];
            List<HexCell> closedPathCells = [];

            BaseUnit currentUnit = state.GetUnitById(UnitId);
            HexCell destinationCell = state.Grid.cells[destinationCellY, destinationCellX];
            HexCell currentCell =
                state.Grid.cells[currentUnit.Y, currentUnit.X];

            currentCell.g = 0;
            currentCell.h = GetPathCost(
                new Vector2(currentCell.XCoord, currentCell.YCoord),
                new Vector2(destinationCell.XCoord, destinationCell.YCoord)
            );

            openPathCells.Add(currentCell);

            while (openPathCells.Count != 0) {
                openPathCells = openPathCells.OrderBy(x => x.F).ThenByDescending(x => x.g).ToList();
                currentCell = openPathCells[0];

                openPathCells.Remove(currentCell);
                closedPathCells.Add(currentCell);

                int g = currentCell.g + 1;

                if (closedPathCells.Contains(destinationCell)) {
                    break;
                }

                foreach (var neighborCell in FindNeighbors(currentCell)
                             .Where(neighborCell => !closedPathCells.Contains(neighborCell))) {
                    if (!openPathCells.Contains(neighborCell)) {
                        neighborCell.g = g;
                        neighborCell.h = GetPathCost(
                            new Vector2(neighborCell.XCoord, neighborCell.YCoord),
                            new Vector2(destinationCell.XCoord, destinationCell.YCoord)
                        );
                        openPathCells.Add(neighborCell);
                    } else if (neighborCell.F > g + neighborCell.h) {
                        neighborCell.g = g;
                    }
                }
            }

            Stack<HexCell> path = new();

            if (closedPathCells.Contains(destinationCell)) {
                currentCell = destinationCell;
                path.Push(currentCell);

                for (int i = destinationCell.g - 1; i >= 0; i--) {
                    currentCell =
                        closedPathCells.Find(
                            cell => cell.g == i && FindNeighbors(currentCell).Contains(cell)
                        );
                    path.Push(currentCell);
                }

                path.Pop();
            }

            return path;

            int GetPathCost(Vector2 start, Vector2 end) {
                float z1 = -(start.X + start.Y);
                float z2 = -(end.X + end.Y);
                int localMax = int.Max(
                    int.Abs((int)start.X - (int)end.X), int.Abs((int)start.Y - (int)end.Y)
                );
                return int.Max(localMax, int.Abs((int)z1 - (int)z2));
            }

            List<HexCell> FindNeighbors(HexCell cell) {
                List<HexCell> neighbors = [];

                AddNeighbor(Offset(new Vector2(cell.XCoord, cell.YCoord), 1, 0));
                AddNeighbor(Offset(new Vector2(cell.XCoord, cell.YCoord), 1, -1));
                AddNeighbor(Offset(new Vector2(cell.XCoord, cell.YCoord), 0, -1));
                AddNeighbor(Offset(new Vector2(cell.XCoord, cell.YCoord), -1, 0));
                AddNeighbor(Offset(new Vector2(cell.XCoord, cell.YCoord), -1, 1));
                AddNeighbor(Offset(new Vector2(cell.XCoord, cell.YCoord), 0, 1));

                return neighbors;

                Vector2 Offset(Vector2 coords, int ox, int oy) {
                    return new Vector2(coords.X + ox, coords.Y + oy);
                }

                void AddNeighbor(Vector2 coords) {
                    for (var i = 0; i < state.Grid.Height; i++) {
                        for (var j = 0; j < state.Grid.Width; j++) {
                            if (new Vector2(
                                    state.Grid.cells[i, j].XCoord, state.Grid.cells[i, j].YCoord
                                ) == coords) {
                                neighbors.Add(state.Grid.cells[i, j]);
                            }
                        }
                    }
                }
            }
        }

        public void Serialize(NetDataWriter writer) {
            writer.Put(UnitId);
            writer.Put(destinationCellX);
            writer.Put(destinationCellY);
            writer.Put(isPathFound);
            writer.Put(pathCells.Count);
            //TODO: collection was modified
            foreach (HexCell pathCell in pathCells) {
                pathCell.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader) {
            UnitId = reader.GetInt();
            destinationCellX = reader.GetInt();
            destinationCellY = reader.GetInt();
            isPathFound = reader.GetBool();
            int pathCellsCount = reader.GetInt();
            pathCells = new Stack<HexCell>();
            for (int i = 0; i < pathCellsCount; i++) {
                pathCells.Push(reader.Get(() => new HexCell()));
            }
        }
    }

using System.Threading;
using System;
using Godot;
using LiteNetLib;
using LiteNetLib.Utils;
using SharedObjects.Network;
using SharedObjects.GameObjects;
using System.Diagnostics;
using SharedObjects.Commands;
using Godot.Collections;
using SharedObjects.GameObjects.Units;
using System.Net.Sockets;
using System.Collections.Generic;
using System.ComponentModel;
using SharedObjects.GameObjects.Orders;
using System.Linq;

namespace TileMap;
public partial class Client : Node
{
    Global globals;

    GameState gs;

    VBoxContainer cellInfo;

    RootNode root;
    Camera2D camera;
    TileMapLayer borderLayer, bgLayer, iconLayer, fogLayer;
    NetworkClient netClient;

    CellInfoBase? currentCellInfoItem = null;

    Vector2I? selectedCell = null;
    bool orderMode = false;

    System.Collections.Generic.Dictionary<UnitType, Vector2I> unitTypeMap = new()
    {
        { UnitType.InfantryUnit,new Vector2I(0,0) },
        { UnitType.AirUnit,new Vector2I(1,0) },
        { UnitType.ArtilleryUnit,new Vector2I(2,0) },
        { UnitType.PlayerBase,new Vector2I(3,0) },
    };
    public override async void _Ready()
    {
        root = GetParent<RootNode>();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _OnParentReady();
    }
    void _OnParentReady()
    {
        globals = GetNode<Global>("/root/Global");

        camera = GetNode<Camera2D>("/root/RootNode/Camera2D");
        cellInfo = GetNode<VBoxContainer>("/root/RootNode/HUD/CellInfo/ScrollContainer/VBox");

        borderLayer = GetNode<TileMapLayer>("/root/RootNode/BorderTileLayer");
        bgLayer = GetNode<TileMapLayer>("/root/RootNode/BackgroundTileLayer");
        iconLayer = GetNode<TileMapLayer>("/root/RootNode/IconTileLayer");
        fogLayer = GetNode<TileMapLayer>("/root/RootNode/FogOfWarLayer");

        borderLayer.EmitSignal(SignalName.Ready);

        //Random rnd = new Random();
        //for (int x = 0; x < 10; x++)
        //{
        //    for (int y = 0; y < 10; y++)
        //    {
        //        Vector2I v = new Vector2I(x, y);
        //        Vector2I atlasCoords = new Vector2I(1, 0);

        //        bgLayer.SetCell(v, 0, new Vector2I(rnd.Next(1, 5), 0));
        //        borderLayer.SetCell(v, 1, new Vector2I(1, 0));
        //        iconLayer.SetCell(v, 2, new Vector2I(rnd.Next(1, 4), 0));
        //    }
        //}



        string playerName = globals.PlayerName;
        netClient = globals.NetworkClient;
        netClient.Connect(playerName);
        netClient.OnGameStateUpdateEvent += UpdateGameState;
    }
    public override void _Process(double delta)
    {
        netClient.Update();
        UpdateCellInfo();
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        {
            Viewport viewport = GetViewport();
            Vector2 mousePos = viewport.GetMousePosition();
            mousePos -= viewport.GetWindow().Size / 2; //adjust position by window size
            mousePos /= camera.Zoom; //adjust position by camera zoom
            mousePos += camera.Position; //adjust position by camera coords
            Vector2I tileCoords = borderLayer.LocalToMap(mousePos);

            if (borderLayer.GetCellTileData(tileCoords) == null || globals.IsOverHUD())
            {
                return;
            }
            if (orderMode)
            {
                int X = selectedCell.Value.X;
                int Y = selectedCell.Value.Y;
                if (X == tileCoords.X && Y == tileCoords.Y)
                    return;

                int unitId = currentCellInfoItem.UnitId;

                orderMode = false;
                //currentActionButton.ToggleMode = false;
                currentCellInfoItem = null;

                MoveCommand moveCommand = new MoveCommand()
                {
                    unitId = unitId,
                    x = tileCoords.X,
                    y = tileCoords.Y
                };
                netClient.commands.Enqueue(moveCommand);
            }
            else
            {
                if (selectedCell != null)
                {
                    borderLayer.SetCell((Vector2I)selectedCell, 1, new Vector2I(1, 0));
                }
                borderLayer.SetCell((Vector2I)tileCoords, 1, new Vector2I(4, 0));
                selectedCell = (Vector2I?)tileCoords;
            }

        }
    }

    #region Updates
    private void UpdateCellInfo()
    {
        if (selectedCell == null)
            return;
        int X = selectedCell.Value.X;
        int Y = selectedCell.Value.Y;
        var cellUnits = new System.Collections.Generic.Dictionary<int, Vector2I>();

        //TODO as list
        List<int> unitIds = gs.Grid.cells[Y, X].CellUnitIds;
        List<int> registeredUnitIds = [];
        foreach (var unitId in unitIds)
        {
            cellUnits[unitId] = selectedCell.Value;
        }

        for (int i = cellInfo.GetChildCount() - 1; i >= 0; i--)
        {
            CellInfoBase cellInfoItem = cellInfo.GetChild<CellInfoBase>(i);
            int cellInfoX = cellInfoItem.unitCoords.X;
            int cellInfoY = cellInfoItem.unitCoords.Y;
            int cellInfoUnitId = cellInfoItem.UnitId;
            if (!cellUnits.ContainsKey(cellInfoUnitId) || !(cellInfoX == X && cellInfoY == Y))
            {
                cellInfoItem.MoveButton.Toggled -= cellInfoItem.Handler;
                cellInfoItem.QueueFree();
            }
            else
            {
                registeredUnitIds.Add(cellInfoUnitId);
            }
        }
        foreach (var unitId in unitIds)
        {
            var currentUnit = gs.GetUnitById(unitId);

            if (registeredUnitIds.Contains(unitId))
            {
                //Update units HP
                for (int i = cellInfo.GetChildCount() - 1; i >= 0; i--)
                {
                    CellInfoBase cellInfoItem = cellInfo.GetChild<CellInfoBase>(i);
                    if (cellInfoItem.UnitId == currentUnit.UnitId)
                        cellInfoItem.UpdateHealth(currentUnit.Health, currentUnit.MaxHealth);
                }
                continue;
            }

            //Creating a sprite using atlas coordinates
            List<Vector3I> atlCoords = [
                new Vector3I(0,currentUnit.PlayerId + 2,0),
            new Vector3I(1,1,0),
            new Vector3I(2,unitTypeMap[currentUnit.Type].X,unitTypeMap[currentUnit.Type].Y),
            ];

            //Initiate a node from a scene
            var unitInfoScene = GD.Load<PackedScene>("res://Scenes/cell_info_base.tscn");
            CellInfoBase panel = unitInfoScene.Instantiate<CellInfoBase>();
            panel.Setup(selectedCell.Value, currentUnit.Health, currentUnit.MaxHealth, unitId, atlCoords, currentUnit.Nickname);

            panel.Handler = (bool toggledOn) =>
            {
                OnMoveButtonPressed(panel, toggledOn);
            };
            panel.MoveButton.Toggled += panel.Handler;

            cellInfo.AddChild(panel);
        }
    }
    private void OnMoveButtonPressed(CellInfoBase cellItem, bool toggledOn)
    {
        orderMode = toggledOn;
        currentCellInfoItem = cellItem;
    }

    private void UpdateGameState(PlayerReceiveUpdatePacket packet)
    {
        gs = packet.state;

        foreach (var cell in gs.Grid.cells)
        {
            Vector2I v = new Vector2I(cell.XCoord, cell.YCoord);
            fogLayer.SetCell(v, 0, new Vector2I(5, 0));
        }

        foreach (var cell in gs.Grid.cells)
        {
            Vector2I v = new Vector2I(cell.XCoord, cell.YCoord);

            if (cell.CellUnitIds.Count == 0)
            {
                bgLayer.SetCell(v, 0, new Vector2I(0, 0));
                if (borderLayer.GetCellTileData(v) == null)
                    borderLayer.SetCell(v, 1, new Vector2I(1, 0));
                iconLayer.SetCell(v, sourceId: -1);
                continue;
            }

            Vector2I bgAtlasCoords = new Vector2I(6, 0);
            Vector2I iconAtlasCoords = new Vector2I(-1, -1);
            HashSet<int> playerIds = new HashSet<int>();
            foreach (var unitId in cell.CellUnitIds)
            {
                var curUnit = gs.GetUnitById(unitId);
                iconAtlasCoords = unitTypeMap[curUnit.Type];

                playerIds.Add(curUnit.PlayerId);
            }
            if (playerIds.Count == 1)
            {
                if (cell.CellUnitIds.Count > 1)
                {
                    iconAtlasCoords.X = 5;
                }
                bgAtlasCoords.X = playerIds.First() + 2;
            }
            if (playerIds.Count == 2)
                iconAtlasCoords.X = 4;

            bgLayer.SetCell(v, 0, bgAtlasCoords);
            if (borderLayer.GetCellTileData(v) == null)
                borderLayer.SetCell(v, 1, new Vector2I(1, 0));
            iconLayer.SetCell(v, 2, iconAtlasCoords);

            foreach (var uniId in cell.CellUnitIds)
            {
                var curUnit = gs.GetUnitById(uniId);
                if (curUnit.PlayerId == netClient.GetClientPlayer().playerId)
                    ClearVision(v, curUnit.VisibleRadius);
            }


        }
    }

    private Vector3I OffsetToCube(int x, int y)
    {
        int q = x;
        int r = y - (x - (x & 1)) / 2;
        int s = -q - r;
        return new Vector3I(q, r, s);
    }
    private Vector2I CubeToOffset(Vector3I cube)
    {
        int x = cube.X;
        int y = cube.Y + (x - (x & 1)) / 2;
        return new Vector2I(x, y);
    }
    private void ClearVision(Vector2I pos, int radius)
    {
        var centerCube = OffsetToCube(pos[0], pos[1]);

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = Math.Max(-radius, -dx - radius); dy <= Math.Min(radius, -dx + radius); dy++)
            {
                int dz = -dx - dy;
                var neighborCube = new Vector3I(centerCube.X + dx, centerCube.Y + dy, centerCube.Z + dz);
                var neighborOffset = CubeToOffset(neighborCube);
                fogLayer.SetCell(neighborOffset, sourceId: -1);
            }
        }
    }
    #endregion
}

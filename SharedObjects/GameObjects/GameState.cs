using System;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib.Utils;
using SharedObjects.Commands;
using SharedObjects.GameObjects.Orders;
using SharedObjects.GameObjects.Units;
using SharedObjects.TextToSpeech;

namespace SharedObjects.GameObjects;

public class GameState : INetSerializable {
    private readonly UnitVoiceDatabase voiceDatabase;
    public HexGrid Grid;
    public List<PlayerInfo> Players; //TODO serialization!
    public readonly List<InfantryUnit> InfantryUnits = [];
    public readonly List<ArtilleryUnit> ArtilleryUnits = [];
    public readonly List<AirUnit> AirUnits = [];
    public readonly List<PlayerBase> PlayerBases = [];
    public readonly List<MoveOrder> MoveOrders = [];
    private int unitIdCounter;
    public TimeSpan ElapsedGameTime { get; set; } = new(0);
    public bool GameInitiated { get; set; }
    public bool IsPaused { get; set; } = true;


    public delegate void AddVoiceRequestDelegate(VoiceRequest request);

    public AddVoiceRequestDelegate AddVoiceRequest;

    //Server only constructor!
    public GameState(UnitVoiceDatabase voiceDatabase, Preset preset) {
        this.voiceDatabase = voiceDatabase;
        Grid = new HexGrid(preset.GridHeight, preset.GridWidth);
        foreach (var info in preset.UnitsInfo) {
            AddUnit(info.unitType, info.ownerId, info.x, info.y);
        }
    }

    // Client only constructor!
    public GameState() {
        Grid = new HexGrid(0, 0);
    }


    public IEnumerable<BaseUnit> Units {
        get {
            return InfantryUnits
                .Concat(ArtilleryUnits.Cast<BaseUnit>())
                .Concat(AirUnits)
                .Concat(PlayerBases);
        }
    }

    public IEnumerable<IOrder> Orders {
        get { return MoveOrders; }
    }


    public BaseUnit GetUnitById(int id) {
        return Units.First(unit => unit.UnitId == id);
    }

    private void AddUnit(UnitType unitType, int ownerId, int x, int y) {
        var voiceData = voiceDatabase.ReserveAndGetUnitData(unitIdCounter, unitType);
        string nickname = voiceData.Nickname;
        if (unitType == UnitType.InfantryUnit)
            InfantryUnits.Add(new InfantryUnit(unitIdCounter, ownerId, x, y, nickname));
        if (unitType == UnitType.ArtilleryUnit)
            ArtilleryUnits.Add(new ArtilleryUnit(unitIdCounter, ownerId, x, y, nickname));
        if (unitType == UnitType.AirUnit)
            AirUnits.Add(new AirUnit(unitIdCounter, ownerId, x, y, nickname));
        if (unitType == UnitType.PlayerBase)
            PlayerBases.Add(new PlayerBase(unitIdCounter, ownerId, x, y, nickname));

        Grid.cells[y, x].UpdateCellUnit(unitIdCounter);
        unitIdCounter++;
    }

    public void RemoveUnit(BaseUnit unitToRemove) {
        if (unitToRemove.GetType() == typeof(InfantryUnit)) {
            InfantryUnits.Remove(unitToRemove as InfantryUnit);
        }

        if (unitToRemove.GetType() == typeof(ArtilleryUnit)) {
            ArtilleryUnits.Remove(unitToRemove as ArtilleryUnit);
        }
    }

    public void AddOrder(IOrder order) {
        if (order.GetType() == typeof(MoveOrder)) {
            MoveOrders.Add(order as MoveOrder);
        }
    }

    public void RemoveOrder(IOrder orderToRemove) {
        if (orderToRemove.GetType() == typeof(MoveOrder)) {
            var order = (MoveOrder)orderToRemove;
            MoveOrders.Remove(order);
            var unit = GetUnitById(order.UnitId);
            AddVoiceRequest(
                new VoiceRequest(
                    unit.PlayerId, unit.UnitId, VoiceRequestType.TaskFinished, 
                    [unit.Y, unit.X], order
                )
            );
        }
    }

    public void Update(TimeSpan timeDelta) {
        if (!GameInitiated | IsPaused)
            return;

        var copy = Orders.ToList();
        foreach (IOrder order in copy) {
            if (order.Update(this) == OrderStatus.Finished) {
                RemoveOrder(order);
            }
        }

        ElapsedGameTime += timeDelta;
    }

    public string GameStateAsString() {
        var answer = "";
        // for (int y = Grid.Height - 1; y >= 0; y--) {
        //     answer += " ";
        //     for (var x = 0; x < Grid.Width; x++) {
        //         var sign = " ";
        //
        //         if (Grid.cells[y, x].CellUnitId != -1)
        //             sign = GetUnitById(Grid.cells[y, x].CellUnitId).PlayerId == 0 ? "?" : "!";
        //
        //         answer += ($"{sign}");
        //     }
        //
        //     answer += ("\n" + (y % 2 == 0 ? "" : " "));
        // }
        //
        return answer;
    }

    public void Serialize(NetDataWriter writer) {
        Grid.Serialize(writer);
        writer.Put(InfantryUnits.Count);
        writer.Put(ArtilleryUnits.Count);
        writer.Put(AirUnits.Count);
        writer.Put(PlayerBases.Count);

        writer.Put(MoveOrders.Count);

        foreach (var unit in InfantryUnits)
            unit.Serialize(writer);

        foreach (var unit in ArtilleryUnits)
            unit.Serialize(writer);

        foreach (var unit in PlayerBases)
            unit.Serialize(writer);

        foreach (var order in MoveOrders) {
            order.Serialize(writer);
        }
    }

    public void Deserialize(NetDataReader reader) {
        Grid.Deserialize(reader);
        int marineCount = reader.GetInt();
        int artilleryCount = reader.GetInt();
        int airCount = reader.GetInt();
        int basesCount = reader.GetInt();
        int moveOrdersCount = reader.GetInt();
        for (var i = 0; i < marineCount; i++)
            InfantryUnits.Add(reader.Get(() => new InfantryUnit()));

        for (var i = 0; i < artilleryCount; i++)
            ArtilleryUnits.Add(reader.Get(() => new ArtilleryUnit()));

        for (var i = 0; i < airCount; i++)
            AirUnits.Add(reader.Get(() => new AirUnit()));

        for (var i = 0; i < basesCount; i++)
            PlayerBases.Add(reader.Get(() => new PlayerBase()));

        for (var i = 0; i < moveOrdersCount; i++)
            MoveOrders.Add(reader.Get(() => new MoveOrder()));
        //TODO Properly serializable orders
    }

    public void InitializeWorld() {
        GameInitiated = true;
        IsPaused = false;
    }

    public void OrderUnit(OrderUnitCommand packetCommand) {
        AddUnit(packetCommand.UnitType, packetCommand.UserId, 6, 6);
    }
}

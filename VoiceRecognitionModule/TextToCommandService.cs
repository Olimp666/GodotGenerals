using System.Diagnostics;
using System.Net.Mime;
using SharedObjects.GameObjects;
using SharedObjects.GameObjects.Orders;
using SharedObjects.GameObjects.Units;
using SharedObjects.TextToSpeech;

namespace VoiceRecognitionModule;

public class TextToCommandService {
    private readonly GameState gameState;

    private readonly UnitVoiceDatabase database;

    //Unbelievable that I have to make it by hand
    private readonly Dictionary<string, int> converter = new() {
        { "ноль", 0 },
        { "один", 1 },
        { "два", 2 },
        { "три", 3 },
        { "четыре", 4 },
        { "пять", 5 },
        { "шесть", 6 },
        { "семь", 7 },
        { "восемь", 8 },
        { "девять", 9 },
    };

    public TextToCommandService(UnitVoiceDatabase database, GameState gameState) {
        this.database = database;
        this.gameState = gameState;
    }

    public IOrder? TextToCommand(int playerId, string text) {
        var command = TryToFormMoveOrder(playerId, text);
        return command;
    }


    private MoveOrder? TryToFormMoveOrder(int playerId, string text) {
        try {
            var unitsHashSet = gameState.Units.Where(
                x => x.PlayerId == playerId
            ).Select(baseUnit => baseUnit.Nickname.ToLower()).ToHashSet();
            unitsHashSet.IntersectWith(text.Split().ToHashSet());
            var unitNickname = unitsHashSet.First();
            var cell = text.Split()
                .Where(s => converter.ContainsKey(s)).ToList()[..2];
            return new MoveOrder(
                converter[cell[0]],
                converter[cell[1]],
                gameState.Units.First(x => x.Nickname.ToLower() == unitNickname).UnitId
            );
        } catch {
            Debug.WriteLine("failed to construct moveorder from text:");
            Debug.WriteLine(text);
            return null;
        }
    }
}

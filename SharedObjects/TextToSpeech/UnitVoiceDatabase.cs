using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedObjects.GameObjects.Units;

namespace SharedObjects.TextToSpeech;

public sealed class UnitVoiceDatabase {
    private static readonly Random rnd = new();
    private static readonly Dictionary<string, UnitVoiceData> unitStubs = new();
    private static readonly Dictionary<int, UnitVoiceData> inUse = new();
    private static HashSet<string> used = [];
    private static HashSet<string> infantryNicknames;
    private static HashSet<string> artilleryNicknames;
    private static HashSet<string> airNicknames;
    private static HashSet<string> playerBaseNicknames;
    private static HashSet<string> genericNicknames;
    public static List<UnitVoiceData> AdministrationVoices { get; set; }
    private static UnitVoiceDatabase instance = null;
    private static readonly object padlock = new object();

    public static UnitVoiceDatabase Instance {
        get {
            lock (padlock) {
                return instance ??= new UnitVoiceDatabase();
            }
        }
    }

    static UnitVoiceDatabase() {
        GenerateUnitVoiceMetadata();
    }

    public UnitVoiceData GetUnitVoiceByUnitId(int id) => inUse[id];

    private static void GenerateUnitVoiceMetadata() {
        infantryNicknames = new HashSet<string>(
            File.ReadAllLines("GameStatePresets/InfantryNicknames.txt")
                .OrderBy(_ => rnd.Next()).ToList()
        );
        artilleryNicknames = new HashSet<string>(
            File.ReadAllLines("GameStatePresets/ArtilleryNicknames.txt")
                .OrderBy(_ => rnd.Next()).ToList()
        );
        artilleryNicknames.ExceptWith(infantryNicknames);
        airNicknames = new HashSet<string>(
            File.ReadAllLines("GameStatePresets/AirNicknames.txt")
                .OrderBy(_ => rnd.Next()).ToList()
        );
        airNicknames.ExceptWith(infantryNicknames);
        airNicknames.ExceptWith(artilleryNicknames);

        playerBaseNicknames = new HashSet<string>(
            File.ReadAllLines("GameStatePresets/PlayerBaseNicknames.txt")
                .OrderBy(_ => rnd.Next()).ToList()
        );

        genericNicknames = new HashSet<string>(
            File.ReadAllLines("GameStatePresets/GenericNicknames.txt")
                .OrderBy(_ => rnd.Next()).ToList()
        );
        genericNicknames.ExceptWith(infantryNicknames);
        genericNicknames.ExceptWith(artilleryNicknames);
        genericNicknames.ExceptWith(airNicknames);
        genericNicknames.ExceptWith(airNicknames);


        foreach (string nickname in infantryNicknames
                     .Union(artilleryNicknames)
                     .Union(airNicknames)
                     .Union(genericNicknames)) {
            unitStubs.Add(nickname, GenerateRandomUnitData(nickname));
        }

        AdministrationVoices = new List<UnitVoiceData>() {
            new(
                "administrator1", 0,
                0, 0,
                "julia", "strict"
            ),

            new(
                "administrator2", 0,
                0, 0, "jane",
                "evil"
            ),
        };
    }


    private static UnitVoiceData GenerateRandomUnitData(string nickname) {
        string name = YandexTtSData.MaleVoices[
            rnd.Next(YandexTtSData.MaleVoices.Count)
        ].name;
        string[]? moods = YandexTtSData.Voices.First(x => x.name == name).mood;
        string? mood = moods?[rnd.Next(moods.Length)];
        return new UnitVoiceData(
            nickname: nickname,
            voiceSpeedModificator: 1 + (float)rnd.NextDouble() / 3,
            pitchShift: rnd.NextInt64(-300, 200),
            radioInterferenceModificator: (float)rnd.NextDouble() * 5,
            yandexVoice: name,
            yandexVoiceRole: mood
        );
    }

    public UnitVoiceData ReserveAndGetUnitData(int unitId, UnitType unitType) {
        HashSet<string> collection = genericNicknames;
        if (unitType == UnitType.InfantryUnit)
            collection = infantryNicknames;
        if (unitType == UnitType.ArtilleryUnit)
            collection = artilleryNicknames;
        if (unitType == UnitType.AirUnit)
            collection = airNicknames;
        string nickname = collection.First();
        collection.Remove(nickname);

        inUse[unitId] = unitStubs[nickname];
        return inUse[unitId];
    }
}

public struct UnitVoiceData {
    public string Nickname;
    public float VoiceSpeedModificator; // [1.0; 1.3]
    public float PitchShift; // [-1000;1000]
    public float RadioInterferenceModificator; // [0; 5];
    public string YandexVoice = "";
    public string? YandexVoiceRole;


    public UnitVoiceData(
        string nickname, float voiceSpeedModificator, float pitchShift,
        float radioInterferenceModificator, string yandexVoice, string? yandexVoiceRole) {
        Nickname = nickname;
        VoiceSpeedModificator = voiceSpeedModificator;
        PitchShift = pitchShift;
        RadioInterferenceModificator = radioInterferenceModificator;
        YandexVoice = yandexVoice;
        YandexVoiceRole = yandexVoiceRole;
    }
}

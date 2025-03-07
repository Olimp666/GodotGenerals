using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedObjects.GameObjects.Units;


namespace SharedObjects.GameObjects;

public class Preset {
    public int GridHeight;
    public int GridWidth;
    public List<UnitInfo> UnitsInfo = [];
    private string _presetFolder = "GameStatePresets/";

    public Preset(string presetName) {
        var input = File.ReadAllLines(_presetFolder + presetName)
            .Where(x => !x.StartsWith('#')).ToList();


        GridHeight = input.Count;
        GridWidth = input.Max(s => s.Length);
        var y = 0;
        foreach (string lineFeed in input) {
            var x = 0;
            foreach (char symbol in lineFeed) {
                x += 1;
                var lower = Char.ToLower(symbol).ToString();
                if (!unitTypes.ContainsKey(lower))
                    continue;
                var owner = Char.IsLower(symbol) ? 0 : 1;
                UnitsInfo.Add(
                    new UnitInfo {
                        unitType = unitTypes[lower], ownerId = owner, x = x, y = y
                    }
                );

            }

            y += 1;
        }

        return;
    }


    private static readonly Dictionary<string, UnitType> unitTypes = new() {
        { "i", UnitType.InfantryUnit },
        { "b", UnitType.PlayerBase },
        { "a", UnitType.ArtilleryUnit },
        { "v", UnitType.AirUnit },
    };
}

public struct UnitInfo {
    public int ownerId;
    public UnitType unitType;
    public int x;
    public int y;
}

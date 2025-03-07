using Terminal.Gui;

namespace Server.InterfaceViews;

public sealed class GameStateView : View {
    private TextView gameField = new() { ReadOnly = true, X = 0, Y = 1 };

    public GameStateView() {
        Width = Dim.Fill();
        Height = Dim.Fill();
        gameField.Width = Dim.Fill();
        gameField.Height = Dim.Fill();
        Add(gameField);
    }

    public void Update() {
        gameField.Text = Program.GameState.GameStateAsString();
    }
}
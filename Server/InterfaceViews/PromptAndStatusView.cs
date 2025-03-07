using Terminal.Gui;

namespace Server.InterfaceViews;

public sealed class PromptAndStatusView : View {
    private readonly TextField commandResult = new() {
        Text = "help",
        Y = 1,
        X = 1,
        Height = 25,
        Width = 25
    };

    private readonly TextField prompt = new("some editable text") { X = 3, Y = 0, Height = 1, Width = 30 };

    private readonly Label promptCursor = new(">:") { X = 0, Y = 0, Height = 1 };
    private readonly Label serverStatusLabel = new("launching");
    private Label gameState;
    private Label gameStatus;
    private TimeSpan serverLastValue;
    private ProgressBar serverStatus;


    public PromptAndStatusView() {
            InitializeComponent();
        }

    private void InitializeComponent() {
            Width = Dim.Fill();
            Height = Dim.Fill();
            Add(promptCursor);
            Add(prompt);
            var serverLabel = new Label("server is:") { X = 40, Y = 0 };
            Add(serverLabel);
            serverStatusLabel.X = 51;
            serverStatusLabel.Y = Pos.Y(serverLabel);
            serverStatusLabel.Height = 1;
            Add(serverStatusLabel);

            serverStatus = new ProgressBar {
                ProgressBarFormat = ProgressBarFormat.Simple,
                ProgressBarStyle = ProgressBarStyle.MarqueeContinuous,
                X = 59,
                Y = 0,
                Width = 7,
                Height = 1
            };
            Add(serverStatus);
            var gameStatusLabel = new Label("game state:") { X = 40, Y = 1 };
            Add(gameStatusLabel);
            gameStatus =
                new Label($"waiting players 0/{Program.PlayersAmount}") { X = Pos.Right(gameStatusLabel) + 1, Y = 1 };
            Add(gameStatus);
            var presetLabel = new Label("preset:") { X = 40, Y = 2 };
            Add(presetLabel);
            var presetName = new Label(Program.PresetPath) { X = Pos.Right(presetLabel) + 1, Y = 2 };
            Add(presetName);

            Add(commandResult);
            Add(new Label("F1 - current") { X = 45, Y = 4 });
            Add(new Label("F2 - game state") { X = 45, Y = 5 });
            Add(new Label("F3 - logs") { X = 45, Y = 6 });
        }

    public void Update() {
            gameStatus.Text = desideGameStatus();

            if (!(Program.Server.TimeAlive > serverLastValue)) {
                serverStatusLabel.Text = "paused";
                serverStatus.Visible = false;
            } else {
                serverStatus.Visible = true;
                serverStatusLabel.Text = "running";
                serverLastValue = (TimeSpan)Program.Server.TimeAlive;
                serverStatus.Pulse();
            }
        }

    private string desideGameStatus() {
            if (Program.Server.ConnectedPeers < Program.PlayersAmount)
                return $"waiting players {Program.Server.ConnectedPeers}/{Program.PlayersAmount}" +
                       $"  {Program.GameState.ElapsedGameTime:g}";
            if (Program.GameState.IsPaused)
                return $"paused {Program.GameState.ElapsedGameTime:g}";

            return $"running {Program.GameState.ElapsedGameTime:g}";
        }
}
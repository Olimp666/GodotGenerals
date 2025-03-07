using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using CommandLine;
using Server.InterfaceViews;
using SharedObjects.GameObjects;
using SharedObjects.GameObjects.Orders;
using SharedObjects.TextToSpeech;
using Terminal.Gui;
using VoiceRecognitionModule;
using VoiceResponseModule;
using Attribute = Terminal.Gui.Attribute;

namespace Server;

internal static class Program {
    public const int Port = 12345;
    public static readonly List<string> Logs = [];

    public static int PlayersAmount;
    public static string PresetPath = "";
    public static bool DisableSoundNotifications = true;
    public static UnitVoiceDatabase VoiceDatabase = UnitVoiceDatabase.Instance;
    public static Server Server = null!;
    public static GameState GameState = null!;
    public static SoundNotificationsService SoundManager = new();

    public static ResponseEmitterService ResponseEmitterService = null!;
    public static VoiceRecognitionService VoiceRecognitionService = null!;


    private static int Main(string[] args) {
        DotNetEnv.Env.Load();
        DotNetEnv.Env.TraversePath().Load();
        Console.OutputEncoding = Encoding.UTF8;
        Trace.Listeners.Add(new DebugListener());

        Parser.Default.ParseArguments<Options>(args).WithParsed(
            o => {
                PlayersAmount = o.PlayersAmountParam;
                PresetPath = o.PresetPathParam;
                DisableSoundNotifications = o.NoSoundNotificationParam;
            }
        );


        Application.UseSystemConsole = false;

        Application.Init();
        GameState = new GameState(VoiceDatabase, new Preset(PresetPath));
        ResponseEmitterService = new ResponseEmitterService(
            VoiceDatabase, GameState, PlayersAmount, Port
        );
        VoiceRecognitionService = new VoiceRecognitionService(
            VoiceDatabase, GameState, PlayersAmount, Port
        ){CommandFormedHandler = YieldCommand};
        
        GameState.AddVoiceRequest = ResponseEmitterService.AddVoiceRequest;
        Server = new Server(GameState) { PlayersAmount = PlayersAmount };
        Server.PeersAmountChanged += PeersAmountChangedHandler;
        if (!DisableSoundNotifications)
            SoundEventsSetup();

        var mainView = new MainView(new Rect(1, 1, 90, 28)) {
            Border = new Border { BorderStyle = BorderStyle.Single }, ColorScheme = new() {
                Normal = Attribute.Make(
                    Color.BrightGreen, Color.Black
                ),
                Focus = Attribute.Make(Color.Brown, Color.Black)
            }
        };
        Application.Top.Add(mainView);


        var gameLoopThread = new Thread(GameLoop);
        var networkThread = new Thread(NetworkLoop);
        var broadcastThread = new Thread(BroadcastLoop);
        var consoleThread = new Thread(ConsoleLoop);
        //var voiceEmitterThread = new Thread(VoiceEmitterLoop);
        //var voiceRecognitionTask = new Task(VoiceRecognitionLoop);

        gameLoopThread.Start();
        networkThread.Start();
        broadcastThread.Start();
        consoleThread.Start();
        //voiceEmitterThread.Start();
        //voiceRecognitionTask.Start();


        gameLoopThread.Join();
        networkThread.Join();
        broadcastThread.Join();
        consoleThread.Join();
        //voiceEmitterThread.Join();
        //voiceRecognitionTask.Wait();

        return 0;
    }

    private static void SoundEventsSetup() {
        Server.ConnectedEvent += SoundManager.PlayConnectedSound;
        Server.DisconnectedEvent += SoundManager.PlayDisconnectedSound;
    }

    private static void NetworkLoop() {
        Server.Start();
        while (true) {
            Server.Update();
            Thread.Sleep(10);
        }
    }

    private static void ConsoleLoop() {
        Application.Run();
        Application.Shutdown();
    }

    private static void PeersAmountChangedHandler() {
        if (Server.ConnectedPeers == 0)
            GameState.IsPaused = true;
        if (Server.ConnectedPeers > 1)
            GameState.IsPaused = false;
        if (Server.ConnectedPeers == PlayersAmount)
            GameState.InitializeWorld();
    }

    private static void GameLoop() {
        var t0 = DateTime.Now;
        while (true) {
            DateTime t1 = DateTime.Now;
            GameState.Update(t1 - t0);
            t0 = t1;
            Thread.Sleep(16); // 60fps
        }

        return;
    }

    private static void VoiceRecognitionLoop() {
        while (true) {
            // VoiceRecognitionService.Process();
        }
    }

    private static void YieldCommand(IOrder command) {
        GameState.AddOrder(command);
    }

    private static void VoiceEmitterLoop() {
        while (true) {
            ResponseEmitterService.ProcessNext();
            Thread.Sleep(100);
        }
    }

    private static void BroadcastLoop() {
        while (true) {
            // Сериализация и рассылка игрового состояния
            // BroadcastGameState();
            Thread.Sleep(50); // Частота рассылки состояния
        }
    }

    public class Options {
        [Option(
            'a', "players",
            Default = 1, HelpText = "Set amount of players for server to await"
        )]
        public required int PlayersAmountParam { get; set; }

        [Option(
            'p', "preset",
            Default = "default.txt", HelpText = "Path to the preset of initial game state"
        )]
        public required string PresetPathParam { get; set; }

        [Option(
            's', "silent",
            Default = false, HelpText = "Disable sound notifications"
        )]
        public required bool NoSoundNotificationParam { get; set; }
    }

    private class DebugListener : TraceListener {
        public override void Write(string? message) {
            if (message != null) Logs.Add(message);
        }

        public override void WriteLine(string? message) {
            if (message != null) Logs.Add(message + "\n");
        }
    }
}

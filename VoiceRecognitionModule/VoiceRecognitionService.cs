using System.Diagnostics;
using Grpc.Core;
using SharedObjects;
using SharedObjects.GameObjects;
using SharedObjects.GameObjects.Orders;
using SharedObjects.Proto;
using SharedObjects.TextToSpeech;
using VoiceRecognitionModule.Backends;

namespace VoiceRecognitionModule;

public class VoiceRecognitionService {
    private readonly Dictionary<int, VoiceRecognitionConnection> receivers = new();
    // private readonly MsEmbeddedStTBackend msBackend = new();
    private readonly VoskStTBackend voskBackend = new();
    private readonly TextToCommandService ttCommand;
    private UnitVoiceDatabase VoiceDatabase { get; }
    public delegate void CommandFormed(IOrder command);

    public CommandFormed CommandFormedHandler = command => { };


    public VoiceRecognitionService(UnitVoiceDatabase voiceDatabase,
        GameState gameState, int playersAmount, int port) {
        VoiceDatabase = voiceDatabase;

        ttCommand = new TextToCommandService(VoiceDatabase, gameState);

        for (var i = 0; i < playersAmount; i++) {
            AddPeer(new VoiceTransmitterData { PlayerId = i, Port = port - 1 - i });
        }
    }


    private void AddPeer(VoiceTransmitterData transmitterData) {
        var receiver = new VoiceRecognitionConnection(
            VoiceReceiveHandlerGenerator(transmitterData.PlayerId)
        );
        var server = new Server {
            Services = { SpeechToCommand.BindService(receiver) },
            Ports = {
                new ServerPort(
                    "localhost", transmitterData.Port, ServerCredentials.Insecure
                )
            }
        };
        server.Start();
        receivers[transmitterData.PlayerId] = receiver;
    }

    private VoiceRecognitionConnection.VoiceAudioDelegate
        VoiceReceiveHandlerGenerator(int playerId) {
        return (audio) => {
            var text = voskBackend.AudioToText(audio);
            Debug.WriteLine($"vosk: {text}");
            var command = ttCommand.TextToCommand(playerId, text);
            if (command != null)
                CommandFormedHandler(command);
            return $"{playerId}: said something";
        };
    }
}
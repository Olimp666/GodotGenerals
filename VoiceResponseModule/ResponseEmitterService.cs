using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Core;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SharedObjects;
using SharedObjects.GameObjects;
using SharedObjects.Proto;
using SharedObjects.TextToSpeech;
using VoiceResponseModule.Backends;

namespace VoiceResponseModule;

public class ResponseEmitterService {
    private YandexTtSBackend yandexBackend = new();
    private readonly MsEmbeddedTtSBackend msBackend = new();
    public bool IsEmitting { get; set; }
    private readonly UnitVoiceDatabase database;
    private readonly Queue<VoiceRequest> voiceRequests = new();

    public void AddVoiceRequest(VoiceRequest request) =>
        voiceRequests.Enqueue(request);

    private readonly SignalGenerator noiseGen = new(22050, 1);
    private readonly MixingWaveProvider32 mixer = new();
    private readonly Random rnd = new();

    private static readonly WaveFormat custom = WaveFormat.CreateCustomFormat(
        WaveFormatEncoding.Pcm, 22050,
        1, 44100, 2,
        16
    );

    private readonly Dictionary<int, VoiceEmitterConnection> transmitters = new();

    public ResponseEmitterService(
        UnitVoiceDatabase database, GameState gameState, int peersAmount, int port
    ) {
        this.database = database;
        VoiceRequestToTextService.gameState = gameState;


        for (var i = 0; i < peersAmount; i++)
            AddPeer(new VoiceTransmitterData { PlayerId = i, Port = port + 1 + i });
    }

    private void AddPeer(VoiceTransmitterData transmitterData) {
        var emitter = new VoiceEmitterConnection();
        var server = new Server {
            Services = { StreamAudioFile.BindService(emitter) },
            Ports = {
                new ServerPort(
                    "localhost", transmitterData.Port, ServerCredentials.Insecure
                )
            }
        };
        server.Start();
        transmitters[transmitterData.PlayerId] = emitter;
    }

    private byte[] PostProduction(
        byte[] audio, object? args, int offset = 0) {
        var source = (byte[])audio.Clone();
        var noise1 = (byte[])audio.Clone();
        var noise2 = (byte[])audio.Clone();
        noiseGen.Gain = 0.015;
        noiseGen.Frequency = 100;
        noiseGen.Type = SignalGeneratorType.Pink;
        noiseGen.ToWaveProvider16().Read(noise1, 0, noise1.Length);
        noiseGen.Gain = (0.01 + rnd.NextDouble() / 100);
        noiseGen.Frequency = rnd.Next(200);
        noiseGen.Type = (SignalGeneratorType)rnd.Next(3, 7);
        Debug.WriteLine(noiseGen.Type);
        noiseGen.ToWaveProvider16().Read(noise2, 0, noise1.Length);
        var mixer2 = new MixingSampleProvider(
            WaveFormat.CreateIeeeFloatWaveFormat(22050, 1)
        );
        mixer2.AddMixerInput(_to32(source));
        mixer2.AddMixerInput(_to32(noise1));
        mixer2.AddMixerInput(_to32(noise2));
        mixer2.ToWaveProvider16().Read(audio, 0, audio.Length);
        return audio;
    }

    private static Wave16ToFloatProvider _to32(byte[] audio) {
        return new Wave16ToFloatProvider(
            new RawSourceWaveStream(
                audio, 0, audio.Length, custom
            )
        );
    }

    private void TransmitResponse(VoiceRequest request, byte[] answer) {
        transmitters[request.PlayerId].addBuffer(answer);
    }

    public async void ProcessNext() {
        if (voiceRequests.Count == 0)
            return;
        await ProcessVoiceRequest(voiceRequests.Dequeue());
    }

    private async Task ProcessVoiceRequest(VoiceRequest request) {
        string text = VoiceRequestToTextService.RenderVoiceRequest(request);
        Debug.WriteLine(text);
        byte[] voiceAnswer = await msBackend.Synthesize(
            text, database.GetUnitVoiceByUnitId(request.UnitId)
        );
        // var provider = new BufferedWaveProvider(new WaveFormat(41000, 1));
        // provider.AddSamples(voiceAnswer, 0, voiceAnswer.Length);
        // var provider = new WaveFileReader("test.wav");
        // Debug.WriteLine(provider.WaveFormat);
        // WaveOutEvent waveOutEvent = new WaveOutEvent();
        // waveOutEvent.Init(provider);
        // waveOutEvent.Play();

        // var fileStream = new Mp3FileReader("tears_in_rain.mp3");
        // var buffer = new byte[fileStream.WaveFormat.AverageBytesPerSecond];

        TransmitResponse(
            request, PostProduction(voiceAnswer, null, 64)
        );
    }
}

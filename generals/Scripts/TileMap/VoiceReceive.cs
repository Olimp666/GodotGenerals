using Godot;
using Grpc.Core;
using Grpc.Net.Client;
using NAudio.Wave;
using SharedObjects.Proto;
using SharedObjects.TextToSpeech;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;

namespace TileMap;
public partial class VoiceReceive : Node
{
    Thread voiceReceiverThread;
    VoiceReceiverModule voiceReceiver;
    WaveOutEvent waveOutEvent = new();
    static WaveFormat custom = WaveFormat.CreateCustomFormat(WaveFormatEncoding.Pcm, 22050, 1, 44100, 2, 16);
    VoiceStreamingProcessor processor;

    BufferedWaveProvider bufferedWaveProvider = new(custom)
    {
        BufferLength = custom.AverageBytesPerSecond * 60,
        DiscardOnBufferOverflow = true
    };

    public override void _Ready()
    {
        Debugger.Log(0, "Msg", "Voice node initialized");
        processor = new VoiceStreamingProcessor();
        waveOutEvent.Init(bufferedWaveProvider);
        voiceReceiver = new VoiceReceiverModule(bufferedWaveProvider);
        waveOutEvent.Play();
        voiceReceiverThread = new Thread(voiceReceiver.ReadBytesFromServer);
        voiceReceiverThread.Start();
    }
    public override void _Process(double delta)
    {
        processor.Update(delta);
    }
}

public class VoiceStreamingProcessor
{
    private readonly WaveInEvent waveInEvent;
    private GrpcChannel channel;
    private SpeechToCommand.SpeechToCommandClient voiceStreamingClient;
    private AsyncClientStreamingCall<VoiceAudio, Response> callForSendVoice;
    private bool _wasSpacePressed = false;

    public VoiceStreamingProcessor()
    {
        WaveFormat format = new WaveFormat(22050, 1);
        waveInEvent = new WaveInEvent();
        waveInEvent.WaveFormat = format;

        waveInEvent.DataAvailable += OnWaveInEventOnDataAvailable;
        //waveInEvent.StartRecording();
    }

    private void OnWaveInEventOnDataAvailable(object s, WaveInEventArgs a)
    {
        callForSendVoice.RequestStream.WriteAsync(
            new VoiceAudio { RecordVoice = Google.Protobuf.ByteString.CopyFrom(a.Buffer) }
        );
    }

    public void Update(double delta)
    {
        bool isSpacePressed = Input.IsKeyPressed(Key.Space);
        if (_wasSpacePressed && !isSpacePressed)
        {
            waveInEvent.StopRecording();
            Thread.Sleep(100);
            channel?.Dispose();
            callForSendVoice?.Dispose();
            channel = null;
            voiceStreamingClient = null;
            callForSendVoice = null;
        }

        if (!_wasSpacePressed && isSpacePressed)
        {
            channel ??= GrpcChannel.ForAddress("http://localhost:12344");
            voiceStreamingClient ??= new SpeechToCommand.SpeechToCommandClient(channel);
            callForSendVoice ??= voiceStreamingClient.AudioToText();
            waveInEvent.StartRecording();
        }

        _wasSpacePressed = isSpacePressed;
    }
}


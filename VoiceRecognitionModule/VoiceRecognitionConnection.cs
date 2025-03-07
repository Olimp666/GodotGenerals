using System.Diagnostics;
using System.Net.NetworkInformation;
using Google.Protobuf;
using Grpc.Core;
using NAudio.Wave;
using SharedObjects.Proto;

namespace VoiceRecognitionModule;

public class VoiceRecognitionConnection : SpeechToCommand.SpeechToCommandBase {
    public delegate string VoiceAudioDelegate(byte[] voice);

    public VoiceAudioDelegate VoiceStreamReceivedHandler;

    public VoiceRecognitionConnection(VoiceAudioDelegate voiceStreamReceivedHandler) {
        VoiceStreamReceivedHandler = voiceStreamReceivedHandler;
    }

    public override async Task<Response> AudioToText(
        IAsyncStreamReader<VoiceAudio> requestStream, ServerCallContext context
    ) {
        Debug.WriteLine("audioRec got connection!");

        var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(22050, 1));
        bufferedWaveProvider.BufferLength =
            bufferedWaveProvider.WaveFormat.AverageBytesPerSecond * 10;
        await foreach (VoiceAudio audio_voice in requestStream.ReadAllAsync()) {
            bufferedWaveProvider.AddSamples(
                audio_voice.RecordVoice.ToByteArray(),
                64, audio_voice.RecordVoice.Length - 64);
        }

        var buffer = new byte[bufferedWaveProvider.BufferLength];
        bufferedWaveProvider.Read(buffer, 0, bufferedWaveProvider.BufferLength);
        VoiceStreamReceivedHandler(buffer);
        return new Response { Content = "Working;" };
    }
}

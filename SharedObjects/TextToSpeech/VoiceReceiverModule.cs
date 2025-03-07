using System;
using System.Threading;
using Grpc.Core;
using Grpc.Net.Client;
using NAudio.Wave;
using SharedObjects.Proto;

namespace SharedObjects.TextToSpeech;

public class VoiceReceiverModule : StreamAudioFile.StreamAudioFileClient {
    private readonly GrpcChannel channel;
    private readonly StreamAudioFile.StreamAudioFileClient client;
    private BufferedWaveProvider bufferedWaveProvider;
    private AsyncServerStreamingCall<Audio> call;
    private bool isReading = false;

    public VoiceReceiverModule(BufferedWaveProvider buffer) {
        channel = GrpcChannel.ForAddress("http://localhost:12346");
        client = new StreamAudioFile.StreamAudioFileClient(channel);
        // bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat());
        bufferedWaveProvider = buffer;
        call = client.AudioStream(new RequestToStartStream());
    }

    public async void ReadBytesFromServer() {

                // var fileStream = new Mp3FileReader("tears_in_rain.mp3");
                // var buffer = new byte[fileStream.WaveFormat.AverageBytesPerSecond];
                //
                // for (long secWindow = 0; secWindow < 6; secWindow++) {
                //     fileStream.Read(buffer, 0, buffer.Length);
                //     bufferedWaveProvider.AddSamples(buffer, 0, buffer.Length);
                //     Console.WriteLine(buffer.GetHashCode());
                //     Thread.Sleep(1000);
                // }
                //
                // return;
                while (true) {
                    try {
                        await foreach (var response in call.ResponseStream.ReadAllAsync()) {
                            bufferedWaveProvider.AddSamples(
                                response.PartOfAudioFile.ToByteArray(),
                                64,
                                response.PartOfAudioFile.Length - 64
                            );
                            Thread.Sleep(1000);
                        }
                    } catch {
                        // ignored
                    }
                }
    }
}

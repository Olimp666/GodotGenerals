using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using SharedObjects.Proto;

namespace VoiceResponseModule;

public class VoiceEmitterConnection : StreamAudioFile.StreamAudioFileBase {
    private Queue<byte[]> toSend = new();

    public void addBuffer(byte[] x) => toSend.Enqueue(x);

    public override async Task AudioStream(
        RequestToStartStream request,
        IServerStreamWriter<Audio> responseStream,
        ServerCallContext context
    ) {
        Debug.WriteLine("audioGen got connection!");
        while (true) {
            if (toSend.Count == 0) {
                Thread.Sleep(200);
                continue;
            }

            var bytesRead = toSend.Dequeue();
            Debug.WriteLine($"sending {bytesRead.Length} bytes from audioGen");
            await responseStream.WriteAsync(
                new Audio
                    { PartOfAudioFile = Google.Protobuf.ByteString.CopyFrom(bytesRead) }
            );
        }
    }
}

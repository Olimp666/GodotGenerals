using System;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Net.Client;
using SharedObjects.TextToSpeech;
using yandex.tts;

namespace VoiceResponseModule.Backends;

internal class YandexTtSBackend:ITtSBackend<UnitVoiceData> {
    private static readonly string iamToken =
        Environment.GetEnvironmentVariable("TTS_YANDEX_IAMTOKEN")
        ?? throw new InvalidOperationException();

    private static readonly string folderId =
        Environment.GetEnvironmentVariable("TTS_YANDEX_FOLDER_ID")
        ?? throw new InvalidOperationException();

    public async Task<byte[]> Synthesize(string text, UnitVoiceData voiceParams) {
        
        RepeatedField<Hints> voiceHints = [
            new Hints { Voice = voiceParams.YandexVoice },
            new Hints { Speed = voiceParams.VoiceSpeedModificator },
            new Hints { PitchShift = voiceParams.PitchShift },
        ];
        if (voiceParams.YandexVoiceRole != null)
            voiceHints.Add(new Hints { Role = voiceParams.YandexVoiceRole });
        
        var request = new UtteranceSynthesisRequest {
            Text = text,
            OutputAudioSpec = new AudioFormatOptions {
                ContainerAudio = new ContainerAudio {
                    ContainerAudioType = ContainerAudio.Types.ContainerAudioType.Wav
                }
            },
            Hints = { voiceHints },
            LoudnessNormalizationType =
                UtteranceSynthesisRequest.Types.LoudnessNormalizationType.Lufs
        };

        var channel = GrpcChannel.ForAddress("https://tts.api.cloud.yandex.net:443");
        var client = new Synthesizer.SynthesizerClient(channel);

        var headers = new Metadata {
            { "authorization", $"Bearer {iamToken}" },
            { "x-folder-id", $"{folderId}" },
        };

        var call = client.UtteranceSynthesis(request, headers);

        using var audio = new MemoryStream();
        await foreach (var response in call.ResponseStream.ReadAllAsync()) {
            audio.Write(
                response.AudioChunk.Data.ToByteArray(),
                0,
                response.AudioChunk.Data.Length
            );
        }

        audio.Seek(0, SeekOrigin.Begin);
        return audio.ToArray();
    }
}

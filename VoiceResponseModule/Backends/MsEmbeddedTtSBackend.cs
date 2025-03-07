using System;
using System.Globalization;
using System.IO;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using SharedObjects.TextToSpeech;

namespace VoiceResponseModule.Backends;

public class MsEmbeddedTtSBackend : ITtSBackend<UnitVoiceData> {
    private static SpeechSynthesizer synth = new();
    private static PromptBuilder builder = new(new CultureInfo("ru-RU"));
    private static PromptStyle style = new() { Emphasis = PromptEmphasis.Reduced };

    public async Task<byte[]> Synthesize(string text, UnitVoiceData args) {
        MemoryStream stream = new MemoryStream();
        synth.SetOutputToWaveStream(stream);
        synth.Rate = 2;
        builder.StartStyle(style);
        builder.AppendText(text);
        builder.EndStyle();
        synth.Speak(builder);
        builder.ClearContent();
        byte[] speechBytes = stream.GetBuffer();
        return _trimEnd(speechBytes);
    }


    private static byte[] _trimEnd(byte[] audio) {
        return audio;
        //2 bytes for frame means vals from the end should be leq 1
        int length = audio.Length - 1;
        while (audio[length] < 2 && audio[length - 1] < 2)
            length -= 2;
        return audio[new Range(0, length)];
    }
}

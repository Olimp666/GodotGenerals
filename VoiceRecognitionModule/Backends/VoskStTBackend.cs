using Vosk;
using System.Text.Json;


namespace VoiceRecognitionModule.Backends;

public class VoskStTBackend {
    private readonly Model model = new("vosk-ru");
    private readonly VoskRecognizer rec;

    internal VoskStTBackend() {
        rec = new VoskRecognizer(model, 22050.0f);
        rec.SetMaxAlternatives(0);
        rec.SetWords(true);
    }

    public string AudioToText(byte[] audio) {
        rec.AcceptWaveform(audio, audio.Length);
        var answer = JsonSerializer.Deserialize<JsonVoskResult>(rec.Result()).text;
        return answer;
    }
}

internal struct JsonVoskResult {
    public string text { get; set; }
}

using System.Diagnostics;
using System.Globalization;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using Google.Protobuf;

namespace VoiceRecognitionModule.Backends;


//That thing doesn't support ru lang.
[Obsolete("Does not support ru lang.")]
public class MsEmbeddedStTBackend {
    private static SpeechRecognitionEngine rec = new(
        new CultureInfo("en-US")
    );

    internal MsEmbeddedStTBackend() {
        rec.SpeechRecognized += OnRecOnSpeechRecognized;
    }

    private static string text { get; set; }

    public string AudioToText(byte[] audio) {
        var stream = new MemoryStream(audio);
        rec.SetInputToAudioStream(
            stream, new SpeechAudioFormatInfo(
                EncodingFormat.Pcm, 22050, 16,
                1, 44100, 2, null
            )
        );
        rec.RecognizeAsync();
        
        return text;
    }

    void OnRecOnSpeechRecognized(object? sender, SpeechRecognizedEventArgs args) {
        text = args.Result.Text;
        Debug.WriteLine($"ms rec: {text}");
    }
}

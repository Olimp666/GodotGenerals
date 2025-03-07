using System.Threading.Tasks;

namespace VoiceResponseModule.Backends;

public interface ITtSBackend<in T> {
    public Task<byte[]> Synthesize(string text, T args);
}
using NetCoreAudio;

namespace Server;

public class SoundNotificationsService {
    private static Player soundPlayer = null!;

    public Task? CurrentTask;

    public SoundNotificationsService() {
        soundPlayer = new Player();
        // PlayConnectedSound();
    }

    public void PlayConnectedSound() {
        CurrentTask = soundPlayer.Play("Assets/peer-connected.wav" );
    }

    public void PlayDisconnectedSound() {
        CurrentTask = soundPlayer.Play("Assets/peer-disconnected.wav");
    }
}
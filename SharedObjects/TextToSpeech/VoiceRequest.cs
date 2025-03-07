using SharedObjects.GameObjects.Orders;

namespace SharedObjects.TextToSpeech;

public struct VoiceRequest {
    public readonly int UnitId;
    public readonly int PlayerId;
    public readonly VoiceRequestType VoiceRequestType;
    public readonly int[]? Sector;
    public readonly IOrder? Order;

    public VoiceRequest(
        int playerId, int unitId, VoiceRequestType voiceRequestType, int[]? sector, IOrder? order
    ) {
        this.UnitId = unitId;
        VoiceRequestType = voiceRequestType;
        Sector = sector;
        Order = order;
        PlayerId = playerId;
    }
}

public enum VoiceRequestType {
    Confirmation,
    Report,
    EnemySpotted,
    EnemyEngaged,
    TaskFinished,
    RandomEvent
}

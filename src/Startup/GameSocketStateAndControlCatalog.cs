namespace Firefall.Reconstruction.Startup;

/// <summary>Names recovered from the native state log-name table, not an inferred transition graph.</summary>
public enum NativeGameSocketState
{
    NotOpened = 0,
    Initializing = 1,
    Transferring = 2,
    Connecting = 3,
    WaitingContact = 4,
    Connected = 5,
    DisconnectedNotify = 6,
    Disconnected = 7,
    Destroyed = 8,
    Closed = 9
}

public enum GameSocketControlOpcode : byte
{
    CloseRequest = 0,
    KeepAlive = 1,
    AcknowledgeChannel1 = 2,
    AcknowledgeChannel2 = 3,
    ClockQuery = 4,
    ClockReply = 5,
    ProbePadding = 6
}

/// <summary>
/// Immediate and deferred channel-zero control observations. Acknowledgment and
/// clock payload algorithms remain partial; this is not a live control handler.
/// </summary>
public static class GameSocketStateAndControlCatalog
{
    public const uint StateNameTableVirtualAddress = 0x1C73A10;
    public const uint StateNameTableFileOffset = 0x1872610;
    public const uint SetStateMethodFileOffset = 0xBD1B70;
    public const uint ImmediateControlMethodFileOffset = 0xBD3510;
    public const uint ImmediateControlJumpTableFileOffset = 0xBD3738;
    public const uint DeferredControlMethodFileOffset = 0xBD3760;
    public const uint DeferredControlJumpTableFileOffset = 0xBD39B4;
    public const uint SendClockQueryMethodFileOffset = 0xBD2840;
    public const uint ProcessClockSampleMethodFileOffset = 0xBE82E0;
    public const int ClockObjectPointerOffset = 0xD78;
    public const int ClockSampleCountOffset = 0x10;
    public const int MinimumClockReplyBodySize = 17;
    public const int MinimumClockQueryBodySize = 9;
    public const int ClockSamplesBeforeConnected = 2;

    public static bool CanFinishConnecting(NativeGameSocketState state, int processedClockSamples) =>
        state == NativeGameSocketState.Connecting && processedClockSamples >= ClockSamplesBeforeConnected;
}

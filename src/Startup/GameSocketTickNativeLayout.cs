namespace Firefall.Reconstruction.Startup;

/// <summary>
/// The two immediate context-driven handshake transitions inside the locked tick.
/// No timer, restart, abort, notification, or receive-queue side effect is simulated.
/// </summary>
public static class GameSocketTickNativeLayout
{
    public const uint MethodFileOffset = 0xBD4BC0;
    public const int VirtualMethodOffset = 0x10;
    public const uint JumpTableFileOffset = 0xBD505C;
    public const uint ConsumeReceivedQueueMethodFileOffset = 0xBD3E50;
    public const uint ActivateReliableQueueMethodFileOffset = 0xBE3F60;
    public const int ReceivedQueueOffset = 0x260;
    public const int RestartCounterOffset = 0x11D;
    public const int LastHandshakeSendTickOffset = 0x250;
    public const int HandshakeRetryDelayOffset = 0x254;
    public const int HandshakeAttemptCounterOffset = 0x258;
    public const int HandshakeAttemptLimitOffset = 0xDF0;
    public const int DefaultHandshakeAttemptLimit = 8;

    public static NativeGameSocketState? SelectReadyHandshakeState(
        NativeGameSocketState state, uint contextCandidate, uint expectedContext, bool hasClockObject)
    {
        if (!Enum.IsDefined(state))
            throw new ArgumentOutOfRangeException(nameof(state));
        if (state == NativeGameSocketState.Initializing && contextCandidate != 0)
            return NativeGameSocketState.Transferring;
        if (state == NativeGameSocketState.Transferring && expectedContext != 0)
            return hasClockObject ? NativeGameSocketState.Connecting : NativeGameSocketState.Connected;
        return null;
    }
}

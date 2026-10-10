namespace Firefall.Reconstruction.Startup;

public enum DestroyedSocketTickSelection
{
    CloseAlreadyRequested,
    RequestPumpClose,
    AssembleOutgoingThenCheckState
}

public readonly record struct QueuedSocketCloseSelection(
    NativeGameSocketState ResultState, bool InvokeChannelCleanup, bool IgnoreEvent);

/// <summary>Offline selections across tick, pump callback and queued close-event dispatch.</summary>
public static class GameSocketCloseCompletionNativeLayout
{
    public const uint TickMethodFileOffset = 0xBD4BC0;
    public const uint TickStateTableFileOffset = 0xBD505C;
    public const uint DestroyedTickBranchFileOffset = 0xBD4FCD;
    public const uint KeepAliveCheckMethodFileOffset = 0xBD2BB0;
    public const uint ReliableRetryCheckMethodFileOffset = 0xBD2AD0;
    public const uint DataAssemblerMethodFileOffset = 0xBD4340;
    public const uint CallbackMethodFileOffset = 0xBD2D10;
    public const uint QueueDispatchMethodFileOffset = 0xBD3E50;
    public const uint CloseQueueEventToken = 1;
    public const int CloseCompletionByteOffset = 0xD94;
    public const int EventQueueOffset = 0x260;

    public static DestroyedSocketTickSelection SelectDestroyedTick(bool closeAlreadyRequested,
        bool keepAliveCheckSucceeded, bool reliableRetryCheckSucceeded)
    {
        if (closeAlreadyRequested)
            return DestroyedSocketTickSelection.CloseAlreadyRequested;
        if (!keepAliveCheckSucceeded || !reliableRetryCheckSucceeded)
            return DestroyedSocketTickSelection.RequestPumpClose;
        return DestroyedSocketTickSelection.AssembleOutgoingThenCheckState;
    }

    public static bool SelectAssemblerTailClose(NativeGameSocketState stateAfterAssembly) =>
        stateAfterAssembly == NativeGameSocketState.Destroyed;

    /// <summary>The callback marks +D94 and enqueues token1; it does not itself write socket state.</summary>
    public static uint CaptureCompletionEvent(bool handleIsInvalid, bool completionAlreadyRecorded)
    {
        if (!handleIsInvalid)
            throw new ArgumentException("Native close completion requires invalid handle +118.", nameof(handleIsInvalid));
        if (completionAlreadyRecorded)
            throw new ArgumentException("Native close completion asserts that byte +D94 is zero.", nameof(completionAlreadyRecorded));
        return CloseQueueEventToken;
    }

    public static QueuedSocketCloseSelection SelectQueuedCloseEvent(NativeGameSocketState state) => state switch
    {
        NativeGameSocketState.Destroyed => new(NativeGameSocketState.Closed, true, false),
        NativeGameSocketState.Initializing or NativeGameSocketState.Transferring or NativeGameSocketState.Connecting
            or NativeGameSocketState.WaitingContact or NativeGameSocketState.Connected =>
            new(NativeGameSocketState.DisconnectedNotify, false, false),
        NativeGameSocketState.Disconnected or NativeGameSocketState.Closed => new(state, false, true),
        _ => throw new ArgumentOutOfRangeException(nameof(state), "The native queued close event asserts on this state.")
    };
}

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketDestroyBranch(
    NativeGameSocketState ResultState,
    bool InvokePumpClose,
    bool InvokeDirectHandleClose,
    bool InvokeChannelCleanup,
    bool RequireInvalidHandleBeforeChannelCleanup);

/// <summary>
/// State-switch selection in BD2090, not a destructor or an I/O implementation.
/// External calls and pending completion can change the socket after selection.
/// </summary>
public static class GameSocketDestroyNativeLayout
{
    public const uint RequestMethodFileOffset = 0xBD2090;
    public const uint BranchTableFileOffset = 0xBD2408;
    public const uint StateIndexTableFileOffset = 0xBD2418;
    public const uint PumpCloseMethodFileOffset = 0xBD2430;
    public const uint DirectHandleCloseMethodFileOffset = 0xBE36B0;
    public const uint ChannelCleanupMethodFileOffset = 0xBD06B0;
    public const uint ClearOwnerMethodFileOffset = 0xBD0880;
    public const int OwnerPointerOffset = 0x110;
    public const int FirstRequestMarkerOffset = 0x124;
    public const int CapturedStateOffset = 0x128;
    public const int CapturedTimestampOffset = 0x12C;
    public const int CapturedByteOffset = 0x130;
    public const uint FirstRequestMarker = 6;
    public const int CloseRequestedByteOffset = 0xD95;
    public const int LockOffset = 0xD98;
    public const int NestedLockOffset = 0xDA8;
    public const int NestingCountOffset = 0xDD8;
    public const int PumpCloseVirtualOffset = 8;

    public static GameSocketDestroyBranch SelectStateBranch(NativeGameSocketState state) => state switch
    {
        NativeGameSocketState.Initializing or NativeGameSocketState.Transferring
            or NativeGameSocketState.WaitingContact => new(NativeGameSocketState.Destroyed, true, false, false, false),
        NativeGameSocketState.Connecting or NativeGameSocketState.Connected =>
            new(NativeGameSocketState.Destroyed, false, false, false, false),
        NativeGameSocketState.NotOpened => new(NativeGameSocketState.Closed, false, true, true, true),
        NativeGameSocketState.DisconnectedNotify or NativeGameSocketState.Disconnected =>
            new(NativeGameSocketState.Closed, false, false, true, true),
        _ => throw new ArgumentOutOfRangeException(nameof(state),
            "Native destroy request asserts on Destroyed, Closed and unknown states.")
    };

    /// <summary>Only a successful atomic zero-to-six marker change records the initial state/time/byte.</summary>
    public static bool SelectFirstRequestCapture(uint previousAtomicMarker) => previousAtomicMarker == 0;
}

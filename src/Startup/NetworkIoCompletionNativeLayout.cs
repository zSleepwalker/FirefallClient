namespace Firefall.Reconstruction.Startup;

/// <summary>
/// RTTI-backed nl::Network/PumpThread IOCP routing reached by nl::GameSocket.
/// This is separate from the WinHTTP request-worker completion path.
/// </summary>
public static class NetworkIoCompletionNativeLayout
{
    public const string NetworkNativeTypeName = "nl::Network";
    public const string PumpThreadNativeTypeName = "nl::Network::PumpThread";
    public const uint NetworkVtableVirtualAddress = 0x1C734B4;
    public const uint PumpThreadVtableVirtualAddress = 0x1C734D4;
    public const uint NetworkTypeDescriptorVirtualAddress = 0x1E2C45C;
    public const uint PumpThreadTypeDescriptorVirtualAddress = 0x1E2C438;
    public const uint NetworkConstructorFileOffset = 0xBBCAD0;
    public const uint InitializeMethodFileOffset = 0xBBD110;
    public const uint RegisterSocketMethodFileOffset = 0xBBD730;
    public const uint AssociateSocketWithPortMethodFileOffset = 0xBBD820;
    public const uint CreatePortCallFileOffset = 0xBBD2E1;
    public const uint AssociateSocketCallFileOffset = 0xBBD8B7;
    public const uint PumpThreadMethodFileOffset = 0xBBC860;
    public const uint DequeueCompletionCallFileOffset = 0xBBC911;
    public const uint GetQueuedCompletionStatusImportVirtualAddress = 0x1AC432C;
    public const int EmbeddedPumpThreadOffset = 0x48;
    public const int SocketContainerOffset = 0xA4;
    public const int CompletionPortHandleOffset = 0xDC;
    public const int PumpThreadStopFlagOffset = 0x04;
    public const int DrainIterationLimit = 100;
    public const int IdleWaitMilliseconds = 500;
    public const int ReceiveCallbackVirtualOffset = 0x18;
    public const int CloseCallbackVirtualOffset = 0x1C;
    public const int ErrorCallbackVirtualOffset = 0x20;
    public const int OperationAbortedError = 995;

    public static int ResolveSuccessfulCompletionSlot(bool hasOverlapped) =>
        hasOverlapped ? ReceiveCallbackVirtualOffset : CloseCallbackVirtualOffset;
}

/// <summary>
/// The completion key is the socket, not a backpointer recovered from OVERLAPPED.
/// Normal successful receives use a retained record and repost the same buffer.
/// </summary>
public static class GameSocketReceiveNativeLayout
{
    public const uint ReceiveCompletionMethodFileOffset = 0xBD2E30;
    public const uint CloseCompletionMethodFileOffset = 0xBD2D10;
    public const uint ErrorCompletionMethodFileOffset = 0xBD2D70;
    public const uint PacketReceiveEntryFileOffset = 0xBD3030;
    public const uint GetOverlappedResultCallFileOffset = 0xBD2ED1;
    public const uint GetOverlappedResultImportVirtualAddress = 0x1AC4D64;
    public const uint PacketReceiveCallFileOffset = 0xBD2F25;
    public const uint RepostReceiveCallFileOffset = 0xBD2FE2;
    public const uint AttachNetworkMethodFileOffset = 0xBCFDF0;
    public const int NetworkPointerOffset = 0x08;
    public const int SuppressReceiveFlagOffset = 0xD95;
    public const int ReceiveLockOffset = 0xD98;
    public const int RetainedOperationPointerOffset = 0xDF4;
    public const int OperationAllocationSize = 0xA0;
    public const int OverlappedOffset = 0x00;
    public const int OverlappedSize = 0x14;
    public const int SenderSockaddrOffset = 0x18;
    public const int SenderSockaddrCapacity = 0x80;
    public const int SenderSockaddrLengthOffset = 0x98;
    public const int IncompleteOperationError = 996;
}

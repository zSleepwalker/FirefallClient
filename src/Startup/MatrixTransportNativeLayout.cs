namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Offline observations of the UDP game socket opened by the matrix connection.
/// Connection state and game-socket state are separate native fields.
/// </summary>
public static class MatrixTransportNativeLayout
{
    public const string NativeTypeName = "nl::GameSocket";
    public const uint VtableVirtualAddress = 0x1C73A5C;
    public const uint TypeDescriptorVirtualAddress = 0x1E2C770;
    public const int AllocationSize = 0xDF8;
    public const uint ConstructorFileOffset = 0xBD00B0;
    public const uint OpenMethodFileOffset = 0xBD0960;
    public const uint SocketFactoryFileOffset = 0xBE3510;
    public const uint AddressToSockaddrMethodFileOffset = 0xBE3B50;
    public const uint SockaddrToAddressMethodFileOffset = 0xBE3A00;
    public const int PumpPointerOffset = 0x08;
    public const int BoundLocalPortOffset = 0x28;
    public const int StateOffset = 0x108;
    public const int HandlerPointerOffset = 0x10C;
    public const int SocketHandleOffset = 0x118;
    public const int RemoteAddressFlagsOffset = 0x230;
    public const int RemoteAddressPortOffset = 0x248;
    public const int InitialReceiveBufferOffset = 0x278;
    public const int InitialReceiveBufferSize = 1400;
    public const int OpeningState = 1;
    public const int FailureState = 7;
    public const int IoPendingError = 997;
    public const int NativeIpv4AddressFamily = 2;
    public const int NativeIpv6AddressFamily = 23;
    public const int NativeDatagramSocketType = 2;
    public const int NativeUdpProtocol = 17;
    public const int NativeOverlappedSocketFlag = 1;
    public const uint BindCallFileOffset = 0xBD0D05;
    public const uint GetSocketNameCallFileOffset = 0xBD0E6E;
    public const uint InitialReceiveCallFileOffset = 0xBD1211;
    public const uint WsaRecvFromImportVirtualAddress = 0x1AC4D54;

    public static int ResolveSocketAddressFamily(int nativeFamilyMode) => nativeFamilyMode switch
    {
        1 => NativeIpv4AddressFamily,
        2 => NativeIpv6AddressFamily,
        _ => throw new ArgumentOutOfRangeException(nameof(nativeFamilyMode),
            "The native factory asserts that family mode is 1 or 2.")
    };

    /// <summary>
    /// Candidates if every bind fails. Native execution stops on the first success.
    /// The final two attempts use port zero, allowing OS-selected ephemeral ports.
    /// </summary>
    public static IReadOnlyList<ushort> GetLocalBindPortAttempts(ushort preferredPort)
    {
        int count = preferredPort == 0 ? 2 : 7;
        var ports = new ushort[count];
        ushort port = preferredPort;
        for (int attempt = 0; attempt < count; attempt++)
        {
            if (attempt >= count - 2)
                port = 0;
            ports[attempt] = port;
            if (port != 0)
                port = unchecked((ushort)(port + 1));
        }
        return Array.AsReadOnly(ports);
    }
}

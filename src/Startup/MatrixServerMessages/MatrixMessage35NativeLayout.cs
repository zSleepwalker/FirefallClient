namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixMessage35ObservedScalarUpdate(
    int ConnectionState, uint WordAt860, uint WordAt864, ulong ClockSampleAt818, byte FlagsAt83D)
{
    public ulong OpaqueValueAt860 => ((ulong)WordAt864 << 32) | WordAt860;
}

/// <summary>
/// Observed scalar portion of msg-35 handling. The reference-backed field copy
/// is recorded by offset but is not reconstructed by this model.
/// </summary>
public static class MatrixMessage35NativeLayout
{
    public const uint HandlerFileOffset = 0x490AD0;
    public const uint DecodedMessageId = 35;
    public const int RequiredConnectionState = 3;
    public const int StateAfterHandling = 4;
    public const int DescriptorFirstWordOffset = 0x28;
    public const int DescriptorSecondWordOffset = 0x2C;
    public const int DescriptorReferenceBackedFieldOffset = 0x34;
    public const int ConnectionFirstWordOffset = 0x860;
    public const int ConnectionSecondWordOffset = 0x864;
    public const int ConnectionReferenceBackedFieldOffset = 0x86C;
    public const int ConnectionClockSampleOffset = 0x818;
    public const int ConnectionRuntimeFlagsOffset = 0x83D;

    public static MatrixMessage35ObservedScalarUpdate CaptureScalarUpdate(
        int connectionState, uint firstWord, uint secondWord, ulong sampledClock,
        bool runtimeFlag1, bool runtimeFlag2, bool runtimeFlag4)
    {
        if (connectionState != RequiredConnectionState)
            throw new ArgumentOutOfRangeException(nameof(connectionState),
                "The native msg-35 handler asserts unless connection state is 3.");
        byte flags = (byte)((runtimeFlag1 ? 1 : 0) | (runtimeFlag2 ? 2 : 0) | (runtimeFlag4 ? 4 : 0));
        return new(StateAfterHandling, firstWord, secondWord, sampledClock, flags);
    }
}

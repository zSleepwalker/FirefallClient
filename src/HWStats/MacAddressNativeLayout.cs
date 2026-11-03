namespace Firefall.Reconstruction.HWStats;

/// <summary>Observed Netbios request facts and selection only; never enumerates real adapters.</summary>
public static class MacAddressNativeLayout
{
    public const uint HelperVa = 0x4041D0;
    public const uint AdapterStatusBufferVa = 0x48EFC0;
    public const int NcbByteCount = 64;
    public const int NcbBufferOffset = 4;
    public const int NcbLengthOffset = 8;
    public const int NcbCallNameOffset = 10;
    public const int NcbLanaOffset = 48;
    public const byte EnumerateCommand = 0x37;
    public const byte ResetCommand = 0x32;
    public const byte AdapterStatusCommand = 0x33;
    public const int EnumerationBufferBytes = 256;
    public const int AdapterStatusBufferBytes = 600;
    public const int AddressByteCount = 6;
    public const string AdapterStatusCallName = "*              ";
    public const string NativeFormat = "macaddr = %02x:%02x:%02x:%02x:%02x:%02x\n";
    public const bool ChecksEnumerationReturn = false;
    public const bool ChecksResetReturn = false;
    public const bool InitializesDestinationOnFailure = false;
    public const bool CallerChecksHelperReturn = false;

    /// <summary>Inputs are observed ASTAT return bytes in enumerated order, not an NCB buffer parser.</summary>
    public static int? CaptureFirstSuccessfulAttempt(ReadOnlySpan<byte> observedAdapterStatusReturns)
    {
        for (int i = 0; i < observedAdapterStatusReturns.Length; i++)
            if (observedAdapterStatusReturns[i] == 0)
                return i;
        return null;
    }
}

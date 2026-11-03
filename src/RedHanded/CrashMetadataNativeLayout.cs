namespace Firefall.Reconstruction.RedHanded;

public enum DebugSourceKind
{
    Unspecified,
    Exception,
    TooLong,
    RemoteBytes,
    BadMemory
}

public readonly record struct DebugSourceDecision(
    DebugSourceKind Kind,
    uint DumpFlags,
    bool ReadsRemoteMemory,
    uint RemoteByteCount);

public readonly record struct ProcessMemoryMetadataDecision(
    ulong ByteCount,
    bool UsedPageFileFallback);

/// <summary>Static captures of RedHanded crash metadata decisions; no target memory is read.</summary>
public static class CrashMetadataNativeLayout
{
    public const uint EmbeddedSourceExceptionCode = 0x012ED500;
    public const uint FullMemorySourceExceptionCode = 0x012ED501;
    public const uint MaximumSourceByteCount = 0x400;
    public const int ExceptionParameterCountOffset = 0x10;
    public const int ExceptionSourceByteCountOffset = 0x14;
    public const int ExceptionSourceAddressOffset = 0x18;
    public const uint ReadSourceCallVa = 0x407910;
    public const uint ProcessVirtualBytesVa = 0x406B60;
    public const uint ProcessVirtualBytesCallVa = 0x407A26;
    public const uint ProcessMemoryInfoCallVa = 0x407A74;
    public const int ProcessMemoryCountersNativeByteCount = 40;
    public const int ProcessMemoryCountersPageFileUsageOffset = 0x20;
    public const string VirtualBytesCounterPattern = @"\Process(%s*)\Virtual Bytes";
    public const string ProcessIdCounterName = "ID Process";
    public const string UnspecifiedSourceMarker = "<<UNSPECIFIED>>";
    public const string ExceptionSourceMarker = "EXCEPTION";
    public const string TooLongSourceMarker = "<<TOOLONG>>";
    public const string BadMemorySourceMarker = "<<BADMEM>>";

    public static DebugSourceDecision CaptureDebugSource(
        bool hasExceptionRecord,
        uint exceptionCode,
        uint parameterCount,
        uint sourceByteCount,
        bool readProcessMemorySucceeded)
    {
        uint flags = DumperHandoffNativeLayout.OrdinaryDumpFlags;
        if (!hasExceptionRecord)
            return new(DebugSourceKind.Unspecified, flags, false, 0);

        if (exceptionCode != EmbeddedSourceExceptionCode
            && exceptionCode != FullMemorySourceExceptionCode)
            return new(DebugSourceKind.Exception, flags, false, 0);

        if (exceptionCode == FullMemorySourceExceptionCode)
            flags = DumperHandoffNativeLayout.FullMemoryDumpFlags;
        if (parameterCount < 2)
            return new(DebugSourceKind.Unspecified, flags, false, 0);
        if (sourceByteCount > MaximumSourceByteCount)
            return new(DebugSourceKind.TooLong, flags, false, 0);

        return new(readProcessMemorySucceeded ? DebugSourceKind.RemoteBytes : DebugSourceKind.BadMemory,
            flags, true, sourceByteCount);
    }

    /// <summary>
    /// A zero preferred result selects the DWORD PagefileUsage observed after the unchecked
    /// GetProcessMemoryInfo call. Its success flag is not examined by the native caller.
    /// </summary>
    public static ProcessMemoryMetadataDecision CaptureProcessMemory(
        ulong pdhVirtualBytes, uint observedPageFileUsage) =>
        pdhVirtualBytes == 0
            ? new(observedPageFileUsage, true)
            : new(pdhVirtualBytes, false);

    public static ulong CaptureMemoryCeiling(
        bool globalMemoryStatusSucceeded, ulong totalPageFile, ulong totalVirtual) =>
        globalMemoryStatusSucceeded ? Math.Min(totalPageFile, totalVirtual) : 0;
}

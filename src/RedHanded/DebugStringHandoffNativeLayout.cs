namespace Firefall.Reconstruction.RedHanded;

public readonly record struct DebugStringReadDecision(
    bool ReadRemoteMemory,
    uint ReadByteCount,
    uint AllocationByteCount);

/// <summary>Static debug-string producer/consumer facts; no credential values or remote reads.</summary>
public static class DebugStringHandoffNativeLayout
{
    public const string ClientLogPathFormat = "RED5-LOGPATH=%s\n";
    public const string ClientDumpAuthFormat = "RED5-DUMPAUTH=!%s\n";
    public const string Prefix = "RED5-";
    public const string LogPathField = "LOGPATH=";
    public const string DumpAuthField = "DUMPAUTH=";
    public const int LogPathValueByteOffset = 13;
    public const int DumpAuthValueByteOffset = 14;
    public const int OwnerLogPathOffset = 0x0C;
    public const int OwnerDumpAuthOffset = 0x10;
    public const int ClientLogPathProducerFileOffset = 0x12BBC60;
    public const int ClientDumpAuthProducerFileOffset = 0x63AEF0;
    public const uint ClientOutputDebugStringImportVa = 0x1AC434C;
    public const uint RemoteReadCallVa = 0x409AF5;
    public const uint LogPathAssignCallVa = 0x409B5C;
    public const uint DumpAuthAssignCallVa = 0x409B92;
    public const uint DebugEventCode = 8;

    public static DebugStringReadDecision CaptureRead(ushort unicodeFlag, ushort reportedLength) =>
        unicodeFlag == 0 && reportedLength > 5
            ? new(true, reportedLength, (uint)reportedLength + 1)
            : new(false, 0, 0);
}

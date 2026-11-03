namespace Firefall.Reconstruction.RedHanded;

public readonly record struct DumperStreamField(
    uint StreamType,
    string Kind,
    string Source,
    uint StreamTypePushVa,
    uint FormatStringVa,
    uint FormatCallVa);

public readonly record struct DumperPipeWriteDecision(
    bool WaitForDumper,
    uint? TerminateExitCode);

public readonly record struct DumperBuildNumbers(uint SourceRevision, uint BuildNumber);

/// <summary>Producer-side facts, in PE virtual addresses, not client raw-image offsets.</summary>
public static class DumperHandoffNativeLayout
{
    public const uint SpawnVa = 0x407040;
    public const uint BuildMetadataVa = 0x407630;
    public const uint CreatePipeCallVa = 0x407117;
    public const uint WriteFileCallVa = 0x40749B;
    public const uint WaitCallVa = 0x4074EB;
    public const uint PrimarySpawnCallVa = 0x408198;
    public const uint FullMemorySpawnCallVa = 0x40832F;
    public const int PipeRequestedCapacity = 0x4000;
    public const uint ChildCreationFlags = 0x08000000;
    public const uint OrdinaryDumpFlags = 0x40;
    public const uint FullMemoryDumpFlags = 2;
    public const string LineTerminator = "\r\n";
    public const string EndOfMetadata = "\r\n";
    public const int OwnerApplicationNameOffset = 0x20;
    public const int OwnerSourceRevisionOffset = 0x24;
    public const int OwnerBuildNumberOffset = 0x28;
    public const int OwnerEnvironmentMnemonicOffset = 0x2C;
    public const int OwnerEnvironmentNameOffset = 0x30;
    public const uint DebugLoopVa = 0x409880;
    public const uint ExceptionHandlerCallVa = 0x409A05;
    public const uint RipHandlerCallVa = 0x409A86;
    public const uint ThreadIdStreamType = 0x1000A;

    public static IReadOnlyList<DumperStreamField> Fields { get; } =
        Array.AsReadOnly(new[]
        {
            new DumperStreamField(0x10000, "str", "owner +0x1C (command line)",
                0x407AAF, 0x545708, 0x407ABA),
            new DumperStreamField(0x10002, "str", "environment mnemonic (owner +0x2C)",
                0x407B54, 0x545714, 0x407B5F),
            new DumperStreamField(0x10003, "str", "environment name (owner +0x30)",
                0x407BFB, 0x545720, 0x407C06),
            new DumperStreamField(0x10004, "i32", "source revision (owner +0x24)",
                0x407C92, 0x54572C, 0x407C9D),
            new DumperStreamField(0x10005, "i32", "build number (owner +0x28)",
                0x407D30, 0x545738, 0x407D3B),
            new DumperStreamField(0x10006, "str", "decoded debug source text",
                0x407DCF, 0x545744, 0x407DDA),
            new DumperStreamField(0x10007, "str", "handler message argument",
                0x407E68, 0x545758, 0x407E79),
            new DumperStreamField(0x10008, "i64", "process memory measurement",
                0x407F08, 0x545764, 0x407F13),
            new DumperStreamField(0x10009, "i64", "minimum of total page file and virtual memory",
                0x407FA3, 0x545774, 0x407FAE),
            new DumperStreamField(0x1000A, "i32", "debug event thread ID",
                0x40803D, 0x545784, 0x408048),
            new DumperStreamField(0x1000B, "str", "application name (owner +0x20)",
                0x4080DF, 0x545790, 0x4080EA)
        });

    /// <summary>A successful short write follows the same termination path as WriteFile failure.</summary>
    public static DumperPipeWriteDecision CapturePipeWrite(
        bool writeSucceeded, uint requestedByteCount, uint writtenByteCount) =>
        writeSucceeded && requestedByteCount == writtenByteCount
            ? new(true, null)
            : new(false, RedHandedLayout.KillExitCode);

    /// <summary>
    /// Captures values after native atoi; zero selects the build defaults.
    /// Negative nonzero results retain their bit pattern when printed with %u.
    /// </summary>
    public static DumperBuildNumbers CaptureBuildNumbers(int sourceRevision, int buildNumber) =>
        new(sourceRevision == 0 ? (uint)RedHandedLayout.DefaultSourceRev : unchecked((uint)sourceRevision),
            buildNumber == 0 ? (uint)RedHandedLayout.DefaultBuildNumber : unchecked((uint)buildNumber));
}

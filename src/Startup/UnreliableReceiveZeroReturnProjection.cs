using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveZeroReturnCapture(
    bool? PriorForwardCallReturnedNormally,
    uint? HelperReturnEax);

public enum UnreliableReceiveZeroReturnOutcome
{
    MissingPriorForwardCallReturnCapture,
    PriorForwardCallDidNotReturnNormally,
    ZeroReturnTailReached
}

public readonly record struct UnreliableReceiveZeroReturnPlan(
    UnreliableReceiveZeroReturnCapture Capture,
    UnreliableReceiveZeroReturnOutcome Outcome,
    bool? PriorForwardCallNormalGatePassed,
    bool UnreachedHelperReturnEaxCaptureProvided,
    bool StackCleanupProjected,
    ushort? StackCleanupBytes,
    bool AlZeroProjected,
    byte? AlAfterXor,
    uint? EaxAfterXorAl,
    bool EdiPopped,
    bool EsiPopped,
    bool EbxPopped,
    bool FrameStackPointerReset,
    bool EbpPopped,
    bool ReturnReached,
    ushort? ReturnImmediateBytes,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveZeroReturnInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the normal forwarding-call return through its zero-AL epilogue.
/// </summary>
public static class UnreliableReceiveZeroReturnProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint TailStartRawFileOffset = 0x00BE5B9E;
    public const uint TailStartVirtualAddress = 0x00FE679E;
    public const uint StackCleanupRawFileOffset = 0x00BE5B9E;
    public const uint XorAlRawFileOffset = 0x00BE5BA1;
    public const uint PopEdiRawFileOffset = 0x00BE5BA3;
    public const uint PopEsiRawFileOffset = 0x00BE5BA4;
    public const uint PopEbxRawFileOffset = 0x00BE5BA5;
    public const uint FrameStackPointerResetRawFileOffset = 0x00BE5BA6;
    public const uint PopEbpRawFileOffset = 0x00BE5BA8;
    public const uint ReturnRawFileOffset = 0x00BE5BA9;
    public const uint TailEndRawFileOffsetExclusive = 0x00BE5BAC;
    public const uint TailEndVirtualAddressExclusive = 0x00FE67AC;

    public const ushort StackCleanupBytes = 0x14;
    public const ushort ReturnImmediateBytes = 4;

    public const string CapturedTailBytes =
        "83 C4 14 32 C0 5F 5E 5B 8B E5 5D C2 04 00";

    public static ImmutableArray<UnreliableReceiveZeroReturnInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveZeroReturnInstructionEvidence(
                "Add 0x14 bytes to ESP",
                StackCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(StackCleanupRawFileOffset),
                "83 C4 14"),
            new UnreliableReceiveZeroReturnInstructionEvidence(
                "Clear AL while retaining the upper 24 bits of EAX",
                XorAlRawFileOffset,
                RawFileOffsetToVirtualAddress(XorAlRawFileOffset),
                "32 C0"),
            new UnreliableReceiveZeroReturnInstructionEvidence(
                "Pop EDI",
                PopEdiRawFileOffset,
                RawFileOffsetToVirtualAddress(PopEdiRawFileOffset),
                "5F"),
            new UnreliableReceiveZeroReturnInstructionEvidence(
                "Pop ESI",
                PopEsiRawFileOffset,
                RawFileOffsetToVirtualAddress(PopEsiRawFileOffset),
                "5E"),
            new UnreliableReceiveZeroReturnInstructionEvidence(
                "Pop EBX",
                PopEbxRawFileOffset,
                RawFileOffsetToVirtualAddress(PopEbxRawFileOffset),
                "5B"),
            new UnreliableReceiveZeroReturnInstructionEvidence(
                "Reset ESP from EBP",
                FrameStackPointerResetRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameStackPointerResetRawFileOffset),
                "8B E5"),
            new UnreliableReceiveZeroReturnInstructionEvidence(
                "Pop EBP",
                PopEbpRawFileOffset,
                RawFileOffsetToVirtualAddress(PopEbpRawFileOffset),
                "5D"),
            new UnreliableReceiveZeroReturnInstructionEvidence(
                "Return and separately discard four bytes",
                ReturnRawFileOffset,
                RawFileOffsetToVirtualAddress(ReturnRawFileOffset),
                "C2 04 00"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static UnreliableReceiveZeroReturnPlan Project(
        UnreliableReceiveZeroReturnCapture capture)
    {
        if (capture.PriorForwardCallReturnedNormally is not bool returnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveZeroReturnOutcome.MissingPriorForwardCallReturnCapture,
                priorForwardCallNormalGatePassed: null,
                unreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue,
                stopAt: TailStartRawFileOffset);
        }

        if (!returnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveZeroReturnOutcome.PriorForwardCallDidNotReturnNormally,
                priorForwardCallNormalGatePassed: false,
                unreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue,
                stopAt: TailStartRawFileOffset);
        }

        uint? eaxAfterXorAl = capture.HelperReturnEax is uint helperReturnEax
            ? helperReturnEax & 0xFFFFFF00
            : null;

        return new UnreliableReceiveZeroReturnPlan(
            capture,
            UnreliableReceiveZeroReturnOutcome.ZeroReturnTailReached,
            PriorForwardCallNormalGatePassed: true,
            UnreachedHelperReturnEaxCaptureProvided: false,
            StackCleanupProjected: true,
            StackCleanupBytes: StackCleanupBytes,
            AlZeroProjected: true,
            AlAfterXor: 0,
            EaxAfterXorAl: eaxAfterXorAl,
            EdiPopped: true,
            EsiPopped: true,
            EbxPopped: true,
            FrameStackPointerReset: true,
            EbpPopped: true,
            ReturnReached: true,
            ReturnImmediateBytes: ReturnImmediateBytes,
            StopAtRawFileOffsetExclusive: TailEndRawFileOffsetExclusive);
    }

    private static UnreliableReceiveZeroReturnPlan Stop(
        UnreliableReceiveZeroReturnCapture capture,
        UnreliableReceiveZeroReturnOutcome outcome,
        bool? priorForwardCallNormalGatePassed,
        bool unreachedHelperReturnEaxCaptureProvided,
        uint stopAt) =>
        new(
            capture,
            outcome,
            priorForwardCallNormalGatePassed,
            unreachedHelperReturnEaxCaptureProvided,
            StackCleanupProjected: false,
            StackCleanupBytes: null,
            AlZeroProjected: false,
            AlAfterXor: null,
            EaxAfterXorAl: null,
            EdiPopped: false,
            EsiPopped: false,
            EbxPopped: false,
            FrameStackPointerReset: false,
            EbpPopped: false,
            ReturnReached: false,
            ReturnImmediateBytes: null,
            stopAt);
}

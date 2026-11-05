using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateFieldSelectionCapture(
    uint? EntryESI,
    uint? EntryEBP,
    uint? FreshEsiPlus14,
    uint? FreshEsiPlus10);

public enum CapacityFinalCalleeAlternateFieldSelectionOutcome
{
    MissingEntryESI,
    MissingFreshEsiPlus14,
    MissingEntryEBP,
    FirstFreshEsiPlus14NonzeroToBoundary,
    MissingFreshEsiPlus10,
    SecondFreshEsiPlus10ZeroToTarget,
    SecondFreshEsiPlus10NonzeroToBoundary
}

public readonly record struct CapacityFinalCalleeAlternateFieldSelectionInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeAlternateFieldSelectionPlan(
    CapacityFinalCalleeAlternateFieldSelectionCapture Capture,
    CapacityFinalCalleeAlternateFieldSelectionOutcome Outcome,
    bool EntryESICaptured,
    uint? FreshEsiPlus14Address,
    bool FreshEsiPlus14ReadReached,
    bool FreshEsiPlus14Captured,
    uint? FirstEAX,
    bool FirstLEAReached,
    uint? FirstECX,
    bool EntryEBPCaptured,
    bool FirstWriteReached,
    uint? FirstWriteAddress,
    uint? FirstWriteValue,
    bool FirstTestReached,
    bool? FirstTestIsZero,
    bool FirstBranchReached,
    bool? FirstBranchTaken,
    uint? FirstBranchSelectedDestinationRawFileOffset,
    uint? FreshEsiPlus10Address,
    bool FreshEsiPlus10ReadReached,
    bool FreshEsiPlus10Captured,
    uint? SecondEAX,
    bool SecondLEAReached,
    uint? SecondECX,
    bool SecondWriteReached,
    uint? SecondWriteAddress,
    uint? SecondWriteValue,
    bool SecondTestReached,
    bool? SecondTestIsZero,
    bool SecondBranchReached,
    bool? SecondBranchTaken,
    uint? SecondBranchSelectedDestinationRawFileOffset,
    bool UnreachedFreshEsiPlus14CaptureProvided,
    bool UnreachedEntryEBPCaptureProvided,
    bool UnreachedFreshEsiPlus10CaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the two alternate field selections without following the loop at FC2.
/// Fresh DWORD reads remain independent from the LEA-derived write values.
/// </summary>
public static class CapacityFinalCalleeAlternateFieldSelectionProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8FA8;
    public const uint EntryVirtualAddress = 0x016C9BA8;
    public const uint FirstReadRawFileOffset = 0x012C8FA8;
    public const uint FirstLEARawFileOffset = 0x012C8FAB;
    public const uint FirstWriteRawFileOffset = 0x012C8FAE;
    public const uint FirstTestRawFileOffset = 0x012C8FB1;
    public const uint FirstBranchRawFileOffset = 0x012C8FB3;
    public const uint SecondReadRawFileOffset = 0x012C8FB5;
    public const uint SecondLEARawFileOffset = 0x012C8FB8;
    public const uint SecondWriteRawFileOffset = 0x012C8FBB;
    public const uint SecondTestRawFileOffset = 0x012C8FBE;
    public const uint SecondBranchRawFileOffset = 0x012C8FC0;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8FC2;
    public const uint FirstNonzeroDestinationRawFileOffset = 0x012C8FC2;
    public const uint SecondNonzeroDestinationRawFileOffset = 0x012C8FC2;
    public const uint SecondZeroDestinationRawFileOffset = 0x012C8FFE;

    public const uint EsiPlus14Displacement = 0x14;
    public const uint EsiPlus10Displacement = 0x10;
    public const uint EbpMinus4Displacement = 4;

    public const string CapturedWindowBytes =
        "8B 46 14 8D 4E 14 89 4D FC 85 C0 75 0D "
        + "8B 46 10 8D 4E 10 89 4D FC 85 C0 74 3C";

    public static ImmutableArray<CapacityFinalCalleeAlternateFieldSelectionInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load a fresh DWORD at entry ESI+14 into EAX",
                FirstReadRawFileOffset, "8B 46 14"),
            Instruction("Form entry ESI+14 in ECX without dereferencing it",
                FirstLEARawFileOffset, "8D 4E 14"),
            Instruction("Write the LEA-derived ECX to entry EBP-4",
                FirstWriteRawFileOffset, "89 4D FC"),
            Instruction("Test the fresh EAX loaded from ESI+14",
                FirstTestRawFileOffset, "85 C0"),
            Instruction("Jump to FC2 when the first fresh EAX is nonzero",
                FirstBranchRawFileOffset, "75 0D",
                branchTargetRawFileOffset: FirstNonzeroDestinationRawFileOffset),
            Instruction("Load a separate fresh DWORD at entry ESI+10 into EAX",
                SecondReadRawFileOffset, "8B 46 10"),
            Instruction("Form entry ESI+10 in ECX without dereferencing it",
                SecondLEARawFileOffset, "8D 4E 10"),
            Instruction("Write the second LEA-derived ECX to entry EBP-4",
                SecondWriteRawFileOffset, "89 4D FC"),
            Instruction("Test the fresh EAX loaded from ESI+10",
                SecondTestRawFileOffset, "85 C0"),
            Instruction("Jump to FFE when the second fresh EAX is zero",
                SecondBranchRawFileOffset, "74 3C",
                branchTargetRawFileOffset: SecondZeroDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEsiPlus14Address(uint entryEsi) =>
        unchecked(entryEsi + EsiPlus14Displacement);

    public static uint DeriveEsiPlus10Address(uint entryEsi) =>
        unchecked(entryEsi + EsiPlus10Displacement);

    public static uint DeriveEbpMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus4Displacement);

    public static CapacityFinalCalleeAlternateFieldSelectionPlan Project(
        CapacityFinalCalleeAlternateFieldSelectionCapture capture)
    {
        if (capture.EntryESI is not uint entryEsi)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlternateFieldSelectionOutcome.MissingEntryESI,
                unreachedFreshEsiPlus14CaptureProvided: capture.FreshEsiPlus14.HasValue,
                unreachedEntryEBPCaptureProvided: capture.EntryEBP.HasValue,
                unreachedFreshEsiPlus10CaptureProvided: capture.FreshEsiPlus10.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint firstReadAddress = DeriveEsiPlus14Address(entryEsi);
        if (capture.FreshEsiPlus14 is not uint firstEax)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlternateFieldSelectionOutcome.MissingFreshEsiPlus14,
                entryEsiCaptured: true,
                freshEsiPlus14Address: firstReadAddress,
                freshEsiPlus14ReadReached: true,
                unreachedEntryEBPCaptureProvided: capture.EntryEBP.HasValue,
                unreachedFreshEsiPlus10CaptureProvided: capture.FreshEsiPlus10.HasValue,
                stopAt: FirstReadRawFileOffset);
        }

        uint firstEcx = firstReadAddress;
        if (capture.EntryEBP is not uint entryEbp)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlternateFieldSelectionOutcome.MissingEntryEBP,
                entryEsiCaptured: true,
                freshEsiPlus14Address: firstReadAddress,
                freshEsiPlus14ReadReached: true,
                freshEsiPlus14Captured: true,
                firstEax: firstEax,
                firstLeaReached: true,
                firstEcx: firstEcx,
                unreachedFreshEsiPlus10CaptureProvided: capture.FreshEsiPlus10.HasValue,
                stopAt: FirstWriteRawFileOffset);
        }

        uint firstWriteAddress = DeriveEbpMinus4Address(entryEbp);
        bool firstIsZero = firstEax == 0;
        if (!firstIsZero)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlternateFieldSelectionOutcome
                    .FirstFreshEsiPlus14NonzeroToBoundary,
                entryEsiCaptured: true,
                freshEsiPlus14Address: firstReadAddress,
                freshEsiPlus14ReadReached: true,
                freshEsiPlus14Captured: true,
                firstEax: firstEax,
                firstLeaReached: true,
                firstEcx: firstEcx,
                entryEbpCaptured: true,
                firstWriteReached: true,
                firstWriteAddress: firstWriteAddress,
                firstWriteValue: firstEcx,
                firstTestReached: true,
                firstTestIsZero: false,
                firstBranchReached: true,
                firstBranchTaken: true,
                firstBranchSelectedDestinationRawFileOffset:
                    FirstNonzeroDestinationRawFileOffset,
                unreachedFreshEsiPlus10CaptureProvided: capture.FreshEsiPlus10.HasValue,
                stopAt: FirstNonzeroDestinationRawFileOffset);
        }

        uint secondReadAddress = DeriveEsiPlus10Address(entryEsi);
        if (capture.FreshEsiPlus10 is not uint secondEax)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlternateFieldSelectionOutcome.MissingFreshEsiPlus10,
                entryEsiCaptured: true,
                freshEsiPlus14Address: firstReadAddress,
                freshEsiPlus14ReadReached: true,
                freshEsiPlus14Captured: true,
                firstEax: firstEax,
                firstLeaReached: true,
                firstEcx: firstEcx,
                entryEbpCaptured: true,
                firstWriteReached: true,
                firstWriteAddress: firstWriteAddress,
                firstWriteValue: firstEcx,
                firstTestReached: true,
                firstTestIsZero: true,
                firstBranchReached: true,
                firstBranchTaken: false,
                firstBranchSelectedDestinationRawFileOffset: SecondReadRawFileOffset,
                freshEsiPlus10Address: secondReadAddress,
                freshEsiPlus10ReadReached: true,
                stopAt: SecondReadRawFileOffset);
        }

        uint secondEcx = secondReadAddress;
        uint secondWriteAddress = firstWriteAddress;
        bool secondIsZero = secondEax == 0;
        uint secondDestination = secondIsZero
            ? SecondZeroDestinationRawFileOffset
            : SecondNonzeroDestinationRawFileOffset;

        return BuildPlan(
            capture,
            secondIsZero
                ? CapacityFinalCalleeAlternateFieldSelectionOutcome
                    .SecondFreshEsiPlus10ZeroToTarget
                : CapacityFinalCalleeAlternateFieldSelectionOutcome
                    .SecondFreshEsiPlus10NonzeroToBoundary,
            entryEsiCaptured: true,
            freshEsiPlus14Address: firstReadAddress,
            freshEsiPlus14ReadReached: true,
            freshEsiPlus14Captured: true,
            firstEax: firstEax,
            firstLeaReached: true,
            firstEcx: firstEcx,
            entryEbpCaptured: true,
            firstWriteReached: true,
            firstWriteAddress: firstWriteAddress,
            firstWriteValue: firstEcx,
            firstTestReached: true,
            firstTestIsZero: true,
            firstBranchReached: true,
            firstBranchTaken: false,
            firstBranchSelectedDestinationRawFileOffset: SecondReadRawFileOffset,
            freshEsiPlus10Address: secondReadAddress,
            freshEsiPlus10ReadReached: true,
            freshEsiPlus10Captured: true,
            secondEax: secondEax,
            secondLeaReached: true,
            secondEcx: secondEcx,
            secondWriteReached: true,
            secondWriteAddress: secondWriteAddress,
            secondWriteValue: secondEcx,
            secondTestReached: true,
            secondTestIsZero: secondIsZero,
            secondBranchReached: true,
            secondBranchTaken: secondIsZero,
            secondBranchSelectedDestinationRawFileOffset: secondDestination,
            stopAt: secondDestination);
    }

    private static CapacityFinalCalleeAlternateFieldSelectionPlan BuildPlan(
        CapacityFinalCalleeAlternateFieldSelectionCapture capture,
        CapacityFinalCalleeAlternateFieldSelectionOutcome outcome,
        bool entryEsiCaptured = false,
        uint? freshEsiPlus14Address = null,
        bool freshEsiPlus14ReadReached = false,
        bool freshEsiPlus14Captured = false,
        uint? firstEax = null,
        bool firstLeaReached = false,
        uint? firstEcx = null,
        bool entryEbpCaptured = false,
        bool firstWriteReached = false,
        uint? firstWriteAddress = null,
        uint? firstWriteValue = null,
        bool firstTestReached = false,
        bool? firstTestIsZero = null,
        bool firstBranchReached = false,
        bool? firstBranchTaken = null,
        uint? firstBranchSelectedDestinationRawFileOffset = null,
        uint? freshEsiPlus10Address = null,
        bool freshEsiPlus10ReadReached = false,
        bool freshEsiPlus10Captured = false,
        uint? secondEax = null,
        bool secondLeaReached = false,
        uint? secondEcx = null,
        bool secondWriteReached = false,
        uint? secondWriteAddress = null,
        uint? secondWriteValue = null,
        bool secondTestReached = false,
        bool? secondTestIsZero = null,
        bool secondBranchReached = false,
        bool? secondBranchTaken = null,
        uint? secondBranchSelectedDestinationRawFileOffset = null,
        bool unreachedFreshEsiPlus14CaptureProvided = false,
        bool unreachedEntryEBPCaptureProvided = false,
        bool unreachedFreshEsiPlus10CaptureProvided = false,
        uint stopAt = BoundaryRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            entryEsiCaptured,
            freshEsiPlus14Address,
            freshEsiPlus14ReadReached,
            freshEsiPlus14Captured,
            firstEax,
            firstLeaReached,
            firstEcx,
            entryEbpCaptured,
            firstWriteReached,
            firstWriteAddress,
            firstWriteValue,
            firstTestReached,
            firstTestIsZero,
            firstBranchReached,
            firstBranchTaken,
            firstBranchSelectedDestinationRawFileOffset,
            freshEsiPlus10Address,
            freshEsiPlus10ReadReached,
            freshEsiPlus10Captured,
            secondEax,
            secondLeaReached,
            secondEcx,
            secondWriteReached,
            secondWriteAddress,
            secondWriteValue,
            secondTestReached,
            secondTestIsZero,
            secondBranchReached,
            secondBranchTaken,
            secondBranchSelectedDestinationRawFileOffset,
            unreachedFreshEsiPlus14CaptureProvided,
            unreachedEntryEBPCaptureProvided,
            unreachedFreshEsiPlus10CaptureProvided,
            stopAt);

    private static CapacityFinalCalleeAlternateFieldSelectionInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);
}

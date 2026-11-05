using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeFieldClearTailCapture(
    uint? ValidatedEntryEDX,
    uint? ValidatedEntryEBP,
    uint? FreshGlobalForEntryCompare,
    uint? FreshFrameMinus12,
    uint? FreshFrameMinus4ForZeroStoreAddress,
    uint? FreshGlobalForTailLoad);

public enum CapacityFinalCalleeFieldClearTailOutcome
{
    MissingValidatedEntryEDX,
    MissingFreshGlobalForEntryCompare,
    MissingValidatedEntryEBP,
    MissingFreshFrameMinus12,
    EntryEDXUnsignedBelowGlobal,
    MissingFreshFrameMinus4ForZeroStoreAddress,
    MissingFreshGlobalForTailLoad,
    TailGlobalLoaded
}

public readonly record struct CapacityFinalCalleeFieldClearTailFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public enum CapacityFinalCalleeFieldClearTailReadKind
{
    FreshGlobalForEntryCompare,
    FreshFrameMinus12,
    FreshFrameMinus4ForZeroStoreAddress,
    FreshGlobalForTailLoad
}

public readonly record struct CapacityFinalCalleeFieldClearTailRead(
    uint RawFileOffset,
    CapacityFinalCalleeFieldClearTailReadKind Kind,
    uint Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeFieldClearTailWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeFieldClearTailInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeFieldClearTailPlan(
    CapacityFinalCalleeFieldClearTailCapture Capture,
    CapacityFinalCalleeFieldClearTailOutcome Outcome,
    bool CompareGlobalReadReached,
    bool CompareGlobalCaptured,
    uint? CompareGlobalValue,
    bool CompareReached,
    uint? CompareResult,
    CapacityFinalCalleeFieldClearTailFlags? CompareFlags,
    bool BranchReached,
    bool? BranchTaken,
    uint? SelectedBranchDestinationRawFileOffset,
    uint? FrameMinus12Address,
    bool FrameMinus12ReadReached,
    bool FrameMinus12Captured,
    uint? FrameMinus12,
    uint? FrameMinus4Address,
    bool FrameMinus4ReadReached,
    bool FrameMinus4Captured,
    uint? FrameMinus4ForZeroStoreAddress,
    bool TailGlobalReadReached,
    bool TailGlobalCaptured,
    uint? TailGlobalValue,
    ImmutableArray<CapacityFinalCalleeFieldClearTailRead> Reads,
    ImmutableArray<CapacityFinalCalleeFieldClearTailWrite> Writes,
    bool UnreachedValidatedEntryEBPCaptureProvided,
    bool UnreachedFreshGlobalForEntryCompareCaptureProvided,
    bool UnreachedFreshFrameMinus12CaptureProvided,
    bool UnreachedFreshFrameMinus4CaptureProvided,
    bool UnreachedFreshGlobalForTailLoadCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the ordered CMP, frame-store/reload, unsigned branch, and tail
/// field-clear sequence. Register inputs and each fresh memory read are
/// independent captures.
/// </summary>
public static class CapacityFinalCalleeFieldClearTailProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8FDD;
    public const uint EntryVirtualAddress = 0x016C9BDD;
    public const uint EntryCompareRawFileOffset = 0x012C8FDD;
    public const uint EntryFrameStoreRawFileOffset = 0x012C8FE3;
    public const uint FrameMinus12LoadRawFileOffset = 0x012C8FE6;
    public const uint EntryBranchRawFileOffset = 0x012C8FE9;
    public const uint TailFrameMinus4LoadRawFileOffset = 0x012C8FEF;
    public const uint ZeroStoreRawFileOffset = 0x012C8FF2;
    public const uint TailGlobalLoadRawFileOffset = 0x012C8FF8;
    public const uint WindowBoundaryRawFileOffsetExclusive = 0x012C8FFE;
    public const uint BelowDestinationRawFileOffset = 0x012C8E76;
    public const uint TailCompletionRawFileOffset = WindowBoundaryRawFileOffsetExclusive;

    public const uint FrameMinus12Displacement = 12;
    public const uint FrameMinus4Displacement = 4;
    public const uint GlobalValueVirtualAddress = 0x01EDADD8;
    public const string CapturedWindowBytes =
        "3B 15 D8 AD ED 01 89 55 FC 8B 55 F4 0F 82 87 FE FF FF "
        + "8B 4D FC C7 01 00 00 00 00 8B 1D D8 AD ED 01";

    public static ImmutableArray<CapacityFinalCalleeFieldClearTailInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry EDX with the fresh global DWORD",
                EntryCompareRawFileOffset, "3B 15 D8 AD ED 01",
                peVirtualAddressOperand: GlobalValueVirtualAddress),
            Instruction("Store the original entry EDX at entry EBP-4",
                EntryFrameStoreRawFileOffset, "89 55 FC"),
            Instruction("Reload a fresh DWORD from entry EBP-12 into EDX",
                FrameMinus12LoadRawFileOffset, "8B 55 F4"),
            Instruction("JB to the below destination using the original CMP flags",
                EntryBranchRawFileOffset, "0F 82 87 FE FF FF",
                branchTargetRawFileOffset: BelowDestinationRawFileOffset),
            Instruction("Reload a fresh DWORD from entry EBP-4 into ECX",
                TailFrameMinus4LoadRawFileOffset, "8B 4D FC"),
            Instruction("Store zero at the independently captured ECX address",
                ZeroStoreRawFileOffset, "C7 01 00 00 00 00"),
            Instruction("Load a fresh global DWORD into EBX after the zero store",
                TailGlobalLoadRawFileOffset, "8B 1D D8 AD ED 01",
                peVirtualAddressOperand: GlobalValueVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFrameAddress(uint entryEbp, uint displacement) =>
        unchecked(entryEbp - displacement);

    public static CapacityFinalCalleeFieldClearTailPlan Project(
        CapacityFinalCalleeFieldClearTailCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeFieldClearTailRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeFieldClearTailWrite>();

        if (capture.ValidatedEntryEDX is not uint entryEdx)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFieldClearTailOutcome.MissingValidatedEntryEDX,
                reads,
                writes,
                unreachedValidatedEntryEbpCaptureProvided:
                    capture.ValidatedEntryEBP.HasValue,
                unreachedFreshGlobalForEntryCompareCaptureProvided:
                    capture.FreshGlobalForEntryCompare.HasValue,
                unreachedFreshFrameMinus12CaptureProvided:
                    capture.FreshFrameMinus12.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4ForZeroStoreAddress.HasValue,
                unreachedFreshGlobalForTailLoadCaptureProvided:
                    capture.FreshGlobalForTailLoad.HasValue,
                stopAt: EntryCompareRawFileOffset);
        }

        reads.Add(new(
            EntryCompareRawFileOffset,
            CapacityFinalCalleeFieldClearTailReadKind.FreshGlobalForEntryCompare,
            GlobalValueVirtualAddress,
            capture.FreshGlobalForEntryCompare.HasValue,
            capture.FreshGlobalForEntryCompare));
        if (capture.FreshGlobalForEntryCompare is not uint compareGlobal)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFieldClearTailOutcome
                    .MissingFreshGlobalForEntryCompare,
                reads,
                writes,
                compareGlobalReadReached: true,
                unreachedValidatedEntryEbpCaptureProvided:
                    capture.ValidatedEntryEBP.HasValue,
                unreachedFreshFrameMinus12CaptureProvided:
                    capture.FreshFrameMinus12.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4ForZeroStoreAddress.HasValue,
                unreachedFreshGlobalForTailLoadCaptureProvided:
                    capture.FreshGlobalForTailLoad.HasValue,
                stopAt: EntryCompareRawFileOffset);
        }

        uint compareResult = unchecked(entryEdx - compareGlobal);
        CapacityFinalCalleeFieldClearTailFlags compareFlags =
            CalculateFlags(entryEdx, compareGlobal, compareResult);
        if (capture.ValidatedEntryEBP is not uint entryEbp)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFieldClearTailOutcome
                    .MissingValidatedEntryEBP,
                reads,
                writes,
                compareGlobalReadReached: true,
                compareGlobalCaptured: true,
                compareGlobalValue: compareGlobal,
                compareReached: true,
                compareResult: compareResult,
                compareFlags: compareFlags,
                unreachedFreshFrameMinus12CaptureProvided:
                    capture.FreshFrameMinus12.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4ForZeroStoreAddress.HasValue,
                unreachedFreshGlobalForTailLoadCaptureProvided:
                    capture.FreshGlobalForTailLoad.HasValue,
                stopAt: EntryFrameStoreRawFileOffset);
        }

        uint frameMinus4Address =
            DeriveFrameAddress(entryEbp, FrameMinus4Displacement);
        writes.Add(new(
            EntryFrameStoreRawFileOffset,
            frameMinus4Address,
            entryEdx));

        uint frameMinus12Address =
            DeriveFrameAddress(entryEbp, FrameMinus12Displacement);
        reads.Add(new(
            FrameMinus12LoadRawFileOffset,
            CapacityFinalCalleeFieldClearTailReadKind.FreshFrameMinus12,
            frameMinus12Address,
            capture.FreshFrameMinus12.HasValue,
            capture.FreshFrameMinus12));
        if (capture.FreshFrameMinus12 is not uint freshFrameMinus12)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFieldClearTailOutcome.MissingFreshFrameMinus12,
                reads,
                writes,
                compareGlobalReadReached: true,
                compareGlobalCaptured: true,
                compareGlobalValue: compareGlobal,
                compareReached: true,
                compareResult: compareResult,
                compareFlags: compareFlags,
                frameMinus12Address: frameMinus12Address,
                frameMinus12ReadReached: true,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4ForZeroStoreAddress.HasValue,
                unreachedFreshGlobalForTailLoadCaptureProvided:
                    capture.FreshGlobalForTailLoad.HasValue,
                stopAt: FrameMinus12LoadRawFileOffset);
        }

        bool branchTaken = compareFlags.Carry;
        uint branchDestination = branchTaken
            ? BelowDestinationRawFileOffset
            : TailFrameMinus4LoadRawFileOffset;
        if (branchTaken)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFieldClearTailOutcome
                    .EntryEDXUnsignedBelowGlobal,
                reads,
                writes,
                compareGlobalReadReached: true,
                compareGlobalCaptured: true,
                compareGlobalValue: compareGlobal,
                compareReached: true,
                compareResult: compareResult,
                compareFlags: compareFlags,
                branchReached: true,
                branchTaken: true,
                selectedBranchDestinationRawFileOffset: branchDestination,
                frameMinus12Address: frameMinus12Address,
                frameMinus12ReadReached: true,
                frameMinus12Captured: true,
                frameMinus12: freshFrameMinus12,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4ForZeroStoreAddress.HasValue,
                unreachedFreshGlobalForTailLoadCaptureProvided:
                    capture.FreshGlobalForTailLoad.HasValue,
                stopAt: BelowDestinationRawFileOffset);
        }

        uint? tailFrameMinus4Address = frameMinus4Address;
        reads.Add(new(
            TailFrameMinus4LoadRawFileOffset,
            CapacityFinalCalleeFieldClearTailReadKind
                .FreshFrameMinus4ForZeroStoreAddress,
            frameMinus4Address,
            capture.FreshFrameMinus4ForZeroStoreAddress.HasValue,
            capture.FreshFrameMinus4ForZeroStoreAddress));
        if (capture.FreshFrameMinus4ForZeroStoreAddress
            is not uint freshFrameMinus4)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFieldClearTailOutcome
                    .MissingFreshFrameMinus4ForZeroStoreAddress,
                reads,
                writes,
                compareGlobalReadReached: true,
                compareGlobalCaptured: true,
                compareGlobalValue: compareGlobal,
                compareReached: true,
                compareResult: compareResult,
                compareFlags: compareFlags,
                branchReached: true,
                branchTaken: false,
                selectedBranchDestinationRawFileOffset: branchDestination,
                frameMinus12Address: frameMinus12Address,
                frameMinus12ReadReached: true,
                frameMinus12Captured: true,
                frameMinus12: freshFrameMinus12,
                frameMinus4Address: tailFrameMinus4Address,
                frameMinus4ReadReached: true,
                unreachedFreshGlobalForTailLoadCaptureProvided:
                    capture.FreshGlobalForTailLoad.HasValue,
                stopAt: TailFrameMinus4LoadRawFileOffset);
        }

        writes.Add(new(
            ZeroStoreRawFileOffset,
            freshFrameMinus4,
            0));

        reads.Add(new(
            TailGlobalLoadRawFileOffset,
            CapacityFinalCalleeFieldClearTailReadKind.FreshGlobalForTailLoad,
            GlobalValueVirtualAddress,
            capture.FreshGlobalForTailLoad.HasValue,
            capture.FreshGlobalForTailLoad));
        if (capture.FreshGlobalForTailLoad is not uint tailGlobal)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFieldClearTailOutcome
                    .MissingFreshGlobalForTailLoad,
                reads,
                writes,
                compareGlobalReadReached: true,
                compareGlobalCaptured: true,
                compareGlobalValue: compareGlobal,
                compareReached: true,
                compareResult: compareResult,
                compareFlags: compareFlags,
                branchReached: true,
                branchTaken: false,
                selectedBranchDestinationRawFileOffset: branchDestination,
                frameMinus12Address: frameMinus12Address,
                frameMinus12ReadReached: true,
                frameMinus12Captured: true,
                frameMinus12: freshFrameMinus12,
                frameMinus4Address: tailFrameMinus4Address,
                frameMinus4ReadReached: true,
                frameMinus4Captured: true,
                frameMinus4ForZeroStoreAddress: freshFrameMinus4,
                tailGlobalReadReached: true,
                stopAt: TailGlobalLoadRawFileOffset);
        }

        return BuildPlan(
            capture,
            CapacityFinalCalleeFieldClearTailOutcome.TailGlobalLoaded,
            reads,
            writes,
            compareGlobalReadReached: true,
            compareGlobalCaptured: true,
            compareGlobalValue: compareGlobal,
            compareReached: true,
            compareResult: compareResult,
            compareFlags: compareFlags,
            branchReached: true,
            branchTaken: false,
            selectedBranchDestinationRawFileOffset: branchDestination,
            frameMinus12Address: frameMinus12Address,
            frameMinus12ReadReached: true,
            frameMinus12Captured: true,
            frameMinus12: freshFrameMinus12,
            frameMinus4Address: tailFrameMinus4Address,
            frameMinus4ReadReached: true,
            frameMinus4Captured: true,
            frameMinus4ForZeroStoreAddress: freshFrameMinus4,
            tailGlobalReadReached: true,
            tailGlobalCaptured: true,
            tailGlobalValue: tailGlobal,
            stopAt: TailCompletionRawFileOffset);
    }

    private static CapacityFinalCalleeFieldClearTailPlan BuildPlan(
        CapacityFinalCalleeFieldClearTailCapture capture,
        CapacityFinalCalleeFieldClearTailOutcome outcome,
        ImmutableArray<CapacityFinalCalleeFieldClearTailRead>.Builder reads,
        ImmutableArray<CapacityFinalCalleeFieldClearTailWrite>.Builder writes,
        bool compareGlobalReadReached = false,
        bool compareGlobalCaptured = false,
        uint? compareGlobalValue = null,
        bool compareReached = false,
        uint? compareResult = null,
        CapacityFinalCalleeFieldClearTailFlags? compareFlags = null,
        bool branchReached = false,
        bool? branchTaken = null,
        uint? selectedBranchDestinationRawFileOffset = null,
        uint? frameMinus12Address = null,
        bool frameMinus12ReadReached = false,
        bool frameMinus12Captured = false,
        uint? frameMinus12 = null,
        uint? frameMinus4Address = null,
        bool frameMinus4ReadReached = false,
        bool frameMinus4Captured = false,
        uint? frameMinus4ForZeroStoreAddress = null,
        bool tailGlobalReadReached = false,
        bool tailGlobalCaptured = false,
        uint? tailGlobalValue = null,
        bool unreachedValidatedEntryEbpCaptureProvided = false,
        bool unreachedFreshGlobalForEntryCompareCaptureProvided = false,
        bool unreachedFreshFrameMinus12CaptureProvided = false,
        bool unreachedFreshFrameMinus4CaptureProvided = false,
        bool unreachedFreshGlobalForTailLoadCaptureProvided = false,
        uint stopAt = WindowBoundaryRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            compareGlobalReadReached,
            compareGlobalCaptured,
            compareGlobalValue,
            compareReached,
            compareResult,
            compareFlags,
            branchReached,
            branchTaken,
            selectedBranchDestinationRawFileOffset,
            frameMinus12Address,
            frameMinus12ReadReached,
            frameMinus12Captured,
            frameMinus12,
            frameMinus4Address,
            frameMinus4ReadReached,
            frameMinus4Captured,
            frameMinus4ForZeroStoreAddress,
            tailGlobalReadReached,
            tailGlobalCaptured,
            tailGlobalValue,
            reads.ToImmutable(),
            writes.ToImmutable(),
            unreachedValidatedEntryEbpCaptureProvided,
            unreachedFreshGlobalForEntryCompareCaptureProvided,
            unreachedFreshFrameMinus12CaptureProvided,
            unreachedFreshFrameMinus4CaptureProvided,
            unreachedFreshGlobalForTailLoadCaptureProvided,
            stopAt);

    private static CapacityFinalCalleeFieldClearTailFlags CalculateFlags(
        uint left,
        uint right,
        uint result)
    {
        byte lowByte = (byte)result;
        bool evenParity = (System.Numerics.BitOperations.PopCount(lowByte) & 1) == 0;
        bool auxiliaryCarry = ((left ^ right ^ result) & 0x10) != 0;
        bool overflow = ((left ^ right) & (left ^ result) & 0x80000000) != 0;

        return new(
            Carry: left < right,
            Parity: evenParity,
            AuxiliaryCarry: auxiliaryCarry,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: overflow);
    }

    private static CapacityFinalCalleeFieldClearTailInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            peVirtualAddressOperand,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);
}

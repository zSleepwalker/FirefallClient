using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeLeafThresholdGateCapture(
    uint? EntryEDX,
    uint? FreshGlobalValue,
    uint? EntryEBP,
    uint? FreshEbpMinusC);

public enum CapacityFinalCalleeLargeLeafThresholdGateOutcome
{
    MissingFreshGlobalValue,
    MissingEntryEDX,
    MissingEntryEBP,
    MissingFreshEbpMinusC,
    UnsignedBelowBranchTarget,
    UnsignedNotBelowFallthrough
}

public enum CapacityFinalCalleeLargeLeafThresholdGateEventKind
{
    Read,
    Compare,
    Write,
    Branch
}

public readonly record struct CapacityFinalCalleeLargeLeafThresholdGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeLeafThresholdGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeLargeLeafThresholdGateRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeLeafThresholdGateWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public readonly record struct CapacityFinalCalleeLargeLeafThresholdGateEvent(
    CapacityFinalCalleeLargeLeafThresholdGateEventKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    bool AddressCaptured = false,
    bool ValueCaptured = false,
    uint? Value = null,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeLargeLeafThresholdGateFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public sealed record CapacityFinalCalleeLargeLeafThresholdGatePlan(
    CapacityFinalCalleeLargeLeafThresholdGateCapture Capture,
    CapacityFinalCalleeLargeLeafThresholdGateOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeLargeLeafThresholdGateRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeLeafThresholdGateRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeLeafThresholdGateWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeLeafThresholdGateWrite>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeLeafThresholdGateEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeLeafThresholdGateEvent>.Empty;
    public bool GlobalReadReached { get; init; }
    public bool GlobalValueCaptured { get; init; }
    public uint? GlobalAddress { get; init; }
    public uint? GlobalValue { get; init; }
    public bool EntryEDXCaptured { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeLargeLeafThresholdGateFlags? CompareFlags { get; init; }
    public CapacityFinalCalleeLargeLeafThresholdGateFlags? FlagsAtStop => CompareFlags;
    public bool StoreReached { get; init; }
    public uint? StoreAddress { get; init; }
    public uint? StoreValue { get; init; }
    public bool FreshFrameReadReached { get; init; }
    public uint? FreshFrameAddress { get; init; }
    public bool FreshFrameCaptured { get; init; }
    public uint? FreshFrameValue { get; init; }
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public CapacityFinalCalleeLargeLeafThresholdGateFlags? BranchFlags { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public bool UnreachedEntryEDXCaptureProvided { get; init; }
    public bool UnreachedEntryEBPCaptureProvided { get; init; }
    public bool UnreachedFreshFrameCaptureProvided { get; init; }
    public bool ExactCleanAdmission { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the large-leaf global threshold compare, EDX store, independent
/// fresh frame reload, and bounded JB using the original CMP flags.
/// </summary>
public static class CapacityFinalCalleeLargeLeafThresholdGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C920B;
    public const uint EntryVirtualAddress = 0x016C9E0B;
    public const uint CompareRawFileOffset = 0x012C920B;
    public const uint StoreRawFileOffset = 0x012C9211;
    public const uint FreshFrameReadRawFileOffset = 0x012C9214;
    public const uint BranchRawFileOffset = 0x012C9217;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C921D;
    public const uint BranchTargetRawFileOffset = 0x012C8E76;
    public const uint FallthroughRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const uint GlobalValueAddress = 0x01EDADD8;
    public const uint EbpMinus4Displacement = 4;
    public const uint EbpMinusCDisplacement = 0x0C;

    public const string CapturedWindowBytes =
        "3B 15 D8 AD ED 01 89 55 FC 8B 55 F4 0F 82 59 FC FF FF";

    public static ImmutableArray<CapacityFinalCalleeLargeLeafThresholdGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry EDX with the fresh DWORD at absolute address 01EDADD8",
                CompareRawFileOffset, "3B 15 D8 AD ED 01"),
            Instruction("Store entry EDX to entry EBP-4 without changing CMP flags",
                StoreRawFileOffset, "89 55 FC"),
            Instruction("Reload the fresh DWORD at entry EBP-C into EDX",
                FreshFrameReadRawFileOffset, "8B 55 F4"),
            Instruction("Branch to 8E76 when the earlier unsigned comparison was below",
                BranchRawFileOffset, "0F 82 59 FC FF FF",
                branchTargetRawFileOffset: BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEbpMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus4Displacement);

    public static uint DeriveEbpMinusCAddress(uint entryEbp) =>
        unchecked(entryEbp - EbpMinusCDisplacement);

    public static CapacityFinalCalleeLargeLeafThresholdGatePlan Project(
        CapacityFinalCalleeLargeLeafThresholdGateCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeLeafThresholdGateRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeLeafThresholdGateWrite>();
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeLeafThresholdGateEvent>();

        bool globalReadReached = false;
        bool globalCaptured = false;
        bool edxCaptured = false;
        bool compareReached = false;
        uint? compareLeft = null;
        uint? compareRight = null;
        uint? compareResult = null;
        CapacityFinalCalleeLargeLeafThresholdGateFlags? compareFlags = null;
        bool storeReached = false;
        uint? storeAddress = null;
        uint? storeValue = null;
        bool frameReadReached = false;
        uint? frameAddress = null;
        bool frameCaptured = false;
        uint? frameValue = null;
        bool branchReached = false;
        bool? branchTaken = null;
        uint? branchDestination = null;

        CapacityFinalCalleeLargeLeafThresholdGatePlan Finish(
            CapacityFinalCalleeLargeLeafThresholdGateOutcome outcome,
            uint stopAt,
            bool exactCleanAdmission = false) =>
            new(capture, outcome)
            {
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                Events = events.ToImmutable(),
                GlobalReadReached = globalReadReached,
                GlobalValueCaptured = globalCaptured,
                GlobalAddress = globalReadReached ? GlobalValueAddress : null,
                GlobalValue = globalCaptured ? capture.FreshGlobalValue : null,
                EntryEDXCaptured = edxCaptured,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareRight,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                StoreReached = storeReached,
                StoreAddress = storeAddress,
                StoreValue = storeValue,
                FreshFrameReadReached = frameReadReached,
                FreshFrameAddress = frameAddress,
                FreshFrameCaptured = frameCaptured,
                FreshFrameValue = frameValue,
                BranchReached = branchReached,
                BranchTaken = branchTaken,
                BranchFlags = branchReached ? compareFlags : null,
                BranchSelectedDestinationRawFileOffset = branchDestination,
                UnreachedEntryEDXCaptureProvided =
                    !globalReadReached && capture.EntryEDX.HasValue,
                UnreachedEntryEBPCaptureProvided =
                    !storeReached && capture.EntryEBP.HasValue,
                UnreachedFreshFrameCaptureProvided =
                    !frameReadReached && capture.FreshEbpMinusC.HasValue,
                ExactCleanAdmission = exactCleanAdmission,
                StopAtRawFileOffsetExclusive = stopAt
            };

        globalReadReached = true;
        edxCaptured = capture.EntryEDX.HasValue;
        globalCaptured = capture.FreshGlobalValue.HasValue;
        reads.Add(new(
            CompareRawFileOffset,
            GlobalValueAddress,
            AddressCaptured: true,
            ValueCaptured: globalCaptured,
            Value: capture.FreshGlobalValue));
        events.Add(new(
            CapacityFinalCalleeLargeLeafThresholdGateEventKind.Read,
            CompareRawFileOffset,
            GlobalValueAddress,
            AddressCaptured: true,
            ValueCaptured: globalCaptured,
            Value: capture.FreshGlobalValue));
        if (capture.FreshGlobalValue is not uint globalValue)
        {
            return Finish(
                CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingFreshGlobalValue,
                CompareRawFileOffset);
        }

        if (capture.EntryEDX is not uint entryEdx)
        {
            return Finish(
                CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingEntryEDX,
                CompareRawFileOffset);
        }

        edxCaptured = true;
        compareReached = true;
        compareLeft = entryEdx;
        compareRight = globalValue;
        compareResult = unchecked(entryEdx - globalValue);
        compareFlags = CalculateCompareFlags(
            entryEdx, globalValue, compareResult.Value);
        events.Add(new(
            CapacityFinalCalleeLargeLeafThresholdGateEventKind.Compare,
            CompareRawFileOffset,
            Left: compareLeft,
            Right: compareRight,
            Result: compareResult,
            Flags: compareFlags));

        if (capture.EntryEBP is not uint entryEbp)
        {
            return Finish(
                CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingEntryEBP,
                StoreRawFileOffset);
        }

        storeReached = true;
        storeAddress = DeriveEbpMinus4Address(entryEbp);
        storeValue = entryEdx;
        writes.Add(new(
            StoreRawFileOffset, storeAddress, true, true, storeValue));
        events.Add(new(
            CapacityFinalCalleeLargeLeafThresholdGateEventKind.Write,
            StoreRawFileOffset,
            storeAddress,
            AddressCaptured: true,
            ValueCaptured: true,
            Value: storeValue));

        frameReadReached = true;
        frameAddress = DeriveEbpMinusCAddress(entryEbp);
        frameCaptured = capture.FreshEbpMinusC.HasValue;
        reads.Add(new(
            FreshFrameReadRawFileOffset,
            frameAddress,
            AddressCaptured: true,
            ValueCaptured: frameCaptured,
            Value: capture.FreshEbpMinusC));
        events.Add(new(
            CapacityFinalCalleeLargeLeafThresholdGateEventKind.Read,
            FreshFrameReadRawFileOffset,
            frameAddress,
            AddressCaptured: true,
            ValueCaptured: frameCaptured,
            Value: capture.FreshEbpMinusC));
        if (capture.FreshEbpMinusC is not uint freshFrame)
        {
            return Finish(
                CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingFreshEbpMinusC,
                FreshFrameReadRawFileOffset);
        }

        frameValue = freshFrame;
        branchReached = true;
        branchTaken = compareFlags!.Value.Carry;
        branchDestination = branchTaken.Value
            ? BranchTargetRawFileOffset
            : FallthroughRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeLargeLeafThresholdGateEventKind.Branch,
            BranchRawFileOffset,
            Flags: compareFlags,
            BranchTaken: branchTaken,
            SelectedDestinationRawFileOffset: branchDestination));

        return Finish(
            branchTaken.Value
                ? CapacityFinalCalleeLargeLeafThresholdGateOutcome.UnsignedBelowBranchTarget
                : CapacityFinalCalleeLargeLeafThresholdGateOutcome.UnsignedNotBelowFallthrough,
            branchDestination.Value,
            exactCleanAdmission: true);
    }

    private static CapacityFinalCalleeLargeLeafThresholdGateFlags CalculateCompareFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeLargeLeafThresholdGateInstructionEvidence Instruction(
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

using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeFrameThresholdCapture(
    uint? EntryEDX,
    uint? EntryEBX,
    uint? EntryEBP,
    uint? FreshEdxPlus8,
    uint? FreshEbpMinus10);

public enum CapacityFinalCalleeLargeFrameThresholdOutcome
{
    MissingEntryEDX,
    MissingFreshEdxPlus8,
    MissingEntryEBX,
    MissingEntryEBP,
    MissingFreshEbpMinus10,
    UnsignedBelowBranchTarget,
    UnsignedNotBelowFallthrough
}

public enum CapacityFinalCalleeLargeFrameThresholdEventKind
{
    Read,
    Compare,
    Write,
    Branch
}

public readonly record struct CapacityFinalCalleeLargeFrameThresholdFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeFrameThresholdInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeLargeFrameThresholdRead(
    uint RawFileOffset,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeFrameThresholdWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public readonly record struct CapacityFinalCalleeLargeFrameThresholdEvent(
    CapacityFinalCalleeLargeFrameThresholdEventKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    bool AddressCaptured = false,
    bool ValueCaptured = false,
    uint? Value = null,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeLargeFrameThresholdFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public sealed record CapacityFinalCalleeLargeFrameThresholdPlan(
    CapacityFinalCalleeLargeFrameThresholdCapture Capture,
    CapacityFinalCalleeLargeFrameThresholdOutcome Outcome)
{
    public bool EntryEDXCaptured { get; init; }
    public bool EntryEBXCaptured { get; init; }
    public bool EntryEBPCaptured { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeFrameThresholdRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeFrameThresholdRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeFrameThresholdWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeFrameThresholdWrite>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeFrameThresholdEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeFrameThresholdEvent>.Empty;
    public bool FreshEdxPlus8ReadReached { get; init; }
    public uint? FreshEdxPlus8Address { get; init; }
    public bool FreshEdxPlus8Captured { get; init; }
    public uint? FreshEdxPlus8 { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeLargeFrameThresholdFlags? CompareFlags { get; init; }
    public bool StoreReached { get; init; }
    public uint? StoreAddress { get; init; }
    public bool StoreValueCaptured { get; init; }
    public uint? StoreValue { get; init; }
    public bool FreshEbpMinus10ReadReached { get; init; }
    public uint? FreshEbpMinus10Address { get; init; }
    public bool FreshEbpMinus10Captured { get; init; }
    public uint? FreshEbpMinus10 { get; init; }
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public CapacityFinalCalleeLargeFrameThresholdFlags? BranchFlags { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public bool UnreachedEntryEBXCaptureProvided { get; init; }
    public bool UnreachedEntryEBPCaptureProvided { get; init; }
    public bool UnreachedFreshEdxPlus8CaptureProvided { get; init; }
    public bool UnreachedFreshEbpMinus10CaptureProvided { get; init; }
    public bool ExactCleanAdmission { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the large-frame threshold compare, intervening frame store and fresh
/// frame load, and the bounded JB. The branch retains flags from the first CMP.
/// </summary>
public static class CapacityFinalCalleeLargeFrameThresholdProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C91A8;
    public const uint EntryVirtualAddress = 0x016C9DA8;
    public const uint EdxPlus8ReadRawFileOffset = 0x012C91A8;
    public const uint CompareRawFileOffset = 0x012C91AB;
    public const uint StoreRawFileOffset = 0x012C91AD;
    public const uint EbpMinus10ReadRawFileOffset = 0x012C91B0;
    public const uint BranchRawFileOffset = 0x012C91B3;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C91B9;
    public const uint BranchTargetRawFileOffset = 0x012C8E76;
    public const uint FallthroughRawFileOffset = BoundaryRawFileOffsetExclusive;

    public const uint EdxPlus8Displacement = 8;
    public const uint EbpMinus14Displacement = 0x14;
    public const uint EbpMinus10Displacement = 0x10;

    public const string CapturedWindowBytes =
        "8B 4A 08 3B CB 89 4D EC 8B 4D F0 0F 82 BD FC FF FF";

    public static ImmutableArray<CapacityFinalCalleeLargeFrameThresholdInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the fresh DWORD at entry EDX+8 into ECX",
                EdxPlus8ReadRawFileOffset, "8B 4A 08"),
            Instruction("Compare loaded ECX with entry EBX",
                CompareRawFileOffset, "3B CB"),
            Instruction("Store loaded ECX to entry EBP-14 without changing CMP flags",
                StoreRawFileOffset, "89 4D EC"),
            Instruction("Load the fresh DWORD at entry EBP-10 into ECX",
                EbpMinus10ReadRawFileOffset, "8B 4D F0"),
            Instruction("Branch to 8E76 when the earlier unsigned comparison was below",
                BranchRawFileOffset, "0F 82 BD FC FF FF",
                branchTargetRawFileOffset: BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEdxPlus8Address(uint entryEdx) =>
        unchecked(entryEdx + EdxPlus8Displacement);

    public static uint DeriveEbpMinus14Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus14Displacement);

    public static uint DeriveEbpMinus10Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus10Displacement);

    public static CapacityFinalCalleeLargeFrameThresholdPlan Project(
        CapacityFinalCalleeLargeFrameThresholdCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeFrameThresholdRead>();
        var writes = ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeFrameThresholdWrite>();
        var events = ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeFrameThresholdEvent>();
        bool edxCaptured = false;
        bool ebxCaptured = false;
        bool ebpCaptured = false;
        bool fieldReadReached = false;
        uint? fieldAddress = null;
        bool fieldCaptured = false;
        uint? fieldValue = null;
        bool compareReached = false;
        uint? compareLeft = null;
        uint? compareRight = null;
        uint? compareResult = null;
        CapacityFinalCalleeLargeFrameThresholdFlags? compareFlags = null;
        bool storeReached = false;
        uint? storeAddress = null;
        bool storeValueCaptured = false;
        uint? storeValue = null;
        bool frameReadReached = false;
        uint? frameAddress = null;
        bool frameCaptured = false;
        uint? frameValue = null;
        bool branchReached = false;
        bool? branchTaken = null;
        uint? branchDestination = null;

        CapacityFinalCalleeLargeFrameThresholdPlan Finish(
            CapacityFinalCalleeLargeFrameThresholdOutcome outcome,
            uint stopAt,
            bool exactCleanAdmission = false) =>
            new(capture, outcome)
            {
                EntryEDXCaptured = edxCaptured,
                EntryEBXCaptured = ebxCaptured,
                EntryEBPCaptured = ebpCaptured,
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                Events = events.ToImmutable(),
                FreshEdxPlus8ReadReached = fieldReadReached,
                FreshEdxPlus8Address = fieldAddress,
                FreshEdxPlus8Captured = fieldCaptured,
                FreshEdxPlus8 = fieldValue,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareRight,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                StoreReached = storeReached,
                StoreAddress = storeAddress,
                StoreValueCaptured = storeValueCaptured,
                StoreValue = storeValue,
                FreshEbpMinus10ReadReached = frameReadReached,
                FreshEbpMinus10Address = frameAddress,
                FreshEbpMinus10Captured = frameCaptured,
                FreshEbpMinus10 = frameValue,
                BranchReached = branchReached,
                BranchTaken = branchTaken,
                BranchFlags = branchReached ? compareFlags : null,
                BranchSelectedDestinationRawFileOffset = branchDestination,
                UnreachedEntryEBXCaptureProvided =
                    !compareReached && capture.EntryEBX.HasValue,
                UnreachedEntryEBPCaptureProvided =
                    !storeReached && capture.EntryEBP.HasValue,
                UnreachedFreshEdxPlus8CaptureProvided =
                    !fieldReadReached && capture.FreshEdxPlus8.HasValue,
                UnreachedFreshEbpMinus10CaptureProvided =
                    !frameReadReached && capture.FreshEbpMinus10.HasValue,
                ExactCleanAdmission = exactCleanAdmission,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEDX is not uint entryEdx)
        {
            return Finish(
                CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEDX,
                EdxPlus8ReadRawFileOffset);
        }

        edxCaptured = true;
        fieldReadReached = true;
        fieldAddress = DeriveEdxPlus8Address(entryEdx);
        reads.Add(new(
            EdxPlus8ReadRawFileOffset, fieldAddress,
            capture.FreshEdxPlus8.HasValue, capture.FreshEdxPlus8));
        events.Add(new(
            CapacityFinalCalleeLargeFrameThresholdEventKind.Read,
            EdxPlus8ReadRawFileOffset,
            fieldAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshEdxPlus8.HasValue,
            Value: capture.FreshEdxPlus8));
        if (capture.FreshEdxPlus8 is not uint freshEdxPlus8)
        {
            return Finish(
                CapacityFinalCalleeLargeFrameThresholdOutcome.MissingFreshEdxPlus8,
                EdxPlus8ReadRawFileOffset);
        }

        fieldCaptured = true;
        fieldValue = freshEdxPlus8;
        if (capture.EntryEBX is not uint entryEbx)
        {
            return Finish(
                CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEBX,
                CompareRawFileOffset);
        }

        ebxCaptured = true;
        compareReached = true;
        compareLeft = freshEdxPlus8;
        compareRight = entryEbx;
        compareResult = unchecked(freshEdxPlus8 - entryEbx);
        compareFlags = CalculateCompareFlags(
            freshEdxPlus8, entryEbx, compareResult.Value);
        events.Add(new(
            CapacityFinalCalleeLargeFrameThresholdEventKind.Compare,
            CompareRawFileOffset,
            Left: compareLeft,
            Right: compareRight,
            Result: compareResult,
            Flags: compareFlags));

        if (capture.EntryEBP is not uint entryEbp)
        {
            return Finish(
                CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEBP,
                StoreRawFileOffset);
        }

        ebpCaptured = true;
        storeReached = true;
        storeAddress = DeriveEbpMinus14Address(entryEbp);
        storeValueCaptured = true;
        storeValue = freshEdxPlus8;
        var write = new CapacityFinalCalleeLargeFrameThresholdWrite(
            StoreRawFileOffset, storeAddress, true, true, storeValue);
        writes.Add(write);
        events.Add(new(
            CapacityFinalCalleeLargeFrameThresholdEventKind.Write,
            StoreRawFileOffset,
            storeAddress,
            AddressCaptured: true,
            ValueCaptured: true,
            Value: storeValue));

        frameReadReached = true;
        frameAddress = DeriveEbpMinus10Address(entryEbp);
        reads.Add(new(
            EbpMinus10ReadRawFileOffset, frameAddress,
            capture.FreshEbpMinus10.HasValue, capture.FreshEbpMinus10));
        events.Add(new(
            CapacityFinalCalleeLargeFrameThresholdEventKind.Read,
            EbpMinus10ReadRawFileOffset,
            frameAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshEbpMinus10.HasValue,
            Value: capture.FreshEbpMinus10));
        if (capture.FreshEbpMinus10 is not uint freshEbpMinus10)
        {
            return Finish(
                CapacityFinalCalleeLargeFrameThresholdOutcome.MissingFreshEbpMinus10,
                EbpMinus10ReadRawFileOffset);
        }

        frameCaptured = true;
        frameValue = freshEbpMinus10;
        branchReached = true;
        branchTaken = compareFlags!.Value.Carry;
        branchDestination = branchTaken.Value
            ? BranchTargetRawFileOffset
            : FallthroughRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeLargeFrameThresholdEventKind.Branch,
            BranchRawFileOffset,
            Flags: compareFlags,
            BranchTaken: branchTaken,
            SelectedDestinationRawFileOffset: branchDestination));

        return Finish(
            branchTaken.Value
                ? CapacityFinalCalleeLargeFrameThresholdOutcome.UnsignedBelowBranchTarget
                : CapacityFinalCalleeLargeFrameThresholdOutcome.UnsignedNotBelowFallthrough,
            branchDestination.Value,
            exactCleanAdmission: true);
    }

    private static CapacityFinalCalleeLargeFrameThresholdFlags CalculateCompareFlags(
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

    private static CapacityFinalCalleeLargeFrameThresholdInstructionEvidence Instruction(
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

using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeFrameRootGateCapture(
    uint? EntryEDX,
    uint? EntryEBP,
    uint? FreshEdxPlusC,
    uint? FreshEdxPlus18);

public enum CapacityFinalCalleeLargeFrameRootGateOutcome
{
    MissingEntryEDX,
    MissingFreshEdxPlusC,
    MissingFreshEdxPlus18,
    MissingEntryEBPForStore,
    EqualToTarget,
    UnequalToFallthrough
}

public enum CapacityFinalCalleeLargeFrameRootGateEventKind
{
    Read,
    Write,
    Compare,
    Branch
}

public readonly record struct CapacityFinalCalleeLargeFrameRootGateCompareFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeFrameRootGateRead(
    uint RawFileOffset,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeFrameRootGateWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public readonly record struct CapacityFinalCalleeLargeFrameRootGateEvent(
    CapacityFinalCalleeLargeFrameRootGateEventKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    bool AddressCaptured = false,
    bool ValueCaptured = false,
    uint? Value = null,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeLargeFrameRootGateCompareFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public readonly record struct CapacityFinalCalleeLargeFrameRootGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeFrameRootGatePlan(
    CapacityFinalCalleeLargeFrameRootGateCapture Capture,
    CapacityFinalCalleeLargeFrameRootGateOutcome Outcome)
{
    public bool EntryEDXCaptured { get; init; }
    public bool EntryEBPCaptured { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeFrameRootGateRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeFrameRootGateRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeFrameRootGateWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeFrameRootGateWrite>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeFrameRootGateEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeFrameRootGateEvent>.Empty;
    public uint? FreshEdxPlusCAddress { get; init; }
    public bool FreshEdxPlusCCaptured { get; init; }
    public uint? FreshEdxPlusC { get; init; }
    public uint? FreshEdxPlus18Address { get; init; }
    public bool FreshEdxPlus18Captured { get; init; }
    public uint? FreshEdxPlus18 { get; init; }
    public bool StoreReached { get; init; }
    public uint? StoreAddress { get; init; }
    public bool StoreValueCaptured { get; init; }
    public uint? StoreValue { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeLargeFrameRootGateCompareFlags? CompareFlags { get; init; }
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public bool UnreachedEntryEBPCaptureProvided { get; init; }
    public bool UnreachedFreshEdxPlusCCaptureProvided { get; init; }
    public bool UnreachedFreshEdxPlus18CaptureProvided { get; init; }
    public bool ExactCleanAdmission { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the large-frame paired loads, delayed store, and CMP/JZ gate only.
/// The two fresh loads remain independent, including when the store aliases either source.
/// </summary>
public static class CapacityFinalCalleeLargeFrameRootGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C919B;
    public const uint EntryVirtualAddress = 0x016C9D9B;
    public const uint FirstReadRawFileOffset = 0x012C919B;
    public const uint SecondReadRawFileOffset = 0x012C919E;
    public const uint StoreRawFileOffset = 0x012C91A1;
    public const uint CompareRawFileOffset = 0x012C91A4;
    public const uint BranchRawFileOffset = 0x012C91A6;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C91A8;
    public const uint EqualDestinationRawFileOffset = 0x012C91D6;
    public const uint UnequalDestinationRawFileOffset = BoundaryRawFileOffsetExclusive;

    public const uint EdxPlusCDisplacement = 0xC;
    public const uint EdxPlus18Displacement = 0x18;
    public const uint EbpMinus10Displacement = 0x10;

    public const string CapturedWindowBytes =
        "8B 42 0C 8B 4A 18 89 4D F0 3B C2 74 2E";

    public static ImmutableArray<CapacityFinalCalleeLargeFrameRootGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the fresh DWORD at entry EDX+C into EAX",
                FirstReadRawFileOffset, "8B 42 0C"),
            Instruction("Load an independent fresh DWORD at entry EDX+18 into ECX",
                SecondReadRawFileOffset, "8B 4A 18"),
            Instruction("Store the second fresh value to entry EBP-10",
                StoreRawFileOffset, "89 4D F0"),
            Instruction("Compare the first fresh EAX value with entry EDX",
                CompareRawFileOffset, "3B C2"),
            Instruction("Jump to 91D6 when EAX equals entry EDX",
                BranchRawFileOffset, "74 2E",
                branchTargetRawFileOffset: EqualDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEdxPlusCAddress(uint entryEdx) =>
        unchecked(entryEdx + EdxPlusCDisplacement);

    public static uint DeriveEdxPlus18Address(uint entryEdx) =>
        unchecked(entryEdx + EdxPlus18Displacement);

    public static uint DeriveEbpMinus10Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus10Displacement);

    public static CapacityFinalCalleeLargeFrameRootGatePlan Project(
        CapacityFinalCalleeLargeFrameRootGateCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeFrameRootGateRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeFrameRootGateWrite>();
        var events = ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeFrameRootGateEvent>();
        bool edxCaptured = false;
        bool ebpCaptured = false;
        uint? firstAddress = null;
        bool firstCaptured = false;
        uint? firstValue = null;
        uint? secondAddress = null;
        bool secondCaptured = false;
        uint? secondValue = null;
        bool storeReached = false;
        uint? storeAddress = null;
        bool storeValueCaptured = false;
        uint? storeValue = null;
        bool compareReached = false;
        uint? compareLeft = null;
        uint? compareRight = null;
        uint? compareResult = null;
        CapacityFinalCalleeLargeFrameRootGateCompareFlags? compareFlags = null;
        bool branchReached = false;
        bool? branchTaken = null;
        uint? branchDestination = null;
        bool unreachedEbp = false;
        bool unreachedSecond = false;

        CapacityFinalCalleeLargeFrameRootGatePlan Finish(
            CapacityFinalCalleeLargeFrameRootGateOutcome outcome,
            uint stopAt,
            bool exactCleanAdmission = false) =>
            new(capture, outcome)
            {
                EntryEDXCaptured = edxCaptured,
                EntryEBPCaptured = ebpCaptured,
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                Events = events.ToImmutable(),
                FreshEdxPlusCAddress = firstAddress,
                FreshEdxPlusCCaptured = firstCaptured,
                FreshEdxPlusC = firstValue,
                FreshEdxPlus18Address = secondAddress,
                FreshEdxPlus18Captured = secondCaptured,
                FreshEdxPlus18 = secondValue,
                StoreReached = storeReached,
                StoreAddress = storeAddress,
                StoreValueCaptured = storeValueCaptured,
                StoreValue = storeValue,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareRight,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                BranchReached = branchReached,
                BranchTaken = branchTaken,
                BranchSelectedDestinationRawFileOffset = branchDestination,
                UnreachedEntryEBPCaptureProvided = unreachedEbp,
                UnreachedFreshEdxPlusCCaptureProvided =
                    !firstCaptured && capture.FreshEdxPlusC.HasValue,
                UnreachedFreshEdxPlus18CaptureProvided =
                    unreachedSecond || (!secondCaptured && capture.FreshEdxPlus18.HasValue),
                ExactCleanAdmission = exactCleanAdmission,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEDX is not uint entryEdx)
        {
            reads.Add(new(FirstReadRawFileOffset, null, false, null));
            events.Add(new(
                CapacityFinalCalleeLargeFrameRootGateEventKind.Read,
                FirstReadRawFileOffset));
            unreachedEbp = capture.EntryEBP.HasValue;
            unreachedSecond = capture.FreshEdxPlus18.HasValue;
            return Finish(
                CapacityFinalCalleeLargeFrameRootGateOutcome.MissingEntryEDX,
                FirstReadRawFileOffset);
        }

        edxCaptured = true;
        firstAddress = DeriveEdxPlusCAddress(entryEdx);
        uint? firstValueInput = capture.FreshEdxPlusC;
        bool firstValueCaptured = firstValueInput.HasValue;
        reads.Add(new(
            FirstReadRawFileOffset,
            firstAddress,
            firstValueCaptured,
            firstValueInput));
        events.Add(new(
            CapacityFinalCalleeLargeFrameRootGateEventKind.Read,
            FirstReadRawFileOffset,
            firstAddress,
            AddressCaptured: true,
            ValueCaptured: firstValueCaptured,
            Value: firstValueInput));
        if (firstValueInput is not uint freshPlusC)
        {
            unreachedEbp = capture.EntryEBP.HasValue;
            unreachedSecond = capture.FreshEdxPlus18.HasValue;
            return Finish(
                CapacityFinalCalleeLargeFrameRootGateOutcome.MissingFreshEdxPlusC,
                FirstReadRawFileOffset);
        }

        firstCaptured = true;
        firstValue = freshPlusC;
        secondAddress = DeriveEdxPlus18Address(entryEdx);
        uint? secondValueInput = capture.FreshEdxPlus18;
        bool secondValueCaptured = secondValueInput.HasValue;
        reads.Add(new(
            SecondReadRawFileOffset,
            secondAddress,
            secondValueCaptured,
            secondValueInput));
        events.Add(new(
            CapacityFinalCalleeLargeFrameRootGateEventKind.Read,
            SecondReadRawFileOffset,
            secondAddress,
            AddressCaptured: true,
            ValueCaptured: secondValueCaptured,
            Value: secondValueInput));
        if (secondValueInput is not uint freshPlus18)
        {
            unreachedEbp = capture.EntryEBP.HasValue;
            return Finish(
                CapacityFinalCalleeLargeFrameRootGateOutcome.MissingFreshEdxPlus18,
                SecondReadRawFileOffset);
        }

        secondCaptured = true;
        secondValue = freshPlus18;
        if (capture.EntryEBP is not uint entryEbp)
        {
            return Finish(
                CapacityFinalCalleeLargeFrameRootGateOutcome.MissingEntryEBPForStore,
                StoreRawFileOffset);
        }

        ebpCaptured = true;
        storeReached = true;
        storeAddress = DeriveEbpMinus10Address(entryEbp);
        storeValueCaptured = true;
        storeValue = secondValue;
        writes.Add(new(
            StoreRawFileOffset,
            storeAddress,
            AddressCaptured: true,
            ValueCaptured: true,
            Value: storeValue));
        events.Add(new(
            CapacityFinalCalleeLargeFrameRootGateEventKind.Write,
            StoreRawFileOffset,
            storeAddress,
            AddressCaptured: true,
            ValueCaptured: true,
            Value: storeValue));

        compareReached = true;
        compareLeft = firstValue;
        compareRight = entryEdx;
        compareResult = unchecked(firstValue.Value - entryEdx);
        compareFlags = CalculateCompareFlags(
            compareLeft.Value,
            compareRight.Value,
            compareResult.Value);
        events.Add(new(
            CapacityFinalCalleeLargeFrameRootGateEventKind.Compare,
            CompareRawFileOffset,
            Left: compareLeft,
            Right: compareRight,
            Result: compareResult,
            Flags: compareFlags));

        branchReached = true;
        branchTaken = compareFlags.Value.Zero;
        branchDestination = branchTaken.Value
            ? EqualDestinationRawFileOffset
            : UnequalDestinationRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeLargeFrameRootGateEventKind.Branch,
            BranchRawFileOffset,
            BranchTaken: branchTaken,
            SelectedDestinationRawFileOffset: branchDestination));

        return Finish(
            branchTaken.Value
                ? CapacityFinalCalleeLargeFrameRootGateOutcome.EqualToTarget
                : CapacityFinalCalleeLargeFrameRootGateOutcome.UnequalToFallthrough,
            branchDestination.Value,
            exactCleanAdmission: true);
    }

    private static CapacityFinalCalleeLargeFrameRootGateCompareFlags CalculateCompareFlags(
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

    private static CapacityFinalCalleeLargeFrameRootGateInstructionEvidence Instruction(
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

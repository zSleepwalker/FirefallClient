using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonDualPointerThresholdCapture(
    uint? FreshGlobalValue,
    uint? EntryEDX,
    uint? FreshEDXPlus8);

public enum CapacityFinalCalleeCommonDualPointerThresholdOutcome
{
    MissingFreshGlobalValue,
    MissingEntryEDX,
    MissingFreshEDXPlus8,
    FirstUnsignedBelowBranchTarget,
    SecondUnsignedBelowBranchTarget,
    BothUnsignedNotBelowFallthrough
}

public enum CapacityFinalCalleeCommonDualPointerThresholdEventKind
{
    Read,
    Compare,
    Branch
}

public readonly record struct CapacityFinalCalleeCommonDualPointerThresholdFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeCommonDualPointerThresholdInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeCommonDualPointerThresholdRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeCommonDualPointerThresholdEvent(
    CapacityFinalCalleeCommonDualPointerThresholdEventKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    bool AddressCaptured = false,
    bool ValueCaptured = false,
    uint? Value = null,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeCommonDualPointerThresholdFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public sealed record CapacityFinalCalleeCommonDualPointerThresholdPlan(
    CapacityFinalCalleeCommonDualPointerThresholdCapture Capture,
    CapacityFinalCalleeCommonDualPointerThresholdOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeCommonDualPointerThresholdRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonDualPointerThresholdRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonDualPointerThresholdEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonDualPointerThresholdEvent>.Empty;
    public bool FreshGlobalReadReached { get; init; }
    public bool FreshGlobalCaptured { get; init; }
    public uint? FreshGlobal { get; init; }
    public bool EntryEDXCaptured { get; init; }
    public bool FreshEDXPlus8ReadReached { get; init; }
    public uint? FreshEDXPlus8Address { get; init; }
    public bool FreshEDXPlus8Captured { get; init; }
    public uint? FreshEDXPlus8 { get; init; }
    public bool FirstCompareReached { get; init; }
    public uint? FirstCompareLeft { get; init; }
    public uint? FirstCompareRight { get; init; }
    public uint? FirstCompareResult { get; init; }
    public CapacityFinalCalleeCommonDualPointerThresholdFlags? FirstCompareFlags { get; init; }
    public bool FirstJBReached { get; init; }
    public bool? FirstJBTaken { get; init; }
    public uint? FirstJBSelectedDestinationRawFileOffset { get; init; }
    public bool SecondCompareReached { get; init; }
    public uint? SecondCompareLeft { get; init; }
    public uint? SecondCompareRight { get; init; }
    public uint? SecondCompareResult { get; init; }
    public CapacityFinalCalleeCommonDualPointerThresholdFlags? SecondCompareFlags { get; init; }
    public bool SecondJBReached { get; init; }
    public bool? SecondJBTaken { get; init; }
    public uint? SecondJBSelectedDestinationRawFileOffset { get; init; }
    public CapacityFinalCalleeCommonDualPointerThresholdFlags? FlagsAtStop =>
        SecondCompareFlags ?? FirstCompareFlags;
    public bool UnreachedFreshGlobalCaptureProvided =>
        !FreshGlobalReadReached && Capture.FreshGlobalValue.HasValue;
    public bool UnreachedEntryEDXCaptureProvided =>
        !EntryEDXCaptured && Capture.EntryEDX.HasValue;
    public bool UnreachedFreshEDXPlus8CaptureProvided =>
        !FreshEDXPlus8ReadReached && Capture.FreshEDXPlus8.HasValue;
    public bool ExactCleanAdmission { get; init; }
    public bool TargetEntered => false;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the ordered fresh global and EDX+8 reads, followed by two unsigned
/// CMP/JB gates, stopping at the selected edge without entering it.
/// </summary>
public static class CapacityFinalCalleeCommonDualPointerThresholdProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9403;
    public const uint EntryVirtualAddress = 0x016CA003;
    public const uint FreshGlobalReadRawFileOffset = 0x012C9403;
    public const uint FreshEDXPlus8ReadRawFileOffset = 0x012C9409;
    public const uint FirstCompareRawFileOffset = 0x012C940C;
    public const uint FirstJBRawFileOffset = 0x012C940E;
    public const uint SecondCompareRawFileOffset = 0x012C9414;
    public const uint SecondJBRawFileOffset = 0x012C9416;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C941C;
    public const uint BranchTargetRawFileOffset = 0x012C8E76;
    public const uint FallthroughRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const uint EdxPlus8Displacement = 8;
    public const uint FreshGlobalVirtualAddress = 0x01EDADD8;

    public const string CapturedWindowBytes =
        "8B 0D D8 AD ED 01 8B 42 08 3B D1 0F 82 62 FA FF FF 3B C1 0F 82 5A FA FF FF";

    public static ImmutableArray<CapacityFinalCalleeCommonDualPointerThresholdInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Load the fresh DWORD at absolute VA 1EDADD8 into ECX",
                FreshGlobalReadRawFileOffset,
                "8B 0D D8 AD ED 01"),
            Instruction(
                "Load the fresh DWORD at entry EDX+8 into EAX",
                FreshEDXPlus8ReadRawFileOffset,
                "8B 42 08"),
            Instruction(
                "Compare entry EDX with the fresh global value",
                FirstCompareRawFileOffset,
                "3B D1"),
            Instruction(
                "Branch to 8E76 when the first unsigned comparison was below",
                FirstJBRawFileOffset,
                "0F 82 62 FA FF FF",
                branchTargetRawFileOffset: BranchTargetRawFileOffset),
            Instruction(
                "Compare the fresh EDX+8 value with the fresh global value",
                SecondCompareRawFileOffset,
                "3B C1"),
            Instruction(
                "Branch to 8E76 when the second unsigned comparison was below",
                SecondJBRawFileOffset,
                "0F 82 5A FA FF FF",
                branchTargetRawFileOffset: BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEdxPlus8Address(uint entryEdx) =>
        unchecked(entryEdx + EdxPlus8Displacement);

    public static CapacityFinalCalleeCommonDualPointerThresholdPlan Project(
        CapacityFinalCalleeCommonDualPointerThresholdCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonDualPointerThresholdRead>();
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonDualPointerThresholdEvent>();
        bool globalReadReached = false;
        bool globalCaptured = false;
        uint? global = null;
        bool entryEdxCaptured = false;
        bool edxPlus8ReadReached = false;
        uint? edxPlus8Address = null;
        bool edxPlus8Captured = false;
        uint? edxPlus8 = null;
        bool firstCompareReached = false;
        uint? firstCompareResult = null;
        CapacityFinalCalleeCommonDualPointerThresholdFlags? firstCompareFlags = null;
        bool firstJBReached = false;
        bool? firstJBTaken = null;
        uint? firstDestination = null;
        bool secondCompareReached = false;
        uint? secondCompareResult = null;
        CapacityFinalCalleeCommonDualPointerThresholdFlags? secondCompareFlags = null;
        bool secondJBReached = false;
        bool? secondJBTaken = null;
        uint? secondDestination = null;

        CapacityFinalCalleeCommonDualPointerThresholdPlan Finish(
            CapacityFinalCalleeCommonDualPointerThresholdOutcome outcome,
            uint stopAt,
            bool exactCleanAdmission = false) =>
            new(capture, outcome)
            {
                Reads = reads.ToImmutable(),
                Events = events.ToImmutable(),
                FreshGlobalReadReached = globalReadReached,
                FreshGlobalCaptured = globalCaptured,
                FreshGlobal = global,
                EntryEDXCaptured = entryEdxCaptured,
                FreshEDXPlus8ReadReached = edxPlus8ReadReached,
                FreshEDXPlus8Address = edxPlus8Address,
                FreshEDXPlus8Captured = edxPlus8Captured,
                FreshEDXPlus8 = edxPlus8,
                FirstCompareReached = firstCompareReached,
                FirstCompareLeft = firstCompareReached ? capture.EntryEDX : null,
                FirstCompareRight = firstCompareReached ? global : null,
                FirstCompareResult = firstCompareResult,
                FirstCompareFlags = firstCompareFlags,
                FirstJBReached = firstJBReached,
                FirstJBTaken = firstJBTaken,
                FirstJBSelectedDestinationRawFileOffset = firstDestination,
                SecondCompareReached = secondCompareReached,
                SecondCompareLeft = secondCompareReached ? edxPlus8 : null,
                SecondCompareRight = secondCompareReached ? global : null,
                SecondCompareResult = secondCompareResult,
                SecondCompareFlags = secondCompareFlags,
                SecondJBReached = secondJBReached,
                SecondJBTaken = secondJBTaken,
                SecondJBSelectedDestinationRawFileOffset = secondDestination,
                ExactCleanAdmission = exactCleanAdmission,
                StopAtRawFileOffsetExclusive = stopAt
            };

        globalReadReached = true;
        reads.Add(new(
            FreshGlobalReadRawFileOffset,
            FreshGlobalVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshGlobalValue.HasValue,
            Value: capture.FreshGlobalValue));
        events.Add(new(
            CapacityFinalCalleeCommonDualPointerThresholdEventKind.Read,
            FreshGlobalReadRawFileOffset,
            FreshGlobalVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshGlobalValue.HasValue,
            Value: capture.FreshGlobalValue));
        if (capture.FreshGlobalValue is not uint freshGlobal)
        {
            return Finish(
                CapacityFinalCalleeCommonDualPointerThresholdOutcome.MissingFreshGlobalValue,
                FreshGlobalReadRawFileOffset);
        }

        globalCaptured = true;
        global = freshGlobal;

        if (capture.EntryEDX is not uint entryEdx)
        {
            return Finish(
                CapacityFinalCalleeCommonDualPointerThresholdOutcome.MissingEntryEDX,
                FreshEDXPlus8ReadRawFileOffset);
        }

        entryEdxCaptured = true;
        edxPlus8ReadReached = true;
        edxPlus8Address = DeriveEdxPlus8Address(entryEdx);
        reads.Add(new(
            FreshEDXPlus8ReadRawFileOffset,
            edxPlus8Address,
            AddressCaptured: true,
            ValueCaptured: capture.FreshEDXPlus8.HasValue,
            Value: capture.FreshEDXPlus8));
        events.Add(new(
            CapacityFinalCalleeCommonDualPointerThresholdEventKind.Read,
            FreshEDXPlus8ReadRawFileOffset,
            edxPlus8Address,
            AddressCaptured: true,
            ValueCaptured: capture.FreshEDXPlus8.HasValue,
            Value: capture.FreshEDXPlus8));
        if (capture.FreshEDXPlus8 is not uint freshEdxPlus8)
        {
            return Finish(
                CapacityFinalCalleeCommonDualPointerThresholdOutcome.MissingFreshEDXPlus8,
                FreshEDXPlus8ReadRawFileOffset);
        }

        edxPlus8Captured = true;
        edxPlus8 = freshEdxPlus8;

        firstCompareReached = true;
        firstCompareResult = unchecked(entryEdx - freshGlobal);
        firstCompareFlags = CalculateCompareFlags(
            entryEdx, freshGlobal, firstCompareResult.Value);
        events.Add(new(
            CapacityFinalCalleeCommonDualPointerThresholdEventKind.Compare,
            FirstCompareRawFileOffset,
            Left: entryEdx,
            Right: freshGlobal,
            Result: firstCompareResult,
            Flags: firstCompareFlags));

        firstJBReached = true;
        firstJBTaken = firstCompareFlags.Value.Carry;
        firstDestination = firstJBTaken.Value
            ? BranchTargetRawFileOffset
            : SecondCompareRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeCommonDualPointerThresholdEventKind.Branch,
            FirstJBRawFileOffset,
            Flags: firstCompareFlags,
            BranchTaken: firstJBTaken,
            SelectedDestinationRawFileOffset: firstDestination));
        if (firstJBTaken.Value)
        {
            return Finish(
                CapacityFinalCalleeCommonDualPointerThresholdOutcome
                    .FirstUnsignedBelowBranchTarget,
                firstDestination.Value,
                exactCleanAdmission: true);
        }

        secondCompareReached = true;
        secondCompareResult = unchecked(freshEdxPlus8 - freshGlobal);
        secondCompareFlags = CalculateCompareFlags(
            freshEdxPlus8, freshGlobal, secondCompareResult.Value);
        events.Add(new(
            CapacityFinalCalleeCommonDualPointerThresholdEventKind.Compare,
            SecondCompareRawFileOffset,
            Left: freshEdxPlus8,
            Right: freshGlobal,
            Result: secondCompareResult,
            Flags: secondCompareFlags));

        secondJBReached = true;
        secondJBTaken = secondCompareFlags.Value.Carry;
        secondDestination = secondJBTaken.Value
            ? BranchTargetRawFileOffset
            : FallthroughRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeCommonDualPointerThresholdEventKind.Branch,
            SecondJBRawFileOffset,
            Flags: secondCompareFlags,
            BranchTaken: secondJBTaken,
            SelectedDestinationRawFileOffset: secondDestination));

        return Finish(
            secondJBTaken.Value
                ? CapacityFinalCalleeCommonDualPointerThresholdOutcome
                    .SecondUnsignedBelowBranchTarget
                : CapacityFinalCalleeCommonDualPointerThresholdOutcome
                    .BothUnsignedNotBelowFallthrough,
            secondDestination.Value,
            exactCleanAdmission: true);
    }

    private static CapacityFinalCalleeCommonDualPointerThresholdFlags CalculateCompareFlags(
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

    private static CapacityFinalCalleeCommonDualPointerThresholdInstructionEvidence Instruction(
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

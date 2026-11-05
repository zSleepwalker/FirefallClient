using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeNodeFieldSelectionCapture(
    uint? EntryEBP,
    uint? FrameMinus4EDX,
    uint? EntryEAX,
    uint? FreshEAXPlus14,
    uint? FreshEAXPlus10);

public enum CapacityFinalCalleeLargeNodeFieldSelectionOutcome
{
    MissingEntryEBPForFrameRead,
    MissingFrameMinus4EDX,
    MissingEntryEAXForPlus14Read,
    MissingFreshEAXPlus14,
    FirstFieldNonzeroToTarget,
    MissingFreshEAXPlus10,
    SecondFieldZeroToTarget,
    SecondFieldNonzeroToFallthrough
}

public readonly record struct CapacityFinalCalleeLargeNodeFieldSelectionFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeNodeFieldSelectionRead(
    uint RawFileOffset,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeNodeFieldSelectionInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeNodeFieldSelectionPlan(
    CapacityFinalCalleeLargeNodeFieldSelectionCapture Capture,
    CapacityFinalCalleeLargeNodeFieldSelectionOutcome Outcome)
{
    public bool FrameReadReached { get; init; }
    public uint? FrameReadAddress { get; init; }
    public bool EntryEBPCaptured { get; init; }
    public bool FrameMinus4EDXCaptured { get; init; }
    public uint? FrameMinus4EDX { get; init; }
    public bool EntryEAXCaptured { get; init; }
    public bool Plus14CompareReached { get; init; }
    public uint? FreshEAXPlus14Address { get; init; }
    public bool FreshEAXPlus14Captured { get; init; }
    public uint? FreshEAXPlus14 { get; init; }
    public CapacityFinalCalleeLargeNodeFieldSelectionFlags? Plus14CompareFlags { get; init; }
    public bool Plus14LeaReached { get; init; }
    public uint? Plus14LeaECXValue { get; init; }
    public bool Plus14BranchReached { get; init; }
    public bool? Plus14BranchTaken { get; init; }
    public uint? Plus14BranchSelectedDestinationRawFileOffset { get; init; }
    public bool Plus10CompareReached { get; init; }
    public uint? FreshEAXPlus10Address { get; init; }
    public bool FreshEAXPlus10Captured { get; init; }
    public uint? FreshEAXPlus10 { get; init; }
    public CapacityFinalCalleeLargeNodeFieldSelectionFlags? Plus10CompareFlags { get; init; }
    public bool Plus10LeaReached { get; init; }
    public uint? Plus10LeaECXValue { get; init; }
    public bool Plus10BranchReached { get; init; }
    public bool? Plus10BranchTaken { get; init; }
    public uint? Plus10BranchSelectedDestinationRawFileOffset { get; init; }
    public CapacityFinalCalleeLargeNodeFieldSelectionFlags? FlagsAtStop { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeNodeFieldSelectionRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeNodeFieldSelectionRead>.Empty;
    public bool UnreachedFrameMinus4EDXCaptureProvided { get; init; }
    public bool UnreachedEntryEAXCaptureProvided { get; init; }
    public bool UnreachedFreshEAXPlus14CaptureProvided { get; init; }
    public bool UnreachedFreshEAXPlus10CaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the frame EDX reload and ordered plus-14/plus-10 field-selection
/// gates at raw 91F0 through 9205, without entering either selected destination.
/// </summary>
public static class CapacityFinalCalleeLargeNodeFieldSelectionProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C91F0;
    public const uint EntryVirtualAddress = 0x016C9DF0;
    public const uint FrameReadRawFileOffset = 0x012C91F0;
    public const uint Plus14CompareRawFileOffset = 0x012C91F3;
    public const uint Plus14LeaRawFileOffset = 0x012C91F7;
    public const uint Plus14BranchRawFileOffset = 0x012C91FA;
    public const uint Plus10CompareRawFileOffset = 0x012C91FC;
    public const uint Plus10LeaRawFileOffset = 0x012C9200;
    public const uint Plus10BranchRawFileOffset = 0x012C9203;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9205;

    public const uint Plus14NonzeroDestinationRawFileOffset = 0x012C9205;
    public const uint Plus10ZeroDestinationRawFileOffset = 0x012C920B;
    public const uint FrameDisplacement = 4;
    public const uint Plus14Displacement = 0x14;
    public const uint Plus10Displacement = 0x10;

    public const string CapturedWindowBytes =
        "8B 55 FC 83 78 14 00 8D 48 14 75 09 "
        + "83 78 10 00 8D 48 10 74 06";

    public static ImmutableArray<CapacityFinalCalleeLargeNodeFieldSelectionInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the prior fresh-frame EDX from entry EBP-4",
                FrameReadRawFileOffset, "8B 55 FC"),
            Instruction("Compare the fresh DWORD at entry EAX+14 against zero",
                Plus14CompareRawFileOffset, "83 78 14 00"),
            Instruction("Form EAX+14 in ECX without reading the field",
                Plus14LeaRawFileOffset, "8D 48 14"),
            Instruction("Jump to 9205 when the fresh plus-14 value is nonzero",
                Plus14BranchRawFileOffset, "75 09",
                branchTargetRawFileOffset: Plus14NonzeroDestinationRawFileOffset),
            Instruction("Compare the fresh DWORD at entry EAX+10 against zero",
                Plus10CompareRawFileOffset, "83 78 10 00"),
            Instruction("Form EAX+10 in ECX without reading the field",
                Plus10LeaRawFileOffset, "8D 48 10"),
            Instruction("Jump to 920B when the fresh plus-10 value is zero",
                Plus10BranchRawFileOffset, "74 06",
                branchTargetRawFileOffset: Plus10ZeroDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFrameReadAddress(uint entryEBP) =>
        unchecked(entryEBP - FrameDisplacement);

    public static uint DeriveFreshEAXPlus14Address(uint entryEAX) =>
        unchecked(entryEAX + Plus14Displacement);

    public static uint DeriveFreshEAXPlus10Address(uint entryEAX) =>
        unchecked(entryEAX + Plus10Displacement);

    public static CapacityFinalCalleeLargeNodeFieldSelectionPlan Project(
        CapacityFinalCalleeLargeNodeFieldSelectionCapture capture)
    {
        var reached = ImmutableArray.CreateBuilder<uint>();
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeNodeFieldSelectionRead>();
        bool frameReadReached = false;
        uint? frameReadAddress = null;
        bool entryEBPCaptured = false;
        bool frameEDXCaptured = false;
        uint? frameEDX = null;
        bool entryEAXCaptured = false;
        bool plus14CompareReached = false;
        uint? plus14Address = null;
        bool plus14Captured = false;
        uint? plus14Value = null;
        CapacityFinalCalleeLargeNodeFieldSelectionFlags? plus14Flags = null;
        bool plus14LeaReached = false;
        uint? plus14LeaValue = null;
        bool plus14BranchReached = false;
        bool? plus14BranchTaken = null;
        uint? plus14BranchDestination = null;
        bool plus10CompareReached = false;
        uint? plus10Address = null;
        bool plus10Captured = false;
        uint? plus10Value = null;
        CapacityFinalCalleeLargeNodeFieldSelectionFlags? plus10Flags = null;
        bool plus10LeaReached = false;
        uint? plus10LeaValue = null;
        bool plus10BranchReached = false;
        bool? plus10BranchTaken = null;
        uint? plus10BranchDestination = null;

        CapacityFinalCalleeLargeNodeFieldSelectionPlan Finish(
            CapacityFinalCalleeLargeNodeFieldSelectionOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                FrameReadReached = frameReadReached,
                FrameReadAddress = frameReadAddress,
                EntryEBPCaptured = entryEBPCaptured,
                FrameMinus4EDXCaptured = frameEDXCaptured,
                FrameMinus4EDX = frameEDX,
                EntryEAXCaptured = entryEAXCaptured,
                Plus14CompareReached = plus14CompareReached,
                FreshEAXPlus14Address = plus14Address,
                FreshEAXPlus14Captured = plus14Captured,
                FreshEAXPlus14 = plus14Value,
                Plus14CompareFlags = plus14Flags,
                Plus14LeaReached = plus14LeaReached,
                Plus14LeaECXValue = plus14LeaValue,
                Plus14BranchReached = plus14BranchReached,
                Plus14BranchTaken = plus14BranchTaken,
                Plus14BranchSelectedDestinationRawFileOffset = plus14BranchDestination,
                Plus10CompareReached = plus10CompareReached,
                FreshEAXPlus10Address = plus10Address,
                FreshEAXPlus10Captured = plus10Captured,
                FreshEAXPlus10 = plus10Value,
                Plus10CompareFlags = plus10Flags,
                Plus10LeaReached = plus10LeaReached,
                Plus10LeaECXValue = plus10LeaValue,
                Plus10BranchReached = plus10BranchReached,
                Plus10BranchTaken = plus10BranchTaken,
                Plus10BranchSelectedDestinationRawFileOffset = plus10BranchDestination,
                FlagsAtStop = plus10Flags ?? plus14Flags,
                ReachedInstructions = reached.ToImmutable(),
                Reads = reads.ToImmutable(),
                UnreachedFrameMinus4EDXCaptureProvided =
                    !frameEDXCaptured && capture.FrameMinus4EDX.HasValue,
                UnreachedEntryEAXCaptureProvided =
                    !entryEAXCaptured && capture.EntryEAX.HasValue,
                UnreachedFreshEAXPlus14CaptureProvided =
                    !plus14Captured && capture.FreshEAXPlus14.HasValue,
                UnreachedFreshEAXPlus10CaptureProvided =
                    !plus10Captured && capture.FreshEAXPlus10.HasValue,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        frameReadReached = true;
        reached.Add(FrameReadRawFileOffset);
        if (capture.EntryEBP is not uint entryEBP)
        {
            reads.Add(new(
                FrameReadRawFileOffset,
                Address: null,
                ValueCaptured: false,
                Value: null));
            return Finish(
                CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingEntryEBPForFrameRead,
                FrameReadRawFileOffset);
        }

        entryEBPCaptured = true;
        frameReadAddress = DeriveFrameReadAddress(entryEBP);
        reads.Add(new(
            FrameReadRawFileOffset,
            frameReadAddress,
            capture.FrameMinus4EDX.HasValue,
            capture.FrameMinus4EDX));
        if (capture.FrameMinus4EDX is not uint capturedFrameEDX)
        {
            return Finish(
                CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingFrameMinus4EDX,
                FrameReadRawFileOffset);
        }

        frameEDXCaptured = true;
        frameEDX = capturedFrameEDX;

        plus14CompareReached = true;
        reached.Add(Plus14CompareRawFileOffset);
        if (capture.EntryEAX is not uint entryEAX)
        {
            reads.Add(new(
                Plus14CompareRawFileOffset,
                Address: null,
                ValueCaptured: false,
                Value: null));
            return Finish(
                CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingEntryEAXForPlus14Read,
                Plus14CompareRawFileOffset);
        }

        entryEAXCaptured = true;
        plus14Address = DeriveFreshEAXPlus14Address(entryEAX);
        reads.Add(new(
            Plus14CompareRawFileOffset,
            plus14Address,
            capture.FreshEAXPlus14.HasValue,
            capture.FreshEAXPlus14));
        if (capture.FreshEAXPlus14 is not uint capturedPlus14)
        {
            return Finish(
                CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingFreshEAXPlus14,
                Plus14CompareRawFileOffset);
        }

        plus14Captured = true;
        plus14Value = capturedPlus14;
        plus14Flags = CalculateCompareAgainstZeroFlags(capturedPlus14);

        plus14LeaReached = true;
        reached.Add(Plus14LeaRawFileOffset);
        plus14LeaValue = DeriveFreshEAXPlus14Address(entryEAX);

        plus14BranchReached = true;
        reached.Add(Plus14BranchRawFileOffset);
        plus14BranchTaken = capturedPlus14 != 0;
        plus14BranchDestination = plus14BranchTaken.Value
            ? Plus14NonzeroDestinationRawFileOffset
            : Plus10CompareRawFileOffset;
        if (plus14BranchTaken.Value)
        {
            return Finish(
                CapacityFinalCalleeLargeNodeFieldSelectionOutcome.FirstFieldNonzeroToTarget,
                Plus14NonzeroDestinationRawFileOffset,
                exactCleanExitAdmitted: !capture.FreshEAXPlus10.HasValue);
        }

        plus10CompareReached = true;
        reached.Add(Plus10CompareRawFileOffset);
        plus10Address = DeriveFreshEAXPlus10Address(entryEAX);
        reads.Add(new(
            Plus10CompareRawFileOffset,
            plus10Address,
            capture.FreshEAXPlus10.HasValue,
            capture.FreshEAXPlus10));
        if (capture.FreshEAXPlus10 is not uint capturedPlus10)
        {
            return Finish(
                CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingFreshEAXPlus10,
                Plus10CompareRawFileOffset);
        }

        plus10Captured = true;
        plus10Value = capturedPlus10;
        plus10Flags = CalculateCompareAgainstZeroFlags(capturedPlus10);

        plus10LeaReached = true;
        reached.Add(Plus10LeaRawFileOffset);
        plus10LeaValue = DeriveFreshEAXPlus10Address(entryEAX);

        plus10BranchReached = true;
        reached.Add(Plus10BranchRawFileOffset);
        plus10BranchTaken = capturedPlus10 == 0;
        plus10BranchDestination = plus10BranchTaken.Value
            ? Plus10ZeroDestinationRawFileOffset
            : BoundaryRawFileOffsetExclusive;
        return Finish(
            plus10BranchTaken.Value
                ? CapacityFinalCalleeLargeNodeFieldSelectionOutcome.SecondFieldZeroToTarget
                : CapacityFinalCalleeLargeNodeFieldSelectionOutcome.SecondFieldNonzeroToFallthrough,
            plus10BranchDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeLargeNodeFieldSelectionFlags CalculateCompareAgainstZeroFlags(
        uint value) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)value),
            AuxiliaryCarry: false,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeLargeNodeFieldSelectionInstructionEvidence Instruction(
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

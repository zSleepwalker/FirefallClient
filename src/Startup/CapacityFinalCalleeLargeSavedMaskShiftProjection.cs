using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSavedMaskShiftCapture(
    uint? EntryEDX,
    uint? FreshEDXPlus1C,
    uint? EntryEBP,
    uint? FreshEBPMinus8);

public enum CapacityFinalCalleeLargeSavedMaskShiftOutcome
{
    MissingEntryEDX,
    MissingFreshEDXPlus1C,
    MissingEntryEBP,
    MissingFreshEBPMinus8,
    BoundedBeforeGlobalAnd
}

public enum CapacityFinalCalleeLargeSavedMaskShiftReadStage
{
    FreshEDXPlus1C,
    FreshEBPMinus8
}

public readonly record struct CapacityFinalCalleeLargeSavedMaskShiftRead(
    CapacityFinalCalleeLargeSavedMaskShiftReadStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool ReadReached,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeSavedMaskShiftFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeLargeSavedMaskShiftInstructionEvidence(
    uint RawFileOffset,
    uint FlatAddress,
    uint PEVirtualAddress,
    ImmutableArray<byte> Bytes);

public sealed record CapacityFinalCalleeLargeSavedMaskShiftPlan(
    CapacityFinalCalleeLargeSavedMaskShiftCapture Capture,
    CapacityFinalCalleeLargeSavedMaskShiftOutcome Outcome)
{
    public bool FreshEDXPlus1CReadReached { get; init; }
    public uint? FreshEDXPlus1CAddress { get; init; }
    public bool FreshEDXPlus1CCaptured { get; init; }
    public bool EntryEBPConsumerReached { get; init; }
    public bool FreshEBPMinus8ReadReached { get; init; }
    public uint? FreshEBPMinus8Address { get; init; }
    public bool FreshEBPMinus8Captured { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeSavedMaskShiftRead> OrderedReads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeSavedMaskShiftRead>.Empty;
    public bool UnreachedFreshEDXPlus1CCaptureProvided { get; init; }
    public bool UnreachedEntryEBPCaptureProvided { get; init; }
    public bool UnreachedFreshEBPMinus8CaptureProvided { get; init; }
    public uint? ShiftCountSource { get; init; }
    public byte? ShiftCountLowByte { get; init; }
    public byte? ShiftCountMasked { get; init; }
    public uint? ShiftInputEAX { get; init; }
    public uint? ShiftResult { get; init; }
    public CapacityFinalCalleeLargeSavedMaskShiftFlags? ShiftFlags { get; init; }
    public bool NotReached { get; init; }
    public uint? NotResult { get; init; }
    public CapacityFinalCalleeLargeSavedMaskShiftFlags? FlagsAtStop { get; init; }
    public bool GlobalAndReached => false;
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Reconstructs the ordered fresh reads and shift/NOT through the bounded
/// 0x925B stop; the following global AND is outside this projection.
/// </summary>
public static class CapacityFinalCalleeLargeSavedMaskShiftProjection
{
    public const uint EntryRawFileOffset = 0x012C9251;
    public const uint EBPMinus8ReadRawFileOffset = 0x012C9254;
    public const uint ShiftRawFileOffset = 0x012C9257;
    public const uint NotRawFileOffset = 0x012C9259;
    public const uint BoundedStopRawFileOffsetExclusive = 0x012C925B;

    public static ImmutableArray<CapacityFinalCalleeLargeSavedMaskShiftInstructionEvidence>
        ProductionInstructionEvidence { get; } =
            ImmutableArray.Create(
                new CapacityFinalCalleeLargeSavedMaskShiftInstructionEvidence(
                    0x012C9251,
                    0x012C9251,
                    0x016C9E51,
                    ImmutableArray.Create<byte>(0x8B, 0x4A, 0x1C)),
                new CapacityFinalCalleeLargeSavedMaskShiftInstructionEvidence(
                    0x012C9254,
                    0x012C9254,
                    0x016C9E54,
                    ImmutableArray.Create<byte>(0x8B, 0x45, 0xF8)),
                new CapacityFinalCalleeLargeSavedMaskShiftInstructionEvidence(
                    0x012C9257,
                    0x012C9257,
                    0x016C9E57,
                    ImmutableArray.Create<byte>(0xD3, 0xE0)),
                new CapacityFinalCalleeLargeSavedMaskShiftInstructionEvidence(
                    0x012C9259,
                    0x012C9259,
                    0x016C9E59,
                    ImmutableArray.Create<byte>(0xF7, 0xD0)));

    public static bool TryGetInstructionEvidence(
        uint rawFileOffset,
        out CapacityFinalCalleeLargeSavedMaskShiftInstructionEvidence evidence)
    {
        foreach (CapacityFinalCalleeLargeSavedMaskShiftInstructionEvidence instruction
            in ProductionInstructionEvidence)
        {
            if (instruction.RawFileOffset == rawFileOffset)
            {
                evidence = instruction;
                return true;
            }
        }

        evidence = default;
        return false;
    }

    public static CapacityFinalCalleeLargeSavedMaskShiftPlan Project(
        CapacityFinalCalleeLargeSavedMaskShiftCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeSavedMaskShiftRead>(2);

        if (!capture.EntryEDX.HasValue)
        {
            return new(capture, CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingEntryEDX)
            {
                UnreachedFreshEDXPlus1CCaptureProvided = capture.FreshEDXPlus1C.HasValue,
                UnreachedEntryEBPCaptureProvided = capture.EntryEBP.HasValue,
                UnreachedFreshEBPMinus8CaptureProvided = capture.FreshEBPMinus8.HasValue,
                StopAtRawFileOffsetExclusive = EntryRawFileOffset
            };
        }

        uint edxPlus1CAddress = unchecked(capture.EntryEDX.Value + 0x1Cu);
        bool firstValueCaptured = capture.FreshEDXPlus1C.HasValue;
        reads.Add(new(
            CapacityFinalCalleeLargeSavedMaskShiftReadStage.FreshEDXPlus1C,
            EntryRawFileOffset,
            edxPlus1CAddress,
            ReadReached: true,
            ValueCaptured: firstValueCaptured,
            Value: capture.FreshEDXPlus1C));

        if (!firstValueCaptured)
        {
            return new(capture, CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingFreshEDXPlus1C)
            {
                FreshEDXPlus1CReadReached = true,
                FreshEDXPlus1CAddress = edxPlus1CAddress,
                UnreachedEntryEBPCaptureProvided = capture.EntryEBP.HasValue,
                UnreachedFreshEBPMinus8CaptureProvided = capture.FreshEBPMinus8.HasValue,
                OrderedReads = reads.ToImmutable(),
                StopAtRawFileOffsetExclusive = EntryRawFileOffset
            };
        }

        uint countSource = capture.FreshEDXPlus1C.Value;
        byte countLowByte = (byte)countSource;
        byte countMasked = (byte)(countLowByte & 31);

        if (!capture.EntryEBP.HasValue)
        {
            return new(capture, CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingEntryEBP)
            {
                FreshEDXPlus1CReadReached = true,
                FreshEDXPlus1CAddress = edxPlus1CAddress,
                FreshEDXPlus1CCaptured = true,
                EntryEBPConsumerReached = true,
                UnreachedFreshEBPMinus8CaptureProvided = capture.FreshEBPMinus8.HasValue,
                OrderedReads = reads.ToImmutable(),
                ShiftCountSource = countSource,
                ShiftCountLowByte = countLowByte,
                ShiftCountMasked = countMasked,
                StopAtRawFileOffsetExclusive = EBPMinus8ReadRawFileOffset
            };
        }

        uint ebpMinus8Address = unchecked(capture.EntryEBP.Value - 8u);
        bool secondValueCaptured = capture.FreshEBPMinus8.HasValue;
        reads.Add(new(
            CapacityFinalCalleeLargeSavedMaskShiftReadStage.FreshEBPMinus8,
            EBPMinus8ReadRawFileOffset,
            ebpMinus8Address,
            ReadReached: true,
            ValueCaptured: secondValueCaptured,
            Value: capture.FreshEBPMinus8));

        if (!secondValueCaptured)
        {
            return new(capture, CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingFreshEBPMinus8)
            {
                FreshEDXPlus1CReadReached = true,
                FreshEDXPlus1CAddress = edxPlus1CAddress,
                FreshEDXPlus1CCaptured = true,
                EntryEBPConsumerReached = true,
                FreshEBPMinus8ReadReached = true,
                FreshEBPMinus8Address = ebpMinus8Address,
                OrderedReads = reads.ToImmutable(),
                ShiftCountSource = countSource,
                ShiftCountLowByte = countLowByte,
                ShiftCountMasked = countMasked,
                StopAtRawFileOffsetExclusive = EBPMinus8ReadRawFileOffset
            };
        }

        int count = countMasked;
        uint shiftInput = capture.FreshEBPMinus8.Value;
        uint shifted = shiftInput << count;
        CapacityFinalCalleeLargeSavedMaskShiftFlags shiftFlags =
            GetShiftFlags(shiftInput, shifted, count);
        uint notResult = ~shifted;

        return new(capture, CapacityFinalCalleeLargeSavedMaskShiftOutcome.BoundedBeforeGlobalAnd)
        {
            FreshEDXPlus1CReadReached = true,
            FreshEDXPlus1CAddress = edxPlus1CAddress,
            FreshEDXPlus1CCaptured = true,
            EntryEBPConsumerReached = true,
            FreshEBPMinus8ReadReached = true,
            FreshEBPMinus8Address = ebpMinus8Address,
            FreshEBPMinus8Captured = true,
            OrderedReads = reads.ToImmutable(),
            ShiftCountSource = countSource,
            ShiftCountLowByte = countLowByte,
            ShiftCountMasked = (byte)count,
            ShiftInputEAX = shiftInput,
            ShiftResult = shifted,
            ShiftFlags = shiftFlags,
            NotReached = true,
            NotResult = notResult,
            FlagsAtStop = shiftFlags,
            ExactCleanExitAdmitted = true,
            StopAtRawFileOffsetExclusive = BoundedStopRawFileOffsetExclusive
        };
    }

    private static CapacityFinalCalleeLargeSavedMaskShiftFlags GetShiftFlags(
        uint input,
        uint result,
        int count)
    {
        if (count == 0)
        {
            return new(null, null, null, null, null, null);
        }

        bool carry = ((input >> (32 - count)) & 1u) != 0;
        bool? overflow = count == 1
            ? ((result & 0x80000000u) != 0) ^ carry
            : null;

        return new(
            carry,
            IsEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000u) != 0,
            Overflow: overflow);
    }

    private static bool IsEvenParity(byte value)
    {
        int setBits = 0;
        for (int bit = 0; bit < 8; bit++)
        {
            setBits += (value >> bit) & 1;
        }

        return (setBits & 1) == 0;
    }
}

using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonFreshMaskGateCapture(
    uint? FreshGlobalValue,
    uint? EntryEDX,
    uint? EntryECX);

public enum CapacityFinalCalleeCommonFreshMaskGateOutcome
{
    MissingFreshGlobalValue,
    MissingEntryEDX,
    MissingEntryECX,
    MissingEntryEDXAndEntryECX,
    ZeroTestResultToBounded93AC,
    NonzeroTestResultToBounded93C0
}

public enum CapacityFinalCalleeCommonFreshMaskGateInstructionStage
{
    FreshGlobalMove,
    ShiftEDXByCL,
    TestEAXAndEDX,
    Jnz
}

public readonly record struct CapacityFinalCalleeCommonFreshMaskGateFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonFreshMaskGateReadAttempt(
    uint RawFileOffset,
    uint AbsoluteAddress,
    bool ReadReached,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeCommonFreshMaskGateEvent(
    CapacityFinalCalleeCommonFreshMaskGateInstructionStage Stage,
    uint RawFileOffset,
    bool Reached,
    bool Completed,
    uint? EaxValue,
    uint? EdxValue,
    byte? ShiftCount,
    uint? Result,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset);

public readonly record struct CapacityFinalCalleeCommonFreshMaskGateInstructionEvidence(
    string Mnemonic,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint PEVirtualAddress,
    ImmutableArray<byte> Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetPEVirtualAddress = null);

public readonly record struct CapacityFinalCalleeCommonFreshMaskGateDestinationEvidence(
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint PEVirtualAddress,
    bool BeforeTargetInstruction);

public sealed record CapacityFinalCalleeCommonFreshMaskGatePlan(
    CapacityFinalCalleeCommonFreshMaskGateCapture Capture,
    CapacityFinalCalleeCommonFreshMaskGateOutcome Outcome)
{
    public bool FreshGlobalReadReached { get; init; }
    public bool FreshGlobalValueCaptured { get; init; }
    public uint? FreshGlobalReadAddress =>
        FreshGlobalReadReached
            ? CapacityFinalCalleeCommonFreshMaskGateProjection.FreshGlobalVirtualAddress
            : null;
    public ImmutableArray<CapacityFinalCalleeCommonFreshMaskGateReadAttempt> ReadAttempts { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonFreshMaskGateReadAttempt>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonFreshMaskGateEvent> OrderedEvents { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonFreshMaskGateEvent>.Empty;
    public bool UnreachedEntryEDXCaptureProvided { get; init; }
    public bool UnreachedEntryECXCaptureProvided { get; init; }
    public bool EntryEDXOperandConsumed { get; init; }
    public bool EntryECXOperandConsumed { get; init; }
    public bool EntryEDXCaptureProvidedButUnused =>
        !EntryEDXOperandConsumed && Capture.EntryEDX.HasValue;
    public bool EntryECXCaptureProvidedButUnused =>
        !EntryECXOperandConsumed && Capture.EntryECX.HasValue;
    public bool ShiftInstructionReached { get; init; }
    public bool ShiftExecuted { get; init; }
    public uint? ShiftCountSource { get; init; }
    public byte? ShiftCountLowByte { get; init; }
    public byte? ShiftCountMasked { get; init; }
    public uint? ShiftInputEDX { get; init; }
    public uint? ShiftResult { get; init; }
    public CapacityFinalCalleeCommonFreshMaskGateFlags? ShiftFlags { get; init; }
    public bool TestReached { get; init; }
    public uint? TestResult { get; init; }
    public CapacityFinalCalleeCommonFreshMaskGateFlags? TestFlags { get; init; }
    public CapacityFinalCalleeCommonFreshMaskGateFlags? FlagsAtStop => TestFlags ?? ShiftFlags;
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? JnzSelectedDestinationRawFileOffset { get; init; }
    public bool ExactCleanBoundedDestination { get; init; }
    public bool MemoryWritePerformed => false;
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the fresh-global MOV, EDX-by-CL shift, TEST, and JNZ through
/// either bounded branch destination without executing either target.
/// </summary>
public static class CapacityFinalCalleeCommonFreshMaskGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C93A1;
    public const uint EntryVirtualAddress = 0x016C9FA1;
    public const uint FreshGlobalMoveRawFileOffset = 0x012C93A1;
    public const uint ShiftRawFileOffset = 0x012C93A6;
    public const uint TestRawFileOffset = 0x012C93A8;
    public const uint JnzRawFileOffset = 0x012C93AA;
    public const uint ZeroDestinationRawFileOffset = 0x012C93AC;
    public const uint NonzeroDestinationRawFileOffset = 0x012C93C0;
    public const uint FreshGlobalVirtualAddress = 0x01EDADCC;
    public const string CapturedWindowBytes = "A1 CC AD ED 01 D3 E2 85 D0 75 14";

    public static ImmutableArray<CapacityFinalCalleeCommonFreshMaskGateInstructionEvidence>
        ProductionInstructionEvidence { get; } =
        ImmutableArray.Create(
            Instruction(
                "MOV EAX, [01EDADCC]",
                FreshGlobalMoveRawFileOffset,
                ImmutableArray.Create<byte>(0xA1, 0xCC, 0xAD, 0xED, 0x01)),
            Instruction(
                "SHL EDX, CL",
                ShiftRawFileOffset,
                ImmutableArray.Create<byte>(0xD3, 0xE2)),
            Instruction(
                "TEST EAX, EDX",
                TestRawFileOffset,
                ImmutableArray.Create<byte>(0x85, 0xD0)),
            Instruction(
                "JNZ 012C93C0",
                JnzRawFileOffset,
                ImmutableArray.Create<byte>(0x75, 0x14),
                NonzeroDestinationRawFileOffset));

    public static ImmutableArray<CapacityFinalCalleeCommonFreshMaskGateDestinationEvidence>
        BoundedDestinations { get; } =
        ImmutableArray.Create(
            Destination(ZeroDestinationRawFileOffset),
            Destination(NonzeroDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static bool TryGetInstructionEvidence(
        uint rawFileOffset,
        out CapacityFinalCalleeCommonFreshMaskGateInstructionEvidence evidence)
    {
        foreach (CapacityFinalCalleeCommonFreshMaskGateInstructionEvidence instruction
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

    public static CapacityFinalCalleeCommonFreshMaskGatePlan Project(
        CapacityFinalCalleeCommonFreshMaskGateCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonFreshMaskGateReadAttempt>(1);
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonFreshMaskGateEvent>(4);
        bool globalCaptured = capture.FreshGlobalValue.HasValue;
        reads.Add(new(
            FreshGlobalMoveRawFileOffset,
            FreshGlobalVirtualAddress,
            ReadReached: true,
            ValueCaptured: globalCaptured,
            Value: capture.FreshGlobalValue));
        events.Add(new(
            CapacityFinalCalleeCommonFreshMaskGateInstructionStage.FreshGlobalMove,
            FreshGlobalMoveRawFileOffset,
            Reached: true,
            Completed: globalCaptured,
            EaxValue: capture.FreshGlobalValue,
            EdxValue: null,
            ShiftCount: null,
            Result: null,
            BranchTaken: null,
            SelectedDestinationRawFileOffset: null));

        if (!globalCaptured)
        {
            return new(
                capture,
                CapacityFinalCalleeCommonFreshMaskGateOutcome.MissingFreshGlobalValue)
            {
                FreshGlobalReadReached = true,
                ReadAttempts = reads.ToImmutable(),
                OrderedEvents = events.ToImmutable(),
                UnreachedEntryEDXCaptureProvided = capture.EntryEDX.HasValue,
                UnreachedEntryECXCaptureProvided = capture.EntryECX.HasValue,
                StopAtRawFileOffsetExclusive = FreshGlobalMoveRawFileOffset
            };
        }

        bool hasEDX = capture.EntryEDX.HasValue;
        bool hasECX = capture.EntryECX.HasValue;
        if (!hasEDX || !hasECX)
        {
            events.Add(new(
                CapacityFinalCalleeCommonFreshMaskGateInstructionStage.ShiftEDXByCL,
                ShiftRawFileOffset,
                Reached: true,
                Completed: false,
                EaxValue: capture.FreshGlobalValue,
                EdxValue: capture.EntryEDX,
                ShiftCount: capture.EntryECX is uint knownCount
                    ? (byte)(knownCount & 31) : null,
                Result: null,
                BranchTaken: null,
                SelectedDestinationRawFileOffset: null));

            CapacityFinalCalleeCommonFreshMaskGateOutcome missingOutcome =
                !hasEDX && !hasECX
                    ? CapacityFinalCalleeCommonFreshMaskGateOutcome
                        .MissingEntryEDXAndEntryECX
                    : !hasEDX
                        ? CapacityFinalCalleeCommonFreshMaskGateOutcome.MissingEntryEDX
                        : CapacityFinalCalleeCommonFreshMaskGateOutcome.MissingEntryECX;

            return new(capture, missingOutcome)
            {
                FreshGlobalReadReached = true,
                FreshGlobalValueCaptured = true,
                ReadAttempts = reads.ToImmutable(),
                OrderedEvents = events.ToImmutable(),
                ShiftInstructionReached = true,
                ShiftInputEDX = capture.EntryEDX,
                ShiftCountSource = capture.EntryECX,
                ShiftCountLowByte = capture.EntryECX is uint countSource
                    ? (byte)countSource : null,
                ShiftCountMasked = capture.EntryECX is uint count
                    ? (byte)(count & 31) : null,
                StopAtRawFileOffsetExclusive = ShiftRawFileOffset
            };
        }

        uint shiftCountSource = capture.EntryECX!.Value;
        byte shiftCountLowByte = (byte)shiftCountSource;
        byte shiftCount = (byte)(shiftCountLowByte & 31);
        uint shiftInput = capture.EntryEDX!.Value;
        uint shifted = shiftInput << shiftCount;
        CapacityFinalCalleeCommonFreshMaskGateFlags shiftFlags =
            CalculateShiftFlags(shiftInput, shifted, shiftCount);
        events.Add(new(
            CapacityFinalCalleeCommonFreshMaskGateInstructionStage.ShiftEDXByCL,
            ShiftRawFileOffset,
            Reached: true,
            Completed: true,
            EaxValue: capture.FreshGlobalValue,
            EdxValue: shiftInput,
            ShiftCount: shiftCount,
            Result: shifted,
            BranchTaken: null,
            SelectedDestinationRawFileOffset: null));

        uint testResult = capture.FreshGlobalValue!.Value & shifted;
        CapacityFinalCalleeCommonFreshMaskGateFlags testFlags =
            CalculateTestFlags(testResult);
        events.Add(new(
            CapacityFinalCalleeCommonFreshMaskGateInstructionStage.TestEAXAndEDX,
            TestRawFileOffset,
            Reached: true,
            Completed: true,
            EaxValue: capture.FreshGlobalValue,
            EdxValue: shifted,
            ShiftCount: null,
            Result: testResult,
            BranchTaken: null,
            SelectedDestinationRawFileOffset: null));

        bool jnzTaken = testResult != 0;
        uint destination = jnzTaken
            ? NonzeroDestinationRawFileOffset
            : ZeroDestinationRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeCommonFreshMaskGateInstructionStage.Jnz,
            JnzRawFileOffset,
            Reached: true,
            Completed: true,
            EaxValue: capture.FreshGlobalValue,
            EdxValue: shifted,
            ShiftCount: null,
            Result: null,
            BranchTaken: jnzTaken,
            SelectedDestinationRawFileOffset: destination));

        return new(
            capture,
            jnzTaken
                ? CapacityFinalCalleeCommonFreshMaskGateOutcome
                    .NonzeroTestResultToBounded93C0
                : CapacityFinalCalleeCommonFreshMaskGateOutcome
                    .ZeroTestResultToBounded93AC)
        {
            FreshGlobalReadReached = true,
            FreshGlobalValueCaptured = true,
            ReadAttempts = reads.ToImmutable(),
            OrderedEvents = events.ToImmutable(),
            EntryEDXOperandConsumed = true,
            EntryECXOperandConsumed = true,
            ShiftInstructionReached = true,
            ShiftExecuted = true,
            ShiftCountSource = shiftCountSource,
            ShiftCountLowByte = shiftCountLowByte,
            ShiftCountMasked = shiftCount,
            ShiftInputEDX = shiftInput,
            ShiftResult = shifted,
            ShiftFlags = shiftFlags,
            TestReached = true,
            TestResult = testResult,
            TestFlags = testFlags,
            JnzReached = true,
            JnzTaken = jnzTaken,
            JnzSelectedDestinationRawFileOffset = destination,
            ExactCleanBoundedDestination = true,
            ExactCleanExitAdmitted = true,
            StopAtRawFileOffsetExclusive = destination
        };
    }

    private static CapacityFinalCalleeCommonFreshMaskGateFlags CalculateShiftFlags(
        uint input,
        uint result,
        int count)
    {
        if (count == 0)
        {
            return UnknownFlags;
        }

        bool carry = ((input >> (32 - count)) & 1u) != 0;
        bool? overflow = count == 1
            ? ((result & 0x80000000u) != 0) ^ carry
            : null;

        return new(
            carry,
            HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000u) != 0,
            Overflow: overflow);
    }

    private static CapacityFinalCalleeCommonFreshMaskGateFlags CalculateTestFlags(
        uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000u) != 0,
            Overflow: false);

    private static CapacityFinalCalleeCommonFreshMaskGateFlags UnknownFlags =>
        new(null, null, null, null, null, null);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeCommonFreshMaskGateInstructionEvidence Instruction(
        string mnemonic,
        uint rawFileOffset,
        ImmutableArray<byte> bytes,
        uint? branchTargetRawFileOffset = null) =>
        new(
            mnemonic,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);

    private static CapacityFinalCalleeCommonFreshMaskGateDestinationEvidence Destination(
        uint rawFileOffset) =>
        new(
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            BeforeTargetInstruction: true);
}

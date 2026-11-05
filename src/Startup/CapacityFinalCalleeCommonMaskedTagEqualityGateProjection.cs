using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonMaskedTagEqualityGateCapture(
    uint? EntryEAX,
    uint? EntryEDI,
    uint? EntryEDX,
    uint? FreshEDXPlus4);

public enum CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome
{
    MissingEntryEAX,
    MissingEntryEDI,
    MissingEntryEDX,
    MissingFreshEDXPlus4,
    EqualToBounded9403,
    UnequalToFallThrough93E4
}

public enum CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage
{
    MoveECXFromEAX,
    MoveEBXFromEDI,
    FreshEDXPlus4MoveToEAX,
    MaskEAXLowTagBits,
    ShiftEBXByCL,
    CompareEAXAndEDI,
    Jz
}

public readonly record struct CapacityFinalCalleeCommonMaskedTagEqualityGateFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonMaskedTagEqualityGateReadAttempt(
    uint? RawFileOffset,
    uint? AbsoluteAddress,
    bool ReadReached,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeCommonMaskedTagEqualityGateEvent(
    CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage Stage,
    uint RawFileOffset,
    bool Reached,
    bool Completed,
    uint? EaxValue,
    uint? EbxValue,
    uint? EcxValue,
    byte? ShiftCount,
    uint? Result,
    CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? FlagsAfter,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset);

public readonly record struct CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionEvidence(
    string Mnemonic,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint PEVirtualAddress,
    ImmutableArray<byte> Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetPEVirtualAddress = null);

public readonly record struct CapacityFinalCalleeCommonMaskedTagEqualityGateDestinationEvidence(
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint PEVirtualAddress,
    bool BeforeTargetInstruction);

public sealed record CapacityFinalCalleeCommonMaskedTagEqualityGatePlan(
    CapacityFinalCalleeCommonMaskedTagEqualityGateCapture Capture,
    CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateReadAttempt> ReadAttempts { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateReadAttempt>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateEvent> OrderedEvents { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateEvent>.Empty;
    public uint? EAXValue { get; init; }
    public uint? EBXValue { get; init; }
    public uint? ECXValue { get; init; }
    public bool EntryEAXOperandConsumed { get; init; }
    public bool EntryEDIOperandConsumed { get; init; }
    public bool EntryEDXOperandConsumed { get; init; }
    public bool FreshEDXPlus4OperandConsumed { get; init; }
    public bool EntryEAXCaptureProvidedButUnused =>
        !EntryEAXOperandConsumed && Capture.EntryEAX.HasValue;
    public bool EntryEDICaptureProvidedButUnused =>
        !EntryEDIOperandConsumed && Capture.EntryEDI.HasValue;
    public bool EntryEDXCaptureProvidedButUnused =>
        !EntryEDXOperandConsumed && Capture.EntryEDX.HasValue;
    public bool FreshEDXPlus4CaptureProvidedButUnused =>
        !FreshEDXPlus4OperandConsumed && Capture.FreshEDXPlus4.HasValue;
    public bool FreshReadReached { get; init; }
    public uint? FreshReadAddress { get; init; }
    public bool MaskReached { get; init; }
    public uint? MaskResult { get; init; }
    public CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? MaskFlags { get; init; }
    public bool ShiftReached { get; init; }
    public bool ShiftExecuted { get; init; }
    public uint? ShiftCountSource { get; init; }
    public byte? ShiftCountLowByte { get; init; }
    public byte? ShiftCountMasked { get; init; }
    public uint? ShiftInputEBX { get; init; }
    public uint? ShiftResult { get; init; }
    public CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? ShiftFlags { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareSubtractionResult { get; init; }
    public CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? CompareFlags { get; init; }
    public CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? FlagsAtStop { get; init; }
    public bool JzReached { get; init; }
    public bool? JzTaken { get; init; }
    public uint? JzSelectedDestinationRawFileOffset { get; init; }
    public bool ExactCleanBoundedDestination { get; init; }
    public bool MemoryWritePerformed => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the MOV, fresh [EDX+4] load, tag-bit mask, shift, compare, and
/// equality branch without entering either bounded destination.
/// </summary>
public static class CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C93D4;
    public const uint MoveECXRawFileOffset = 0x012C93D4;
    public const uint MoveEBXRawFileOffset = 0x012C93D6;
    public const uint FreshEDXPlus4MoveRawFileOffset = 0x012C93D8;
    public const uint MaskRawFileOffset = 0x012C93DB;
    public const uint ShiftRawFileOffset = 0x012C93DE;
    public const uint CompareRawFileOffset = 0x012C93E0;
    public const uint JzRawFileOffset = 0x012C93E2;
    public const uint FallThroughDestinationRawFileOffset = 0x012C93E4;
    public const uint EqualDestinationRawFileOffset = 0x012C9403;
    public const uint FreshEDXPlus4Displacement = 4;
    public const uint EAXTagMask = 0xFFFFFFF8;
    public const string CapturedWindowBytes =
        "8B C8 8B DF 8B 42 04 83 E0 F8 D3 E3 3B C7 74 1F";

    public static ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionEvidence>
        ProductionInstructionEvidence { get; } =
        ImmutableArray.Create(
            Instruction(
                "MOV ECX, EAX",
                MoveECXRawFileOffset,
                ImmutableArray.Create<byte>(0x8B, 0xC8)),
            Instruction(
                "MOV EBX, EDI",
                MoveEBXRawFileOffset,
                ImmutableArray.Create<byte>(0x8B, 0xDF)),
            Instruction(
                "MOV EAX, [EDX+4]",
                FreshEDXPlus4MoveRawFileOffset,
                ImmutableArray.Create<byte>(0x8B, 0x42, 0x04)),
            Instruction(
                "AND EAX, FFFFFFF8",
                MaskRawFileOffset,
                ImmutableArray.Create<byte>(0x83, 0xE0, 0xF8)),
            Instruction(
                "SHL EBX, CL",
                ShiftRawFileOffset,
                ImmutableArray.Create<byte>(0xD3, 0xE3)),
            Instruction(
                "CMP EAX, EDI",
                CompareRawFileOffset,
                ImmutableArray.Create<byte>(0x3B, 0xC7)),
            Instruction(
                "JZ 012C9403",
                JzRawFileOffset,
                ImmutableArray.Create<byte>(0x74, 0x1F),
                EqualDestinationRawFileOffset));

    public static ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateDestinationEvidence>
        BoundedDestinations { get; } =
        ImmutableArray.Create(
            Destination(EqualDestinationRawFileOffset),
            Destination(FallThroughDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static bool TryGetInstructionEvidence(
        uint rawFileOffset,
        out CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionEvidence evidence)
    {
        foreach (CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionEvidence instruction
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

    public static CapacityFinalCalleeCommonMaskedTagEqualityGatePlan Project(
        CapacityFinalCalleeCommonMaskedTagEqualityGateCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonMaskedTagEqualityGateReadAttempt>(1);
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonMaskedTagEqualityGateEvent>(7);
        uint? ecx = capture.EntryEAX;
        uint? ebx = null;
        uint? eax = capture.EntryEAX;
        bool eaxOperandConsumed = capture.EntryEAX.HasValue;
        bool ediOperandConsumed = false;
        bool edxOperandConsumed = false;
        bool freshOperandConsumed = false;
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? flags = null;

        events.Add(Event(
            CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.MoveECXFromEAX,
            MoveECXRawFileOffset,
            ecx.HasValue,
            eax,
            ebx,
            ecx,
            flags));
        if (!ecx.HasValue)
        {
            return Stop(
                capture,
                CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingEntryEAX,
                reads,
                events,
                eax,
                ebx,
                ecx,
                eaxOperandConsumed,
                ediOperandConsumed,
                edxOperandConsumed,
                freshOperandConsumed,
                MoveECXRawFileOffset,
                flags);
        }

        ediOperandConsumed = capture.EntryEDI.HasValue;
        ebx = capture.EntryEDI;
        events.Add(Event(
            CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.MoveEBXFromEDI,
            MoveEBXRawFileOffset,
            ebx.HasValue,
            eax,
            ebx,
            ecx,
            flags));
        if (!ebx.HasValue)
        {
            return Stop(
                capture,
                CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingEntryEDI,
                reads,
                events,
                eax,
                ebx,
                ecx,
                eaxOperandConsumed,
                ediOperandConsumed,
                edxOperandConsumed,
                freshOperandConsumed,
                MoveEBXRawFileOffset,
                flags);
        }

        bool loadAddressKnown = capture.EntryEDX.HasValue;
        uint? readAddress = loadAddressKnown
            ? unchecked(capture.EntryEDX!.Value + FreshEDXPlus4Displacement)
            : null;
        edxOperandConsumed = loadAddressKnown;
        bool freshReadReached = loadAddressKnown;
        bool freshValueCaptured = freshReadReached && capture.FreshEDXPlus4.HasValue;
        reads.Add(new(
            FreshEDXPlus4MoveRawFileOffset,
            readAddress,
            freshReadReached,
            freshValueCaptured,
            freshValueCaptured ? capture.FreshEDXPlus4 : null));
        events.Add(Event(
            CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.FreshEDXPlus4MoveToEAX,
            FreshEDXPlus4MoveRawFileOffset,
            freshValueCaptured,
            freshValueCaptured ? capture.FreshEDXPlus4 : eax,
            ebx,
            ecx,
            flags));
        if (!freshReadReached)
        {
            return Stop(
                capture,
                CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingEntryEDX,
                reads,
                events,
                eax,
                ebx,
                ecx,
                eaxOperandConsumed,
                ediOperandConsumed,
                edxOperandConsumed,
                freshOperandConsumed,
                FreshEDXPlus4MoveRawFileOffset,
                flags,
                freshReadReached,
                readAddress);
        }

        if (!freshValueCaptured)
        {
            return Stop(
                capture,
                CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingFreshEDXPlus4,
                reads,
                events,
                eax,
                ebx,
                ecx,
                eaxOperandConsumed,
                ediOperandConsumed,
                edxOperandConsumed,
                freshOperandConsumed,
                FreshEDXPlus4MoveRawFileOffset,
                flags,
                freshReadReached,
                readAddress);
        }

        freshOperandConsumed = true;
        eax = capture.FreshEDXPlus4;
        uint masked = eax!.Value & EAXTagMask;
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags maskFlags =
            CalculateLogicalFlags(masked);
        flags = maskFlags;
        events.Add(Event(
            CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.MaskEAXLowTagBits,
            MaskRawFileOffset,
            true,
            masked,
            ebx,
            ecx,
            flags,
            result: masked));

        uint countSource = ecx!.Value;
        byte countLowByte = (byte)countSource;
        byte shiftCount = (byte)(countLowByte & 31);
        uint shiftInput = ebx!.Value;
        uint shifted = shiftInput << shiftCount;
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags shiftFlags =
            CalculateShiftFlags(shiftInput, shifted, shiftCount, maskFlags);
        flags = shiftFlags;
        events.Add(Event(
            CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.ShiftEBXByCL,
            ShiftRawFileOffset,
            true,
            masked,
            shifted,
            ecx,
            flags,
            shiftCount,
            shifted));

        uint compareRight = capture.EntryEDI!.Value;
        uint difference = unchecked(masked - compareRight);
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags compareFlags =
            CalculateCompareFlags(masked, compareRight, difference);
        flags = compareFlags;
        events.Add(Event(
            CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.CompareEAXAndEDI,
            CompareRawFileOffset,
            true,
            masked,
            shifted,
            ecx,
            flags,
            result: difference));

        bool equal = difference == 0;
        uint destination = equal
            ? EqualDestinationRawFileOffset
            : FallThroughDestinationRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.Jz,
            JzRawFileOffset,
            Reached: true,
            Completed: true,
            masked,
            shifted,
            ecx,
            ShiftCount: null,
            Result: null,
            FlagsAfter: flags,
            BranchTaken: equal,
            SelectedDestinationRawFileOffset: destination));

        return new(
            capture,
            equal
                ? CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.EqualToBounded9403
                : CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.UnequalToFallThrough93E4)
        {
            ReadAttempts = reads.ToImmutable(),
            OrderedEvents = events.ToImmutable(),
            EAXValue = masked,
            EBXValue = shifted,
            ECXValue = ecx,
            EntryEAXOperandConsumed = eaxOperandConsumed,
            EntryEDIOperandConsumed = ediOperandConsumed,
            EntryEDXOperandConsumed = edxOperandConsumed,
            FreshEDXPlus4OperandConsumed = freshOperandConsumed,
            FreshReadReached = freshReadReached,
            FreshReadAddress = readAddress,
            MaskReached = true,
            MaskResult = masked,
            MaskFlags = maskFlags,
            ShiftReached = true,
            ShiftExecuted = true,
            ShiftCountSource = countSource,
            ShiftCountLowByte = countLowByte,
            ShiftCountMasked = shiftCount,
            ShiftInputEBX = shiftInput,
            ShiftResult = shifted,
            ShiftFlags = shiftFlags,
            CompareReached = true,
            CompareLeft = masked,
            CompareRight = compareRight,
            CompareSubtractionResult = difference,
            CompareFlags = compareFlags,
            FlagsAtStop = compareFlags,
            JzReached = true,
            JzTaken = equal,
            JzSelectedDestinationRawFileOffset = destination,
            ExactCleanBoundedDestination = true,
            StopAtRawFileOffsetExclusive = destination
        };
    }

    private static CapacityFinalCalleeCommonMaskedTagEqualityGatePlan Stop(
        CapacityFinalCalleeCommonMaskedTagEqualityGateCapture capture,
        CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome outcome,
        ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateReadAttempt>.Builder reads,
        ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateEvent>.Builder events,
        uint? eax,
        uint? ebx,
        uint? ecx,
        bool eaxOperandConsumed,
        bool ediOperandConsumed,
        bool edxOperandConsumed,
        bool freshOperandConsumed,
        uint stopAt,
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? flags,
        bool freshReadReached = false,
        uint? freshReadAddress = null) =>
        new(capture, outcome)
        {
            ReadAttempts = reads.ToImmutable(),
            OrderedEvents = events.ToImmutable(),
            EAXValue = eax,
            EBXValue = ebx,
            ECXValue = ecx,
            EntryEAXOperandConsumed = eaxOperandConsumed,
            EntryEDIOperandConsumed = ediOperandConsumed,
            EntryEDXOperandConsumed = edxOperandConsumed,
            FreshEDXPlus4OperandConsumed = freshOperandConsumed,
            FreshReadReached = freshReadReached,
            FreshReadAddress = freshReadAddress,
            FlagsAtStop = flags,
            StopAtRawFileOffsetExclusive = stopAt
        };

    private static CapacityFinalCalleeCommonMaskedTagEqualityGateEvent Event(
        CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage stage,
        uint rawFileOffset,
        bool completed,
        uint? eax,
        uint? ebx,
        uint? ecx,
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? flags,
        byte? shiftCount = null,
        uint? result = null) =>
        new(
            stage,
            rawFileOffset,
            Reached: true,
            Completed: completed,
            EaxValue: eax,
            EbxValue: ebx,
            EcxValue: ecx,
            ShiftCount: shiftCount,
            Result: result,
            FlagsAfter: flags,
            BranchTaken: null,
            SelectedDestinationRawFileOffset: null);

    private static CapacityFinalCalleeCommonMaskedTagEqualityGateFlags CalculateLogicalFlags(
        uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000u) != 0,
            Overflow: false);

    private static CapacityFinalCalleeCommonMaskedTagEqualityGateFlags CalculateShiftFlags(
        uint input,
        uint result,
        int count,
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags priorFlags)
    {
        if (count == 0)
        {
            return priorFlags;
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

    private static CapacityFinalCalleeCommonMaskedTagEqualityGateFlags CalculateCompareFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000u) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000u) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionEvidence Instruction(
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

    private static CapacityFinalCalleeCommonMaskedTagEqualityGateDestinationEvidence Destination(
        uint rawFileOffset) =>
        new(
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            BeforeTargetInstruction: true);
}

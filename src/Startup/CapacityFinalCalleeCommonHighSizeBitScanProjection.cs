using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonHighSizeBitScanCapture(
    uint? EntryEAX,
    uint? LateEntryEBP,
    uint? LateEntryEDI);

public enum CapacityFinalCalleeCommonHighSizeBitScanOutcome
{
    MissingEntryEAXAt936F,
    UndefinedZeroBsrResult,
    MissingLateEntryEBPAt9374,
    MissingLateEntryEDIAt9377,
    IndexedResultAt9384BeforeNextStore
}

public enum CapacityFinalCalleeCommonHighSizeBitScanEventKind
{
    BitScanReverse,
    MoveEAXToEDX,
    StoreEDXToFrame,
    MoveEDIToEAX,
    LeaShiftCount,
    ShiftRight,
    AndOne,
    LeaIndexedResult
}

public readonly record struct CapacityFinalCalleeCommonHighSizeBitScanFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonHighSizeBitScanInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct CapacityFinalCalleeCommonHighSizeBitScanEvent(
    CapacityFinalCalleeCommonHighSizeBitScanEventKind Kind,
    uint RawFileOffset,
    uint? Input = null,
    uint? Immediate = null,
    uint? Result = null,
    uint? Address = null,
    bool? DestinationDefined = null,
    bool? Committed = null,
    CapacityFinalCalleeCommonHighSizeBitScanFlags? Flags = null);

public sealed record CapacityFinalCalleeCommonHighSizeBitScanPlan(
    CapacityFinalCalleeCommonHighSizeBitScanCapture Capture,
    CapacityFinalCalleeCommonHighSizeBitScanOutcome Outcome)
{
    public bool BSRReached { get; init; }
    public uint? BSRInputEAX { get; init; }
    public uint? BSRResultEAX { get; init; }
    public bool? BSRDestinationDefined { get; init; }
    public CapacityFinalCalleeCommonHighSizeBitScanFlags? BSRFlags { get; init; }
    public bool MoveEDXReached { get; init; }
    public uint? EDXResult { get; init; }
    public bool StoreAttempted { get; init; }
    public uint? StoreAddress { get; init; }
    public uint? StoreValue { get; init; }
    public bool StoreCommitted { get; init; }
    public bool MoveEDIToEAXReached { get; init; }
    public uint? ShiftInputEAX { get; init; }
    public bool LeaShiftCountReached { get; init; }
    public uint? UnmaskedShiftCount { get; init; }
    public byte? ShiftCount { get; init; }
    public bool ShiftReached { get; init; }
    public uint? ShiftResultEAX { get; init; }
    public CapacityFinalCalleeCommonHighSizeBitScanFlags? ShiftFlags { get; init; }
    public bool AndReached { get; init; }
    public uint? AndResultEAX { get; init; }
    public CapacityFinalCalleeCommonHighSizeBitScanFlags? AndFlags { get; init; }
    public bool FinalLeaReached { get; init; }
    public uint? FinalIndexECX { get; init; }
    public CapacityFinalCalleeCommonHighSizeBitScanFlags? FlagsAtStop { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonHighSizeBitScanEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonHighSizeBitScanEvent>.Empty;
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets =>
        Events.Select(item => item.RawFileOffset).ToImmutableArray();
    public ImmutableArray<uint> Reads { get; init; } = ImmutableArray<uint>.Empty;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the bounded BSR-to-index sequence at raw 012C936F through the
/// instruction ending at 012C9384, without entering the following store.
/// </summary>
public static class CapacityFinalCalleeCommonHighSizeBitScanProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x012C936F;
    public const uint BsrRawFileOffset = EntryRawFileOffset;
    public const uint MoveEDXRawFileOffset = 0x012C9372;
    public const uint FrameStoreRawFileOffset = 0x012C9374;
    public const uint MoveEDIRawFileOffset = 0x012C9377;
    public const uint CountLeaRawFileOffset = 0x012C9379;
    public const uint ShiftRawFileOffset = 0x012C937C;
    public const uint AndRawFileOffset = 0x012C937E;
    public const uint IndexLeaRawFileOffset = 0x012C9381;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9384;
    public const uint FollowingStoreRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const uint FrameStoreDisplacement = 0x14;

    public const string CapturedWindowBytes =
        "0F BD C0 8B D0 89 45 EC 8B C7 8D 4A 07 D3 E8 83 E0 01 8D 0C 50";

    public static ImmutableArray<CapacityFinalCalleeCommonHighSizeBitScanInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("BSR EAX, EAX", BsrRawFileOffset, "0F BD C0"),
            Instruction("Move EAX to EDX", MoveEDXRawFileOffset, "8B D0"),
            Instruction("Store EAX at [EBP-14]", FrameStoreRawFileOffset, "89 45 EC"),
            Instruction("Move EDI to EAX", MoveEDIRawFileOffset, "8B C7"),
            Instruction("LEA ECX, [EDX+7]", CountLeaRawFileOffset, "8D 4A 07"),
            Instruction("SHR EAX, CL", ShiftRawFileOffset, "D3 E8"),
            Instruction("AND EAX, 1", AndRawFileOffset, "83 E0 01"),
            Instruction("LEA ECX, [EAX+EDX*2]", IndexLeaRawFileOffset, "8D 0C 50"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static byte EffectiveShiftCount(uint bitIndex) =>
        (byte)((bitIndex + 7) & 31);

    public static CapacityFinalCalleeCommonHighSizeBitScanPlan Project(
        CapacityFinalCalleeCommonHighSizeBitScanCapture capture)
    {
        CapacityFinalCalleeCommonHighSizeBitScanPlan Stop(
            CapacityFinalCalleeCommonHighSizeBitScanOutcome outcome,
            uint stopAt,
            ImmutableArray<CapacityFinalCalleeCommonHighSizeBitScanEvent> events = default,
            bool bsrReached = false,
            uint? bsrInput = null,
            uint? bsrResult = null,
            bool? bsrDestinationDefined = null,
            CapacityFinalCalleeCommonHighSizeBitScanFlags? bsrFlags = null,
            bool moveEDXReached = false,
            uint? edxResult = null,
            bool storeAttempted = false,
            uint? storeAddress = null,
            uint? storeValue = null,
            bool storeCommitted = false,
            bool moveEDIReached = false,
            uint? shiftInput = null,
            bool leaShiftCountReached = false,
            uint? unmaskedShiftCount = null,
            byte? shiftCount = null,
            bool shiftReached = false,
            uint? shiftResult = null,
            CapacityFinalCalleeCommonHighSizeBitScanFlags? shiftFlags = null,
            bool andReached = false,
            uint? andResult = null,
            CapacityFinalCalleeCommonHighSizeBitScanFlags? andFlags = null,
            bool finalLeaReached = false,
            uint? finalIndex = null,
            CapacityFinalCalleeCommonHighSizeBitScanFlags? flagsAtStop = null,
            ImmutableArray<uint> writes = default) =>
            new(capture, outcome)
            {
                BSRReached = bsrReached,
                BSRInputEAX = bsrInput,
                BSRResultEAX = bsrResult,
                BSRDestinationDefined = bsrDestinationDefined,
                BSRFlags = bsrFlags,
                MoveEDXReached = moveEDXReached,
                EDXResult = edxResult,
                StoreAttempted = storeAttempted,
                StoreAddress = storeAddress,
                StoreValue = storeValue,
                StoreCommitted = storeCommitted,
                MoveEDIToEAXReached = moveEDIReached,
                ShiftInputEAX = shiftInput,
                LeaShiftCountReached = leaShiftCountReached,
                UnmaskedShiftCount = unmaskedShiftCount,
                ShiftCount = shiftCount,
                ShiftReached = shiftReached,
                ShiftResultEAX = shiftResult,
                ShiftFlags = shiftFlags,
                AndReached = andReached,
                AndResultEAX = andResult,
                AndFlags = andFlags,
                FinalLeaReached = finalLeaReached,
                FinalIndexECX = finalIndex,
                FlagsAtStop = flagsAtStop,
                Events = events.IsDefault
                    ? ImmutableArray<CapacityFinalCalleeCommonHighSizeBitScanEvent>.Empty
                    : events,
                Writes = writes.IsDefault ? ImmutableArray<uint>.Empty : writes,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEAX is not uint entryEAX)
        {
            return Stop(
                CapacityFinalCalleeCommonHighSizeBitScanOutcome.MissingEntryEAXAt936F,
                EntryRawFileOffset);
        }

        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonHighSizeBitScanEvent>();
        if (entryEAX == 0)
        {
            CapacityFinalCalleeCommonHighSizeBitScanFlags zeroBsrFlags =
                new(null, null, null, true, null, null);
            events.Add(new(
                CapacityFinalCalleeCommonHighSizeBitScanEventKind.BitScanReverse,
                BsrRawFileOffset,
                Input: entryEAX,
                DestinationDefined: false,
                Flags: zeroBsrFlags));
            return Stop(
                CapacityFinalCalleeCommonHighSizeBitScanOutcome.UndefinedZeroBsrResult,
                MoveEDXRawFileOffset,
                events.ToImmutable(),
                bsrReached: true,
                bsrInput: entryEAX,
                bsrDestinationDefined: false,
                bsrFlags: zeroBsrFlags,
                flagsAtStop: zeroBsrFlags);
        }

        uint bitIndex = (uint)BitOperations.Log2(entryEAX);
        CapacityFinalCalleeCommonHighSizeBitScanFlags bsrFlags =
            new(null, null, null, false, null, null);
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeBitScanEventKind.BitScanReverse,
            BsrRawFileOffset,
            Input: entryEAX,
            Result: bitIndex,
            DestinationDefined: true,
            Flags: bsrFlags));

        events.Add(new(
            CapacityFinalCalleeCommonHighSizeBitScanEventKind.MoveEAXToEDX,
            MoveEDXRawFileOffset,
            Input: bitIndex,
            Result: bitIndex,
            Flags: bsrFlags));

        if (capture.LateEntryEBP is not uint entryEBP)
        {
            events.Add(new(
                CapacityFinalCalleeCommonHighSizeBitScanEventKind.StoreEDXToFrame,
                FrameStoreRawFileOffset,
                Input: bitIndex,
                Address: null,
                Committed: false,
                Flags: bsrFlags));
            return Stop(
                CapacityFinalCalleeCommonHighSizeBitScanOutcome.MissingLateEntryEBPAt9374,
                FrameStoreRawFileOffset,
                events.ToImmutable(),
                bsrReached: true,
                bsrInput: entryEAX,
                bsrResult: bitIndex,
                bsrDestinationDefined: true,
                bsrFlags: bsrFlags,
                moveEDXReached: true,
                edxResult: bitIndex,
                storeAttempted: true,
                storeValue: bitIndex,
                flagsAtStop: bsrFlags);
        }

        uint storeAddress = unchecked(entryEBP - FrameStoreDisplacement);
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeBitScanEventKind.StoreEDXToFrame,
            FrameStoreRawFileOffset,
            Input: bitIndex,
            Address: storeAddress,
            Committed: true,
            Flags: bsrFlags));
        ImmutableArray<uint> writes = ImmutableArray.Create(storeAddress);

        if (capture.LateEntryEDI is not uint entryEDI)
        {
            return Stop(
                CapacityFinalCalleeCommonHighSizeBitScanOutcome.MissingLateEntryEDIAt9377,
                MoveEDIRawFileOffset,
                events.ToImmutable(),
                bsrReached: true,
                bsrInput: entryEAX,
                bsrResult: bitIndex,
                bsrDestinationDefined: true,
                bsrFlags: bsrFlags,
                moveEDXReached: true,
                edxResult: bitIndex,
                storeAttempted: true,
                storeAddress: storeAddress,
                storeValue: bitIndex,
                storeCommitted: true,
                flagsAtStop: bsrFlags,
                writes: writes);
        }

        events.Add(new(
            CapacityFinalCalleeCommonHighSizeBitScanEventKind.MoveEDIToEAX,
            MoveEDIRawFileOffset,
            Input: entryEDI,
            Result: entryEDI,
            Flags: bsrFlags));

        uint unmaskedShiftCount = unchecked(bitIndex + 7);
        byte shiftCount = EffectiveShiftCount(bitIndex);
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeBitScanEventKind.LeaShiftCount,
            CountLeaRawFileOffset,
            Input: bitIndex,
            Immediate: 7,
            Result: unmaskedShiftCount,
            Flags: bsrFlags));

        uint shifted = entryEDI >> shiftCount;
        CapacityFinalCalleeCommonHighSizeBitScanFlags shiftFlags =
            shiftCount == 0
                ? bsrFlags
                : CalculateShiftFlags(entryEDI, shifted, shiftCount);
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeBitScanEventKind.ShiftRight,
            ShiftRawFileOffset,
            Input: entryEDI,
            Immediate: shiftCount,
            Result: shifted,
            Flags: shiftFlags));

        uint lowBit = shifted & 1;
        CapacityFinalCalleeCommonHighSizeBitScanFlags andFlags = CalculateAndFlags(lowBit);
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeBitScanEventKind.AndOne,
            AndRawFileOffset,
            Input: shifted,
            Immediate: 1,
            Result: lowBit,
            Flags: andFlags));

        uint finalIndex = unchecked((bitIndex * 2) + lowBit);
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeBitScanEventKind.LeaIndexedResult,
            IndexLeaRawFileOffset,
            Input: lowBit,
            Immediate: bitIndex * 2,
            Result: finalIndex,
            Flags: andFlags));

        return Stop(
            CapacityFinalCalleeCommonHighSizeBitScanOutcome.IndexedResultAt9384BeforeNextStore,
            BoundaryRawFileOffsetExclusive,
            events.ToImmutable(),
            bsrReached: true,
            bsrInput: entryEAX,
            bsrResult: bitIndex,
            bsrDestinationDefined: true,
            bsrFlags: bsrFlags,
            moveEDXReached: true,
            edxResult: bitIndex,
            storeAttempted: true,
            storeAddress: storeAddress,
            storeValue: bitIndex,
            storeCommitted: true,
            moveEDIReached: true,
            shiftInput: entryEDI,
            leaShiftCountReached: true,
            unmaskedShiftCount: unmaskedShiftCount,
            shiftCount: shiftCount,
            shiftReached: true,
            shiftResult: shifted,
            shiftFlags: shiftFlags,
            andReached: true,
            andResult: lowBit,
            andFlags: andFlags,
            finalLeaReached: true,
            finalIndex: finalIndex,
            flagsAtStop: andFlags,
            writes: writes);
    }

    private static CapacityFinalCalleeCommonHighSizeBitScanFlags CalculateShiftFlags(
        uint input,
        uint result,
        byte count) =>
        new(
            Carry: ((input >> (count - 1)) & 1) != 0,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: count == 1 ? (input & 0x80000000) != 0 : null);

    private static CapacityFinalCalleeCommonHighSizeBitScanFlags CalculateAndFlags(uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: false,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeCommonHighSizeBitScanInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes);
}

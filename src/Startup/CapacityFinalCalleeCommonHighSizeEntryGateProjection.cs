using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonHighSizeEntryGateCapture(
    uint? EntryEDI);

public enum CapacityFinalCalleeCommonHighSizeEntryGateOutcome
{
    MissingEntryEDI,
    HighSizeAt9361BeforeCompare,
    ZeroSizeAt9384BeforeIndexedStore
}

public enum CapacityFinalCalleeCommonHighSizeEntryGateEventKind
{
    Move,
    ShiftRight,
    Test,
    BranchNotZero,
    Xor,
    Jump
}

public readonly record struct CapacityFinalCalleeCommonHighSizeEntryGateFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonHighSizeEntryGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeCommonHighSizeEntryGateEvent(
    CapacityFinalCalleeCommonHighSizeEntryGateEventKind Kind,
    uint RawFileOffset,
    uint? Input = null,
    uint? Result = null,
    CapacityFinalCalleeCommonHighSizeEntryGateFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public sealed record CapacityFinalCalleeCommonHighSizeEntryGatePlan(
    CapacityFinalCalleeCommonHighSizeEntryGateCapture Capture,
    CapacityFinalCalleeCommonHighSizeEntryGateOutcome Outcome)
{
    public bool EntryEDICaptured { get; init; }
    public bool MoveReached { get; init; }
    public uint? MoveResultEAX { get; init; }
    public bool ShiftReached { get; init; }
    public uint? ShiftInputEAX { get; init; }
    public byte? ShiftCount { get; init; }
    public uint? ShiftResultEAX { get; init; }
    public CapacityFinalCalleeCommonHighSizeEntryGateFlags? ShiftFlags { get; init; }
    public bool TestReached { get; init; }
    public uint? TestInputEAX { get; init; }
    public CapacityFinalCalleeCommonHighSizeEntryGateFlags? TestFlags { get; init; }
    public bool JNZReached { get; init; }
    public bool? JNZTaken { get; init; }
    public uint? JNZSelectedDestinationRawFileOffset { get; init; }
    public bool XorReached { get; init; }
    public uint? XorResultECX { get; init; }
    public CapacityFinalCalleeCommonHighSizeEntryGateFlags? XorFlags { get; init; }
    public bool JMPReached { get; init; }
    public bool? JMPTaken { get; init; }
    public uint? JMPSelectedDestinationRawFileOffset { get; init; }
    public CapacityFinalCalleeCommonHighSizeEntryGateFlags? FlagsAtStop =>
        JMPReached ? XorFlags : TestFlags;
    public ImmutableArray<CapacityFinalCalleeCommonHighSizeEntryGateEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonHighSizeEntryGateEvent>.Empty;
    public bool FrameReadReached => false;
    public bool GlobalReadReached => false;
    public ImmutableArray<uint> Reads { get; init; } = ImmutableArray<uint>.Empty;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public bool ExactCleanAdmission { get; init; }
    public bool TargetEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the SHR/TEST/JNZ high-size entry gate at 9354, stopping at its
/// selected destination without entering the comparison or indexed-store path.
/// </summary>
public static class CapacityFinalCalleeCommonHighSizeEntryGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9354;
    public const uint EntryVirtualAddress = 0x016C9F54;
    public const uint MoveRawFileOffset = 0x012C9354;
    public const uint ShiftRawFileOffset = 0x012C9356;
    public const uint TestRawFileOffset = 0x012C9359;
    public const uint JNZRawFileOffset = 0x012C935B;
    public const uint XorRawFileOffset = 0x012C935D;
    public const uint JMPRawFileOffset = 0x012C935F;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9361;
    public const uint HighSizeDestinationRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const uint ZeroSizeDestinationRawFileOffset = 0x012C9384;

    public const byte ShiftCount = 8;

    public const string CapturedWindowBytes =
        "8B C7 C1 E8 08 85 C0 75 04 33 C9 EB 23";

    public static ImmutableArray<CapacityFinalCalleeCommonHighSizeEntryGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Move entry EDI into EAX",
                MoveRawFileOffset,
                "8B C7"),
            Instruction(
                "Shift EAX right by eight",
                ShiftRawFileOffset,
                "C1 E8 08"),
            Instruction(
                "Test shifted EAX against itself",
                TestRawFileOffset,
                "85 C0"),
            Instruction(
                "Branch to 9361 when shifted EAX is nonzero",
                JNZRawFileOffset,
                "75 04",
                branchTargetRawFileOffset: HighSizeDestinationRawFileOffset),
            Instruction(
                "Zero ECX when shifted EAX is zero",
                XorRawFileOffset,
                "33 C9"),
            Instruction(
                "Jump to 9384 before the indexed store path",
                JMPRawFileOffset,
                "EB 23",
                branchTargetRawFileOffset: ZeroSizeDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ShiftRight(uint value, int count) =>
        value >> (count & 31);

    public static CapacityFinalCalleeCommonHighSizeEntryGatePlan Project(
        CapacityFinalCalleeCommonHighSizeEntryGateCapture capture)
    {
        CapacityFinalCalleeCommonHighSizeEntryGatePlan Finish(
            CapacityFinalCalleeCommonHighSizeEntryGateOutcome outcome,
            bool entryCaptured = false,
            bool moveReached = false,
            uint? moveResult = null,
            bool shiftReached = false,
            uint? shiftInput = null,
            uint? shiftResult = null,
            CapacityFinalCalleeCommonHighSizeEntryGateFlags? shiftFlags = null,
            bool testReached = false,
            uint? testInput = null,
            CapacityFinalCalleeCommonHighSizeEntryGateFlags? testFlags = null,
            bool jnzReached = false,
            bool? jnzTaken = null,
            uint? jnzDestination = null,
            bool xorReached = false,
            uint? xorResult = null,
            CapacityFinalCalleeCommonHighSizeEntryGateFlags? xorFlags = null,
            bool jmpReached = false,
            bool? jmpTaken = null,
            uint? jmpDestination = null,
            bool exactCleanAdmission = false,
            uint stopAt = EntryRawFileOffset,
            ImmutableArray<CapacityFinalCalleeCommonHighSizeEntryGateEvent> events = default) =>
            new(capture, outcome)
            {
                EntryEDICaptured = entryCaptured,
                MoveReached = moveReached,
                MoveResultEAX = moveResult,
                ShiftReached = shiftReached,
                ShiftInputEAX = shiftInput,
                ShiftCount = shiftReached ? ShiftCount : null,
                ShiftResultEAX = shiftResult,
                ShiftFlags = shiftFlags,
                TestReached = testReached,
                TestInputEAX = testInput,
                TestFlags = testFlags,
                JNZReached = jnzReached,
                JNZTaken = jnzTaken,
                JNZSelectedDestinationRawFileOffset = jnzDestination,
                XorReached = xorReached,
                XorResultECX = xorResult,
                XorFlags = xorFlags,
                JMPReached = jmpReached,
                JMPTaken = jmpTaken,
                JMPSelectedDestinationRawFileOffset = jmpDestination,
                Events = events.IsDefault
                    ? ImmutableArray<CapacityFinalCalleeCommonHighSizeEntryGateEvent>.Empty
                    : events,
                ExactCleanAdmission = exactCleanAdmission,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEDI is not uint entryEdi)
        {
            return Finish(CapacityFinalCalleeCommonHighSizeEntryGateOutcome.MissingEntryEDI);
        }

        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonHighSizeEntryGateEvent>();
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Move,
            MoveRawFileOffset,
            Input: entryEdi,
            Result: entryEdi));

        uint shifted = ShiftRight(entryEdi, ShiftCount);
        CapacityFinalCalleeCommonHighSizeEntryGateFlags shiftFlags =
            CalculateShiftFlags(entryEdi, shifted);
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeEntryGateEventKind.ShiftRight,
            ShiftRawFileOffset,
            Input: entryEdi,
            Result: shifted,
            Flags: shiftFlags));

        CapacityFinalCalleeCommonHighSizeEntryGateFlags testFlags =
            CalculateTestFlags(shifted);
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Test,
            TestRawFileOffset,
            Input: shifted,
            Result: shifted,
            Flags: testFlags));

        bool jnzTaken = shifted != 0;
        uint jnzDestination = jnzTaken
            ? HighSizeDestinationRawFileOffset
            : XorRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeEntryGateEventKind.BranchNotZero,
            JNZRawFileOffset,
            Input: shifted,
            Flags: testFlags,
            BranchTaken: jnzTaken,
            SelectedDestinationRawFileOffset: jnzDestination));

        if (jnzTaken)
        {
            return Finish(
                CapacityFinalCalleeCommonHighSizeEntryGateOutcome.HighSizeAt9361BeforeCompare,
                entryCaptured: true,
                moveReached: true,
                moveResult: entryEdi,
                shiftReached: true,
                shiftInput: entryEdi,
                shiftResult: shifted,
                shiftFlags: shiftFlags,
                testReached: true,
                testInput: shifted,
                testFlags: testFlags,
                jnzReached: true,
                jnzTaken: true,
                jnzDestination: jnzDestination,
                exactCleanAdmission: true,
                stopAt: HighSizeDestinationRawFileOffset,
                events: events.ToImmutable());
        }

        CapacityFinalCalleeCommonHighSizeEntryGateFlags xorFlags = CalculateXorZeroFlags();
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Xor,
            XorRawFileOffset,
            Result: 0,
            Flags: xorFlags));

        events.Add(new(
            CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Jump,
            JMPRawFileOffset,
            Flags: xorFlags,
            BranchTaken: true,
            SelectedDestinationRawFileOffset: ZeroSizeDestinationRawFileOffset));

        return Finish(
            CapacityFinalCalleeCommonHighSizeEntryGateOutcome.ZeroSizeAt9384BeforeIndexedStore,
            entryCaptured: true,
            moveReached: true,
            moveResult: entryEdi,
            shiftReached: true,
            shiftInput: entryEdi,
            shiftResult: shifted,
            shiftFlags: shiftFlags,
            testReached: true,
            testInput: shifted,
            testFlags: testFlags,
            jnzReached: true,
            jnzTaken: false,
            jnzDestination: jnzDestination,
            xorReached: true,
            xorResult: 0,
            xorFlags: xorFlags,
            jmpReached: true,
            jmpTaken: true,
            jmpDestination: ZeroSizeDestinationRawFileOffset,
            exactCleanAdmission: true,
            stopAt: ZeroSizeDestinationRawFileOffset,
            events: events.ToImmutable());
    }

    private static CapacityFinalCalleeCommonHighSizeEntryGateFlags CalculateShiftFlags(
        uint input,
        uint result) =>
        new(
            Carry: (input & (1u << (ShiftCount - 1))) != 0,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: null);

    private static CapacityFinalCalleeCommonHighSizeEntryGateFlags CalculateTestFlags(
        uint value) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)value),
            Auxiliary: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static CapacityFinalCalleeCommonHighSizeEntryGateFlags CalculateXorZeroFlags() =>
        new(
            Carry: false,
            Parity: true,
            Auxiliary: null,
            Zero: true,
            Sign: false,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeCommonHighSizeEntryGateInstructionEvidence Instruction(
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

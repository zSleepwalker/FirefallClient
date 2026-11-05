using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonHighSizeCeilingGateCapture(
    uint? EntryEAX);

public enum CapacityFinalCalleeCommonHighSizeCeilingGateOutcome
{
    MissingEntryEAX,
    UnsignedBelowOrEqualToCeilingAt936FBeforeBSR,
    UnsignedAboveCeilingAt9384BeforeStore
}

public enum CapacityFinalCalleeCommonHighSizeCeilingGateEventKind
{
    Compare,
    BranchBelowOrEqual,
    Move,
    Jump
}

public readonly record struct CapacityFinalCalleeCommonHighSizeCeilingGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeCommonHighSizeCeilingGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeCommonHighSizeCeilingGateEvent(
    CapacityFinalCalleeCommonHighSizeCeilingGateEventKind Kind,
    uint RawFileOffset,
    uint? Input = null,
    uint? Immediate = null,
    uint? Result = null,
    CapacityFinalCalleeCommonHighSizeCeilingGateFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public sealed record CapacityFinalCalleeCommonHighSizeCeilingGatePlan(
    CapacityFinalCalleeCommonHighSizeCeilingGateCapture Capture,
    CapacityFinalCalleeCommonHighSizeCeilingGateOutcome Outcome)
{
    public bool CompareReached { get; init; }
    public uint? CompareInputEAX { get; init; }
    public uint? CompareImmediate { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeCommonHighSizeCeilingGateFlags? CompareFlags { get; init; }
    public bool JBEReached { get; init; }
    public bool? JBETaken { get; init; }
    public uint? JBESelectedDestinationRawFileOffset { get; init; }
    public bool MoveReached { get; init; }
    public uint? MoveResultECX { get; init; }
    public bool JMPReached { get; init; }
    public bool? JMPTaken { get; init; }
    public uint? JMPSelectedDestinationRawFileOffset { get; init; }
    public CapacityFinalCalleeCommonHighSizeCeilingGateFlags? FlagsAtStop { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonHighSizeCeilingGateEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonHighSizeCeilingGateEvent>.Empty;
    public bool FrameReadReached => false;
    public bool GlobalReadReached => false;
    public ImmutableArray<uint> Reads { get; init; } = ImmutableArray<uint>.Empty;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public bool TargetEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the standalone unsigned EAX-versus-FFFF ceiling gate at 9361,
/// stopping at either selected destination without entering BSR or the store.
/// </summary>
public static class CapacityFinalCalleeCommonHighSizeCeilingGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9361;
    public const uint EntryVirtualAddress = 0x016C9F61;
    public const uint CompareRawFileOffset = 0x012C9361;
    public const uint JBERawFileOffset = 0x012C9366;
    public const uint MoveRawFileOffset = 0x012C9368;
    public const uint JMPRawFileOffset = 0x012C936D;
    public const uint BelowOrEqualDestinationRawFileOffset = 0x012C936F;
    public const uint AboveDestinationRawFileOffset = 0x012C9384;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C936F;
    public const uint Ceiling = 0x0000FFFF;
    public const uint AbovePathECX = 31;

    public const string CapturedWindowBytes =
        "3D FF FF 00 00 76 07 B9 1F 00 00 00 EB 15";

    public static ImmutableArray<CapacityFinalCalleeCommonHighSizeCeilingGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Compare full unsigned EAX against 0000FFFF",
                CompareRawFileOffset,
                "3D FF FF 00 00"),
            Instruction(
                "Branch to 936F when unsigned EAX is below or equal",
                JBERawFileOffset,
                "76 07",
                branchTargetRawFileOffset: BelowOrEqualDestinationRawFileOffset),
            Instruction(
                "Set ECX to 31 on the unsigned-above path",
                MoveRawFileOffset,
                "B9 1F 00 00 00"),
            Instruction(
                "Jump to 9384 before the store",
                JMPRawFileOffset,
                "EB 15",
                branchTargetRawFileOffset: AboveDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonHighSizeCeilingGatePlan Project(
        CapacityFinalCalleeCommonHighSizeCeilingGateCapture capture)
    {
        CapacityFinalCalleeCommonHighSizeCeilingGatePlan Finish(
            CapacityFinalCalleeCommonHighSizeCeilingGateOutcome outcome,
            bool compareReached = false,
            uint? compareInput = null,
            uint? compareResult = null,
            CapacityFinalCalleeCommonHighSizeCeilingGateFlags? compareFlags = null,
            bool jbeReached = false,
            bool? jbeTaken = null,
            uint? jbeDestination = null,
            bool moveReached = false,
            uint? moveResult = null,
            bool jmpReached = false,
            bool? jmpTaken = null,
            uint? jmpDestination = null,
            uint stopAt = EntryRawFileOffset,
            ImmutableArray<CapacityFinalCalleeCommonHighSizeCeilingGateEvent> events = default) =>
            new(capture, outcome)
            {
                CompareReached = compareReached,
                CompareInputEAX = compareInput,
                CompareImmediate = compareReached ? Ceiling : null,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JBEReached = jbeReached,
                JBETaken = jbeTaken,
                JBESelectedDestinationRawFileOffset = jbeDestination,
                MoveReached = moveReached,
                MoveResultECX = moveResult,
                JMPReached = jmpReached,
                JMPTaken = jmpTaken,
                JMPSelectedDestinationRawFileOffset = jmpDestination,
                FlagsAtStop = compareFlags,
                Events = events.IsDefault
                    ? ImmutableArray<CapacityFinalCalleeCommonHighSizeCeilingGateEvent>.Empty
                    : events,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEAX is not uint entryEax)
        {
            return Finish(CapacityFinalCalleeCommonHighSizeCeilingGateOutcome.MissingEntryEAX);
        }

        uint compareResult = unchecked(entryEax - Ceiling);
        CapacityFinalCalleeCommonHighSizeCeilingGateFlags compareFlags =
            CalculateCompareFlags(entryEax, Ceiling, compareResult);
        bool jbeTaken = compareFlags.Carry || compareFlags.Zero;
        uint jbeDestination = jbeTaken
            ? BelowOrEqualDestinationRawFileOffset
            : MoveRawFileOffset;

        var events = ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonHighSizeCeilingGateEvent>();
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeCeilingGateEventKind.Compare,
            CompareRawFileOffset,
            Input: entryEax,
            Immediate: Ceiling,
            Result: compareResult,
            Flags: compareFlags));
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeCeilingGateEventKind.BranchBelowOrEqual,
            JBERawFileOffset,
            Input: entryEax,
            Immediate: Ceiling,
            Flags: compareFlags,
            BranchTaken: jbeTaken,
            SelectedDestinationRawFileOffset: jbeDestination));

        if (jbeTaken)
        {
            return Finish(
                CapacityFinalCalleeCommonHighSizeCeilingGateOutcome
                    .UnsignedBelowOrEqualToCeilingAt936FBeforeBSR,
                compareReached: true,
                compareInput: entryEax,
                compareResult: compareResult,
                compareFlags: compareFlags,
                jbeReached: true,
                jbeTaken: true,
                jbeDestination: jbeDestination,
                stopAt: BelowOrEqualDestinationRawFileOffset,
                events: events.ToImmutable());
        }

        events.Add(new(
            CapacityFinalCalleeCommonHighSizeCeilingGateEventKind.Move,
            MoveRawFileOffset,
            Immediate: AbovePathECX,
            Result: AbovePathECX,
            Flags: compareFlags));
        events.Add(new(
            CapacityFinalCalleeCommonHighSizeCeilingGateEventKind.Jump,
            JMPRawFileOffset,
            Flags: compareFlags,
            BranchTaken: true,
            SelectedDestinationRawFileOffset: AboveDestinationRawFileOffset));

        return Finish(
            CapacityFinalCalleeCommonHighSizeCeilingGateOutcome
                .UnsignedAboveCeilingAt9384BeforeStore,
            compareReached: true,
            compareInput: entryEax,
            compareResult: compareResult,
            compareFlags: compareFlags,
            jbeReached: true,
            jbeTaken: false,
            jbeDestination: jbeDestination,
            moveReached: true,
            moveResult: AbovePathECX,
            jmpReached: true,
            jmpTaken: true,
            jmpDestination: AboveDestinationRawFileOffset,
            stopAt: AboveDestinationRawFileOffset,
            events: events.ToImmutable());
    }

    private static CapacityFinalCalleeCommonHighSizeCeilingGateFlags CalculateCompareFlags(
        uint input,
        uint immediate,
        uint result) =>
        new(
            Carry: input < immediate,
            Parity: HasEvenParity((byte)result),
            Auxiliary: (input & 0xF) < (immediate & 0xF),
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((input ^ immediate) & (input ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeCommonHighSizeCeilingGateInstructionEvidence Instruction(
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

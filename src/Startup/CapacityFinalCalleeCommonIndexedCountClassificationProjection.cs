using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonIndexedCountClassificationCapture(
    uint? EntryEBX,
    uint? FreshIndexedDWORD,
    uint? LateEntryECX);

public enum CapacityFinalCalleeCommonIndexedCountClassificationOutcome
{
    MissingEntryEBX,
    MissingFreshIndexedDWORDAt93C0,
    MissingLateEntryECXAt93C2,
    Equal31At93D4,
    NonEqualAt93D4
}

public readonly record struct CapacityFinalCalleeCommonIndexedCountClassificationFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonIndexedCountClassificationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCommonIndexedCountClassificationPlan(
    CapacityFinalCalleeCommonIndexedCountClassificationCapture Capture,
    CapacityFinalCalleeCommonIndexedCountClassificationOutcome Outcome)
{
    public bool IndexedDWORDReadAttempted { get; init; }
    public bool IndexedDWORDReadCompleted { get; init; }
    public uint? IndexedDWORDReadAddress { get; init; }
    public uint? FreshEDX { get; init; }
    public bool UnreachedFreshIndexedDWORDObservationProvided =>
        !IndexedDWORDReadAttempted && Capture.FreshIndexedDWORD.HasValue;
    public bool CompareReached { get; init; }
    public bool CompareInstructionReached => IndexedDWORDReadCompleted;
    public bool UnreachedLateEntryECXCaptureProvided =>
        !CompareInstructionReached && Capture.LateEntryECX.HasValue;
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeCommonIndexedCountClassificationFlags? CompareFlags { get; init; }
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? JnzSelectedDestinationRawFileOffset { get; init; }
    public bool XorEAXReached { get; init; }
    public bool XorEAXInputKnown { get; init; }
    public uint? XorEAXResult { get; init; }
    public CapacityFinalCalleeCommonIndexedCountClassificationFlags? XorFlags { get; init; }
    public bool JumpReached { get; init; }
    public uint? JumpSelectedDestinationRawFileOffset { get; init; }
    public bool ShiftECXReached { get; init; }
    public uint? ShiftInputECX { get; init; }
    public uint? ShiftResultECX { get; init; }
    public CapacityFinalCalleeCommonIndexedCountClassificationFlags? ShiftFlags { get; init; }
    public bool MoveImmediateEAXReached { get; init; }
    public uint? MoveImmediateEAXResult { get; init; }
    public bool SubtractReached { get; init; }
    public uint? SubtractLeft { get; init; }
    public uint? SubtractRight { get; init; }
    public uint? SubtractResult { get; init; }
    public CapacityFinalCalleeCommonIndexedCountClassificationFlags? SubtractFlags { get; init; }
    public CapacityFinalCalleeCommonIndexedCountClassificationFlags? FlagsAtStop { get; init; }
    public ImmutableArray<uint> Reads { get; init; } = ImmutableArray<uint>.Empty;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the fresh indexed DWORD load and ECX classification at 93C0,
/// stopping before the instruction at 93D4.
/// </summary>
public static class CapacityFinalCalleeCommonIndexedCountClassificationProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x012C93C0;
    public const uint FreshLoadRawFileOffset = EntryRawFileOffset;
    public const uint CompareRawFileOffset = 0x012C93C2;
    public const uint JnzRawFileOffset = 0x012C93C5;
    public const uint XorRawFileOffset = 0x012C93C7;
    public const uint JumpRawFileOffset = 0x012C93C9;
    public const uint ShiftRawFileOffset = 0x012C93CB;
    public const uint MoveImmediateRawFileOffset = 0x012C93CD;
    public const uint SubtractRawFileOffset = 0x012C93D2;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C93D4;
    public const uint FollowingInstructionRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const uint CompareImmediate = 0x1F;
    public const uint NonEqualEAXImmediate = 0x19;
    public const uint JnzBranchTargetRawFileOffset = 0x012C93CB;
    public const uint EqualBranchTargetRawFileOffset = 0x012C93D4;

    public const string CapturedWindowBytes =
        "8B 13 83 F9 1F 75 04 33 C0 EB 09 D1 E9 B8 19 00 00 00 2B C1";

    public static ImmutableArray<CapacityFinalCalleeCommonIndexedCountClassificationInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Read the fresh indexed DWORD at [EBX] into EDX",
                FreshLoadRawFileOffset, "8B 13"),
            Instruction("Compare late entry ECX with 1Fh",
                CompareRawFileOffset, "83 F9 1F"),
            Instruction("Branch to the non-equal path when ECX is not 1Fh",
                JnzRawFileOffset, "75 04",
                branchTargetRawFileOffset: JnzBranchTargetRawFileOffset),
            Instruction("Clear EAX by XOR; EAX's incoming value is not assumed",
                XorRawFileOffset, "33 C0"),
            Instruction("Jump over the non-equal path to the exclusive boundary",
                JumpRawFileOffset, "EB 09",
                branchTargetRawFileOffset: EqualBranchTargetRawFileOffset),
            Instruction("Shift ECX right by one",
                ShiftRawFileOffset, "D1 E9"),
            Instruction("Load 19h into EAX",
                MoveImmediateRawFileOffset, "B8 19 00 00 00"),
            Instruction("Subtract shifted ECX from EAX",
                SubtractRawFileOffset, "2B C1"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonIndexedCountClassificationPlan Project(
        CapacityFinalCalleeCommonIndexedCountClassificationCapture capture)
    {
        CapacityFinalCalleeCommonIndexedCountClassificationPlan Stop(
            CapacityFinalCalleeCommonIndexedCountClassificationOutcome outcome,
            uint stopAt,
            bool readAttempted = false,
            bool readCompleted = false,
            uint? readAddress = null,
            uint? freshEDX = null,
            bool compareReached = false,
            uint? compareLeft = null,
            uint? compareResult = null,
            CapacityFinalCalleeCommonIndexedCountClassificationFlags? compareFlags = null,
            bool jnzReached = false,
            bool? jnzTaken = null,
            uint? jnzDestination = null,
            bool xorReached = false,
            uint? xorResult = null,
            CapacityFinalCalleeCommonIndexedCountClassificationFlags? xorFlags = null,
            bool jumpReached = false,
            uint? jumpDestination = null,
            bool shiftReached = false,
            uint? shiftInput = null,
            uint? shiftResult = null,
            CapacityFinalCalleeCommonIndexedCountClassificationFlags? shiftFlags = null,
            bool moveImmediateReached = false,
            uint? moveImmediateResult = null,
            bool subtractReached = false,
            uint? subtractRight = null,
            uint? subtractResult = null,
            CapacityFinalCalleeCommonIndexedCountClassificationFlags? subtractFlags = null,
            CapacityFinalCalleeCommonIndexedCountClassificationFlags? flagsAtStop = null) =>
            new(capture, outcome)
            {
                IndexedDWORDReadAttempted = readAttempted,
                IndexedDWORDReadCompleted = readCompleted,
                IndexedDWORDReadAddress = readAddress,
                FreshEDX = freshEDX,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = readCompleted ? CompareImmediate : null,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JnzReached = jnzReached,
                JnzTaken = jnzTaken,
                JnzSelectedDestinationRawFileOffset = jnzDestination,
                XorEAXReached = xorReached,
                XorEAXInputKnown = false,
                XorEAXResult = xorResult,
                XorFlags = xorFlags,
                JumpReached = jumpReached,
                JumpSelectedDestinationRawFileOffset = jumpDestination,
                ShiftECXReached = shiftReached,
                ShiftInputECX = shiftInput,
                ShiftResultECX = shiftResult,
                ShiftFlags = shiftFlags,
                MoveImmediateEAXReached = moveImmediateReached,
                MoveImmediateEAXResult = moveImmediateResult,
                SubtractReached = subtractReached,
                SubtractLeft = subtractReached ? NonEqualEAXImmediate : null,
                SubtractRight = subtractRight,
                SubtractResult = subtractResult,
                SubtractFlags = subtractFlags,
                FlagsAtStop = flagsAtStop,
                Reads = readAttempted && readAddress is uint address
                    ? ImmutableArray.Create(address)
                    : ImmutableArray<uint>.Empty,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEBX is not uint entryEBX)
        {
            return Stop(
                CapacityFinalCalleeCommonIndexedCountClassificationOutcome.MissingEntryEBX,
                FreshLoadRawFileOffset);
        }

        if (capture.FreshIndexedDWORD is not uint freshIndexedDWORD)
        {
            return Stop(
                CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingFreshIndexedDWORDAt93C0,
                FreshLoadRawFileOffset,
                readAttempted: true,
                readAddress: entryEBX);
        }

        if (capture.LateEntryECX is not uint lateEntryECX)
        {
            return Stop(
                CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingLateEntryECXAt93C2,
                CompareRawFileOffset,
                readAttempted: true,
                readCompleted: true,
                readAddress: entryEBX,
                freshEDX: freshIndexedDWORD);
        }

        uint compareResult = unchecked(lateEntryECX - CompareImmediate);
        CapacityFinalCalleeCommonIndexedCountClassificationFlags compareFlags =
            CalculateSubtractionFlags(lateEntryECX, CompareImmediate, compareResult);
        bool jnzTaken = lateEntryECX != CompareImmediate;

        if (!jnzTaken)
        {
            CapacityFinalCalleeCommonIndexedCountClassificationFlags xorFlags = new(
                Carry: false,
                Parity: HasEvenParity(0),
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false);
            return Stop(
                CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4,
                BoundaryRawFileOffsetExclusive,
                readAttempted: true,
                readCompleted: true,
                readAddress: entryEBX,
                freshEDX: freshIndexedDWORD,
                compareReached: true,
                compareLeft: lateEntryECX,
                compareResult: compareResult,
                compareFlags: compareFlags,
                jnzReached: true,
                jnzTaken: false,
                jnzDestination: XorRawFileOffset,
                xorReached: true,
                xorResult: 0,
                xorFlags: xorFlags,
                jumpReached: true,
                jumpDestination: BoundaryRawFileOffsetExclusive,
                flagsAtStop: xorFlags);
        }

        uint shiftedECX = lateEntryECX >> 1;
        CapacityFinalCalleeCommonIndexedCountClassificationFlags shiftFlags = new(
            Carry: (lateEntryECX & 1) != 0,
            Parity: HasEvenParity((byte)shiftedECX),
            Auxiliary: null,
            Zero: shiftedECX == 0,
            Sign: (shiftedECX & 0x80000000) != 0,
            Overflow: (lateEntryECX & 0x80000000) != 0);
        uint subtractResult = unchecked(NonEqualEAXImmediate - shiftedECX);
        CapacityFinalCalleeCommonIndexedCountClassificationFlags subtractFlags =
            CalculateSubtractionFlags(NonEqualEAXImmediate, shiftedECX, subtractResult);
        return Stop(
            CapacityFinalCalleeCommonIndexedCountClassificationOutcome.NonEqualAt93D4,
            BoundaryRawFileOffsetExclusive,
            readAttempted: true,
            readCompleted: true,
            readAddress: entryEBX,
            freshEDX: freshIndexedDWORD,
            compareReached: true,
            compareLeft: lateEntryECX,
            compareResult: compareResult,
            compareFlags: compareFlags,
            jnzReached: true,
            jnzTaken: true,
            jnzDestination: JnzBranchTargetRawFileOffset,
            shiftReached: true,
            shiftInput: lateEntryECX,
            shiftResult: shiftedECX,
            shiftFlags: shiftFlags,
            moveImmediateReached: true,
            moveImmediateResult: NonEqualEAXImmediate,
            subtractReached: true,
            subtractRight: shiftedECX,
            subtractResult: subtractResult,
            subtractFlags: subtractFlags,
            flagsAtStop: subtractFlags);
    }

    private static CapacityFinalCalleeCommonIndexedCountClassificationFlags
        CalculateSubtractionFlags(uint left, uint right, uint result) =>
            new(
                Carry: left < right,
                Parity: HasEvenParity((byte)result),
                Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
                Zero: result == 0,
                Sign: (result & 0x80000000) != 0,
                Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeCommonIndexedCountClassificationInstructionEvidence
        Instruction(
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

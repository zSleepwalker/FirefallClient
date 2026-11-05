using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonTraversalIterationCapture(
    uint? EntryEBX,
    uint? LateEntryEDX,
    uint? FreshChildDWORD,
    uint? LateFreshChildPlus4,
    uint? EntryEDI);

public enum CapacityFinalCalleeCommonTraversalIterationOutcome
{
    MissingEntryEBXAt93E4,
    MissingLateEntryEDXAt93EE,
    MissingFreshChildDWORDAt93F1,
    ZeroChildTo945C,
    MissingLateFreshChildPlus4At93F9,
    MissingEntryEDIAt93FF,
    MaskedChildEqualsEntryEDIAt9403,
    MaskedChildDiffersFromEntryEDITo93E4
}

public enum CapacityFinalCalleeCommonTraversalIterationEventKind
{
    MoveEBXToEAX,
    DoubleEBX,
    ShiftEAXRight31,
    AddFourToEAX,
    FormChildAddress,
    LoadFreshChildDWORD,
    TestChildDWORD,
    BranchIfChildZero,
    MoveChildToEDX,
    LoadFreshChildPlus4,
    AndEAXWithFFFFFFF8,
    CompareEAXWithEDI,
    BranchIfNotEqual
}

public readonly record struct CapacityFinalCalleeCommonTraversalIterationFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonTraversalIterationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeCommonTraversalIterationRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeCommonTraversalIterationEvent(
    CapacityFinalCalleeCommonTraversalIterationEventKind Kind,
    uint RawFileOffset,
    uint? Input = null,
    uint? SecondInput = null,
    uint? Result = null,
    uint? Address = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null,
    CapacityFinalCalleeCommonTraversalIterationFlags? Flags = null);

public sealed record CapacityFinalCalleeCommonTraversalIterationPlan(
    CapacityFinalCalleeCommonTraversalIterationCapture Capture,
    CapacityFinalCalleeCommonTraversalIterationOutcome Outcome)
{
    public bool EntryMoveReached { get; init; }
    public bool EntryEBXCaptured { get; init; }
    public uint? EAXAfterEntryMove { get; init; }
    public bool DoubleEBXReached { get; init; }
    public uint? EBXAfterDouble { get; init; }
    public CapacityFinalCalleeCommonTraversalIterationFlags? DoubleEBXFlags { get; init; }
    public bool ShiftEAXReached { get; init; }
    public uint? ShiftInputEAX { get; init; }
    public uint? ShiftResultEAX { get; init; }
    public CapacityFinalCalleeCommonTraversalIterationFlags? ShiftFlags { get; init; }
    public bool AddFourReached { get; init; }
    public uint? EAXAfterAddFour { get; init; }
    public CapacityFinalCalleeCommonTraversalIterationFlags? AddFourFlags { get; init; }
    public bool ChildAddressLeaReached { get; init; }
    public bool LateEntryEDXCaptured { get; init; }
    public uint? ChildFieldOffset { get; init; }
    public uint? ChildFieldAddress { get; init; }
    public bool FreshChildReadReached { get; init; }
    public bool FreshChildDWORDCaptured { get; init; }
    public uint? FreshChildDWORD { get; init; }
    public CapacityFinalCalleeCommonTraversalIterationFlags? ChildTestFlags { get; init; }
    public bool ChildZeroBranchReached { get; init; }
    public bool? ChildZeroBranchTaken { get; init; }
    public uint? ChildZeroBranchDestinationRawFileOffset { get; init; }
    public bool MoveChildToEDXReached { get; init; }
    public uint? EDXAfterChildMove { get; init; }
    public uint? FreshChildPlus4Address { get; init; }
    public bool FreshChildPlus4ReadReached { get; init; }
    public bool FreshChildPlus4Captured { get; init; }
    public uint? FreshChildPlus4 { get; init; }
    public bool AndEAXReached { get; init; }
    public uint? AndInputEAX { get; init; }
    public uint? MaskedChildDWORD { get; init; }
    public CapacityFinalCalleeCommonTraversalIterationFlags? AndFlags { get; init; }
    public bool CompareReached { get; init; }
    public bool EntryEDICaptured { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeCommonTraversalIterationFlags? CompareFlags { get; init; }
    public bool LoopBranchReached { get; init; }
    public bool? LoopBranchTaken { get; init; }
    public uint? LoopBranchSelectedDestinationRawFileOffset { get; init; }
    public CapacityFinalCalleeCommonTraversalIterationFlags? FlagsAtStop { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonTraversalIterationEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonTraversalIterationEvent>.Empty;
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets =>
        Events.Select(item => item.RawFileOffset).ToImmutableArray();
    public ImmutableArray<CapacityFinalCalleeCommonTraversalIterationRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonTraversalIterationRead>.Empty;
    public bool UnreachedLateEntryEDXCaptureProvided { get; init; }
    public bool UnreachedFreshChildDWORDCaptureProvided { get; init; }
    public bool UnreachedLateFreshChildPlus4CaptureProvided { get; init; }
    public bool UnreachedEntryEDICaptureProvided { get; init; }
    public bool ExactSingleIterationEvidence { get; init; }
    public bool TargetEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects exactly one bounded traversal iteration at raw 012C93E4 through
/// the child test and masked-field comparison, without entering any selected edge.
/// </summary>
public static class CapacityFinalCalleeCommonTraversalIterationProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C93E4;
    public const uint EntryVirtualAddress = 0x016C9FE4;
    public const uint MoveEBXToEAXRawFileOffset = EntryRawFileOffset;
    public const uint DoubleEBXRawFileOffset = 0x012C93E6;
    public const uint ShiftEAXRawFileOffset = 0x012C93E8;
    public const uint AddFourRawFileOffset = 0x012C93EB;
    public const uint ChildAddressLeaRawFileOffset = 0x012C93EE;
    public const uint FreshChildReadRawFileOffset = 0x012C93F1;
    public const uint ChildTestRawFileOffset = 0x012C93F3;
    public const uint ChildZeroBranchRawFileOffset = 0x012C93F5;
    public const uint MoveChildToEDXRawFileOffset = 0x012C93F7;
    public const uint FreshChildPlus4ReadRawFileOffset = 0x012C93F9;
    public const uint AndEAXRawFileOffset = 0x012C93FC;
    public const uint CompareRawFileOffset = 0x012C93FF;
    public const uint LoopBranchRawFileOffset = 0x012C9401;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9403;
    public const uint ZeroChildBranchTargetRawFileOffset = 0x012C945C;
    public const uint ReturnImmediatelyBeforeZeroChildTargetRawFileOffset = 0x012C945B;
    public const uint LoopBranchTargetRawFileOffset = EntryRawFileOffset;
    public const uint EqualFallthroughRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const uint ShiftImmediate = 31;
    public const uint AddImmediate = 4;
    public const uint ChildFieldScale = 4;
    public const uint FreshChildPlus4Displacement = 4;
    public const uint EAXMask = 0xFFFFFFF8;

    public const string CapturedWindowBytes =
        "8B C3 03 DB C1 E8 1F 83 C0 04 8D 0C 82 8B 01 85 C0 74 65 "
        + "8B D0 8B 42 04 83 E0 F8 3B C7 75 E1";

    public static ImmutableArray<CapacityFinalCalleeCommonTraversalIterationInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Move entry EBX to EAX", MoveEBXToEAXRawFileOffset, "8B C3"),
            Instruction("Double EBX with DWORD wraparound", DoubleEBXRawFileOffset, "03 DB"),
            Instruction("Shift the copied EAX right by 31", ShiftEAXRawFileOffset, "C1 E8 1F"),
            Instruction("Add four to the selected field word index", AddFourRawFileOffset, "83 C0 04"),
            Instruction("Form EDX plus EAX times four without reading memory",
                ChildAddressLeaRawFileOffset, "8D 0C 82"),
            Instruction("Load the fresh DWORD at [ECX] into EAX",
                FreshChildReadRawFileOffset, "8B 01"),
            Instruction("Test the fresh child DWORD", ChildTestRawFileOffset, "85 C0"),
            Instruction("Branch to 945C when the fresh child DWORD is zero",
                ChildZeroBranchRawFileOffset, "74 65",
                branchTargetRawFileOffset: ZeroChildBranchTargetRawFileOffset),
            Instruction("Move the nonzero child DWORD from EAX to EDX",
                MoveChildToEDXRawFileOffset, "8B D0"),
            Instruction("Read a fresh DWORD at child EDX plus four",
                FreshChildPlus4ReadRawFileOffset, "8B 42 04"),
            Instruction("Mask EAX with FFFFFFF8h", AndEAXRawFileOffset, "83 E0 F8"),
            Instruction("Compare masked EAX with entry EDI", CompareRawFileOffset, "3B C7"),
            Instruction("Branch back to 93E4 when the comparison is not equal",
                LoopBranchRawFileOffset, "75 E1",
                branchTargetRawFileOffset: LoopBranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveChildFieldOffset(uint entryEBX) =>
        unchecked(((entryEBX >> (int)ShiftImmediate) + AddImmediate) * ChildFieldScale);

    public static uint DeriveChildFieldAddress(uint entryEDX, uint entryEBX) =>
        unchecked(entryEDX + DeriveChildFieldOffset(entryEBX));

    public static uint DeriveFreshChildPlus4Address(uint freshChildDWORD) =>
        unchecked(freshChildDWORD + FreshChildPlus4Displacement);

    public static CapacityFinalCalleeCommonTraversalIterationPlan Project(
        CapacityFinalCalleeCommonTraversalIterationCapture capture)
    {
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonTraversalIterationEvent>();
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonTraversalIterationRead>();
        bool entryMoveReached = false;
        bool entryEBXCaptured = false;
        uint? eaxAfterEntryMove = null;
        bool doubleEBXReached = false;
        uint? ebxAfterDouble = null;
        CapacityFinalCalleeCommonTraversalIterationFlags? doubleEBXFlags = null;
        bool shiftReached = false;
        uint? shiftInput = null;
        uint? shiftResult = null;
        CapacityFinalCalleeCommonTraversalIterationFlags? shiftFlags = null;
        bool addFourReached = false;
        uint? eaxAfterAddFour = null;
        CapacityFinalCalleeCommonTraversalIterationFlags? addFourFlags = null;
        bool childAddressLeaReached = false;
        bool lateEntryEDXCaptured = false;
        uint? childFieldOffset = null;
        uint? childFieldAddress = null;
        bool freshChildReadReached = false;
        bool freshChildCaptured = false;
        uint? freshChild = null;
        CapacityFinalCalleeCommonTraversalIterationFlags? childTestFlags = null;
        bool childZeroBranchReached = false;
        bool? childZeroBranchTaken = null;
        uint? childZeroDestination = null;
        bool moveChildReached = false;
        uint? edxAfterChildMove = null;
        uint? freshChildPlus4Address = null;
        bool freshChildPlus4ReadReached = false;
        bool freshChildPlus4Captured = false;
        uint? freshChildPlus4 = null;
        bool andReached = false;
        uint? andInput = null;
        uint? maskedChild = null;
        CapacityFinalCalleeCommonTraversalIterationFlags? andFlags = null;
        bool compareReached = false;
        bool entryEDICaptured = false;
        uint? compareResult = null;
        CapacityFinalCalleeCommonTraversalIterationFlags? compareFlags = null;
        bool loopBranchReached = false;
        bool? loopBranchTaken = null;
        uint? loopBranchDestination = null;
        CapacityFinalCalleeCommonTraversalIterationFlags? flags = null;

        CapacityFinalCalleeCommonTraversalIterationPlan Finish(
            CapacityFinalCalleeCommonTraversalIterationOutcome outcome,
            uint stopAt,
            bool exactSingleIterationEvidence = false) =>
            new(capture, outcome)
            {
                EntryMoveReached = entryMoveReached,
                EntryEBXCaptured = entryEBXCaptured,
                EAXAfterEntryMove = eaxAfterEntryMove,
                DoubleEBXReached = doubleEBXReached,
                EBXAfterDouble = ebxAfterDouble,
                DoubleEBXFlags = doubleEBXFlags,
                ShiftEAXReached = shiftReached,
                ShiftInputEAX = shiftInput,
                ShiftResultEAX = shiftResult,
                ShiftFlags = shiftFlags,
                AddFourReached = addFourReached,
                EAXAfterAddFour = eaxAfterAddFour,
                AddFourFlags = addFourFlags,
                ChildAddressLeaReached = childAddressLeaReached,
                LateEntryEDXCaptured = lateEntryEDXCaptured,
                ChildFieldOffset = childFieldOffset,
                ChildFieldAddress = childFieldAddress,
                FreshChildReadReached = freshChildReadReached,
                FreshChildDWORDCaptured = freshChildCaptured,
                FreshChildDWORD = freshChild,
                ChildTestFlags = childTestFlags,
                ChildZeroBranchReached = childZeroBranchReached,
                ChildZeroBranchTaken = childZeroBranchTaken,
                ChildZeroBranchDestinationRawFileOffset = childZeroDestination,
                MoveChildToEDXReached = moveChildReached,
                EDXAfterChildMove = edxAfterChildMove,
                FreshChildPlus4Address = freshChildPlus4Address,
                FreshChildPlus4ReadReached = freshChildPlus4ReadReached,
                FreshChildPlus4Captured = freshChildPlus4Captured,
                FreshChildPlus4 = freshChildPlus4,
                AndEAXReached = andReached,
                AndInputEAX = andInput,
                MaskedChildDWORD = maskedChild,
                AndFlags = andFlags,
                CompareReached = compareReached,
                EntryEDICaptured = entryEDICaptured,
                CompareLeft = compareReached ? maskedChild : null,
                CompareRight = compareReached ? capture.EntryEDI : null,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                LoopBranchReached = loopBranchReached,
                LoopBranchTaken = loopBranchTaken,
                LoopBranchSelectedDestinationRawFileOffset = loopBranchDestination,
                FlagsAtStop = flags,
                Events = events.ToImmutable(),
                Reads = reads.ToImmutable(),
                UnreachedLateEntryEDXCaptureProvided =
                    !lateEntryEDXCaptured && capture.LateEntryEDX.HasValue,
                UnreachedFreshChildDWORDCaptureProvided =
                    !freshChildReadReached && capture.FreshChildDWORD.HasValue,
                UnreachedLateFreshChildPlus4CaptureProvided =
                    !freshChildPlus4ReadReached && capture.LateFreshChildPlus4.HasValue,
                UnreachedEntryEDICaptureProvided =
                    !compareReached && capture.EntryEDI.HasValue,
                ExactSingleIterationEvidence = exactSingleIterationEvidence,
                StopAtRawFileOffsetExclusive = stopAt
            };

        entryMoveReached = true;
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.MoveEBXToEAX,
            MoveEBXToEAXRawFileOffset,
            Input: capture.EntryEBX,
            Result: capture.EntryEBX));
        if (capture.EntryEBX is not uint entryEBX)
        {
            return Finish(
                CapacityFinalCalleeCommonTraversalIterationOutcome.MissingEntryEBXAt93E4,
                MoveEBXToEAXRawFileOffset);
        }

        entryEBXCaptured = true;
        eaxAfterEntryMove = entryEBX;

        doubleEBXReached = true;
        uint doubledEBX = unchecked(entryEBX + entryEBX);
        doubleEBXFlags = CalculateAddFlags(entryEBX, entryEBX, doubledEBX);
        flags = doubleEBXFlags;
        ebxAfterDouble = doubledEBX;
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.DoubleEBX,
            DoubleEBXRawFileOffset,
            Input: entryEBX,
            SecondInput: entryEBX,
            Result: doubledEBX,
            Flags: doubleEBXFlags));

        shiftReached = true;
        shiftInput = eaxAfterEntryMove;
        uint shiftedEAX = entryEBX >> (int)ShiftImmediate;
        shiftResult = shiftedEAX;
        shiftFlags = CalculateShiftRight31Flags(entryEBX, shiftedEAX);
        flags = shiftFlags;
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.ShiftEAXRight31,
            ShiftEAXRawFileOffset,
            Input: entryEBX,
            SecondInput: ShiftImmediate,
            Result: shiftedEAX,
            Flags: shiftFlags));

        addFourReached = true;
        uint afterAddFour = unchecked(shiftedEAX + AddImmediate);
        eaxAfterAddFour = afterAddFour;
        addFourFlags = CalculateAddFlags(shiftedEAX, AddImmediate, afterAddFour);
        flags = addFourFlags;
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.AddFourToEAX,
            AddFourRawFileOffset,
            Input: shiftedEAX,
            SecondInput: AddImmediate,
            Result: afterAddFour,
            Flags: addFourFlags));

        childAddressLeaReached = true;
        childFieldOffset = unchecked(afterAddFour * ChildFieldScale);
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.FormChildAddress,
            ChildAddressLeaRawFileOffset,
            Input: capture.LateEntryEDX,
            SecondInput: afterAddFour,
            Result: capture.LateEntryEDX is uint entryEDX
                ? unchecked(entryEDX + (afterAddFour * ChildFieldScale))
                : null,
            Flags: flags));
        if (capture.LateEntryEDX is not uint capturedEntryEDX)
        {
            return Finish(
                CapacityFinalCalleeCommonTraversalIterationOutcome.MissingLateEntryEDXAt93EE,
                    ChildAddressLeaRawFileOffset);
        }

        lateEntryEDXCaptured = true;
        childFieldAddress = unchecked(capturedEntryEDX + childFieldOffset.Value);

        freshChildReadReached = true;
        reads.Add(new(
            FreshChildReadRawFileOffset,
            childFieldAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshChildDWORD.HasValue,
            Value: capture.FreshChildDWORD));
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.LoadFreshChildDWORD,
            FreshChildReadRawFileOffset,
            Result: capture.FreshChildDWORD,
            Address: childFieldAddress,
            Flags: flags));
        if (capture.FreshChildDWORD is not uint capturedChild)
        {
            return Finish(
                CapacityFinalCalleeCommonTraversalIterationOutcome.MissingFreshChildDWORDAt93F1,
                FreshChildReadRawFileOffset);
        }

        freshChildCaptured = true;
        freshChild = capturedChild;

        childTestFlags = CalculateTestFlags(capturedChild);
        flags = childTestFlags;
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.TestChildDWORD,
            ChildTestRawFileOffset,
            Input: capturedChild,
            Result: capturedChild,
            Flags: childTestFlags));

        childZeroBranchReached = true;
        childZeroBranchTaken = capturedChild == 0;
        childZeroDestination = childZeroBranchTaken.Value
            ? ZeroChildBranchTargetRawFileOffset
            : MoveChildToEDXRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.BranchIfChildZero,
            ChildZeroBranchRawFileOffset,
            BranchTaken: childZeroBranchTaken,
            SelectedDestinationRawFileOffset: childZeroDestination,
            Flags: childTestFlags));
        if (childZeroBranchTaken.Value)
        {
            return Finish(
                CapacityFinalCalleeCommonTraversalIterationOutcome.ZeroChildTo945C,
                ZeroChildBranchTargetRawFileOffset,
                exactSingleIterationEvidence:
                    !capture.LateFreshChildPlus4.HasValue && !capture.EntryEDI.HasValue);
        }

        moveChildReached = true;
        edxAfterChildMove = capturedChild;
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.MoveChildToEDX,
            MoveChildToEDXRawFileOffset,
            Input: capturedChild,
            Result: capturedChild,
            Flags: flags));

        freshChildPlus4Address = DeriveFreshChildPlus4Address(capturedChild);
        freshChildPlus4ReadReached = true;
        reads.Add(new(
            FreshChildPlus4ReadRawFileOffset,
            freshChildPlus4Address,
            AddressCaptured: true,
            ValueCaptured: capture.LateFreshChildPlus4.HasValue,
            Value: capture.LateFreshChildPlus4));
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.LoadFreshChildPlus4,
            FreshChildPlus4ReadRawFileOffset,
            Input: capturedChild,
            Result: capture.LateFreshChildPlus4,
            Address: freshChildPlus4Address,
            Flags: flags));
        if (capture.LateFreshChildPlus4 is not uint capturedChildPlus4)
        {
            return Finish(
                CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MissingLateFreshChildPlus4At93F9,
                FreshChildPlus4ReadRawFileOffset);
        }

        freshChildPlus4Captured = true;
        freshChildPlus4 = capturedChildPlus4;

        andReached = true;
        andInput = capturedChildPlus4;
        maskedChild = capturedChildPlus4 & EAXMask;
        andFlags = CalculateAndFlags(maskedChild.Value);
        flags = andFlags;
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.AndEAXWithFFFFFFF8,
            AndEAXRawFileOffset,
            Input: capturedChildPlus4,
            SecondInput: EAXMask,
            Result: maskedChild,
            Flags: andFlags));

        compareReached = true;
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.CompareEAXWithEDI,
            CompareRawFileOffset,
            Input: maskedChild,
            SecondInput: capture.EntryEDI,
            Flags: flags));
        if (capture.EntryEDI is not uint entryEDI)
        {
            return Finish(
                CapacityFinalCalleeCommonTraversalIterationOutcome.MissingEntryEDIAt93FF,
                CompareRawFileOffset);
        }

        entryEDICaptured = true;
        uint subtraction = unchecked(maskedChild.Value - entryEDI);
        compareResult = subtraction;
        compareFlags = CalculateSubtractFlags(maskedChild.Value, entryEDI, subtraction);
        flags = compareFlags;

        loopBranchReached = true;
        loopBranchTaken = subtraction != 0;
        loopBranchDestination = loopBranchTaken.Value
            ? LoopBranchTargetRawFileOffset
            : EqualFallthroughRawFileOffset;
        events[events.Count - 1] = events[events.Count - 1] with
        {
            Result = subtraction,
            Flags = compareFlags
        };
        events.Add(new(
            CapacityFinalCalleeCommonTraversalIterationEventKind.BranchIfNotEqual,
            LoopBranchRawFileOffset,
            BranchTaken: loopBranchTaken,
            SelectedDestinationRawFileOffset: loopBranchDestination,
            Flags: compareFlags));

        return Finish(
            loopBranchTaken.Value
                ? CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildDiffersFromEntryEDITo93E4
                : CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildEqualsEntryEDIAt9403,
            loopBranchDestination.Value,
            exactSingleIterationEvidence: true);
    }

    private static CapacityFinalCalleeCommonTraversalIterationFlags CalculateAddFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: (ulong)left + right > uint.MaxValue,
            Parity: HasEvenParity((byte)result),
            Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: (~(left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static CapacityFinalCalleeCommonTraversalIterationFlags CalculateShiftRight31Flags(
        uint input,
        uint result) =>
        new(
            Carry: ((input >> 30) & 1) != 0,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: null);

    private static CapacityFinalCalleeCommonTraversalIterationFlags CalculateTestFlags(uint value) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)value),
            Auxiliary: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static CapacityFinalCalleeCommonTraversalIterationFlags CalculateAndFlags(uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: false);

    private static CapacityFinalCalleeCommonTraversalIterationFlags CalculateSubtractFlags(
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

    private static CapacityFinalCalleeCommonTraversalIterationInstructionEvidence Instruction(
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

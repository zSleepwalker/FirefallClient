using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlignmentDispatchCapture(
    bool CaptureValidatedEntry,
    uint EntryEAX,
    uint EntryESI,
    uint EntryECX,
    uint EntryEBP,
    uint EntryESP,
    uint? EntryEsiDword,
    uint? GlobalDispatchDword,
    uint? CallCellTargetVirtualAddress,
    bool? CallReturnedNormally);

public enum CapacityFinalCalleeAlignmentDispatchOutcome
{
    EntryNotValidated,
    MissingEntryEsiDword,
    EntryAlLowBitSet,
    EntryEcxNonzero,
    ComputedEcxZero,
    MissingGlobalDispatchDword,
    MissingCallCellTarget,
    MissingCallReturnStatus,
    CallDidNotReturnNormally,
    CallReturnedNormally
}

public enum CapacityFinalCalleeAlignmentDispatchReadKind
{
    EntryEsiDword,
    GlobalDispatchDword,
    CallCellTargetVirtualAddress
}

public readonly record struct CapacityFinalCalleeAlignmentDispatchReadDescriptor(
    uint RawFileOffset,
    CapacityFinalCalleeAlignmentDispatchReadKind Kind,
    uint? PeVirtualAddress,
    string? BaseRegister,
    uint Displacement);

public readonly record struct CapacityFinalCalleeAlignmentDispatchRead(
    uint RawFileOffset,
    CapacityFinalCalleeAlignmentDispatchReadKind Kind,
    uint Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeAlignmentDispatchWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeAlignmentDispatchPush(
    uint RawFileOffset,
    uint StackAddress,
    uint Value);

public readonly record struct CapacityFinalCalleeAlignmentDispatchFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeAlignmentDispatchInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    byte? ImmediateValue = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeAlignmentDispatchPlan(
    CapacityFinalCalleeAlignmentDispatchCapture Capture,
    CapacityFinalCalleeAlignmentDispatchOutcome Outcome,
    ImmutableArray<CapacityFinalCalleeAlignmentDispatchRead> Reads,
    ImmutableArray<CapacityFinalCalleeAlignmentDispatchWrite> FrameWrites,
    ImmutableArray<CapacityFinalCalleeAlignmentDispatchPush> Pushes,
    uint? EAX,
    uint? ESI,
    uint? ECX,
    uint? EDI,
    uint? EDX,
    uint? EBX,
    uint? ESP,
    bool TestAlReached,
    CapacityFinalCalleeAlignmentDispatchFlags? TestAlFlags,
    bool TestAlBranchReached,
    bool? TestAlBranchTaken,
    uint? TestAlSelectedDestinationRawFileOffset,
    uint? TestAlSelectedDestinationVirtualAddress,
    bool EntryEsiReadReached,
    bool EntryEsiDwordCaptured,
    uint? EntryEsiDword,
    bool TestEntryEcxReached,
    CapacityFinalCalleeAlignmentDispatchFlags? TestEntryEcxFlags,
    bool TestEntryEcxBranchReached,
    bool? TestEntryEcxBranchTaken,
    uint? TestEntryEcxSelectedDestinationRawFileOffset,
    uint? TestEntryEcxSelectedDestinationVirtualAddress,
    bool TestComputedEcxReached,
    CapacityFinalCalleeAlignmentDispatchFlags? TestComputedEcxFlags,
    bool TestComputedEcxBranchReached,
    bool? TestComputedEcxBranchTaken,
    uint? TestComputedEcxSelectedDestinationRawFileOffset,
    uint? TestComputedEcxSelectedDestinationVirtualAddress,
    bool GlobalDispatchReadReached,
    bool GlobalDispatchDwordCaptured,
    uint? GlobalDispatchDword,
    bool CallCellReadReached,
    bool CallCellTargetCaptured,
    uint? CallCellTargetVirtualAddress,
    bool CallReached,
    bool? CallReturnedNormally,
    bool CallRegisterOutputsUnknown,
    bool UnreachedEntryEsiDwordCaptureProvided,
    bool UnreachedGlobalDispatchDwordCaptureProvided,
    bool UnreachedCallCellTargetCaptureProvided,
    bool UnreachedCallReturnStatusProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the bounded alignment/dispatch window from independent entry registers.
/// The indirect call and its effects remain opaque; register fields are pre-call values
/// once the call is reached, and only a captured normal-return gate permits reporting
/// that this window reached its exclusive boundary.
/// </summary>
public static class CapacityFinalCalleeAlignmentDispatchProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8DA7;
    public const uint EntryVirtualAddress = 0x016C99A7;
    public const uint EntryAlTestRawFileOffset = 0x012C8DB9;
    public const uint EntryAlBranchRawFileOffset = 0x012C8DBB;
    public const uint EntryEsiReadRawFileOffset = 0x012C8DC1;
    public const uint EntryEcxTestRawFileOffset = 0x012C8DC5;
    public const uint EntryEcxBranchRawFileOffset = 0x012C8DC7;
    public const uint ComputedEcxTestRawFileOffset = 0x012C8DD3;
    public const uint ComputedEcxBranchRawFileOffset = 0x012C8DD5;
    public const uint GlobalDispatchReadRawFileOffset = 0x012C8DD7;
    public const uint FirstPushRawFileOffset = 0x012C8DE0;
    public const uint SecondPushRawFileOffset = 0x012C8DE5;
    public const uint ThirdPushRawFileOffset = 0x012C8DE6;
    public const uint CallRawFileOffset = 0x012C8DE7;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8DED;
    public const uint BoundaryVirtualAddressExclusive = 0x016C99ED;

    public const uint EntryAlBranchDestinationRawFileOffset = 0x012C8EF8;
    public const uint EntryAlBranchDestinationVirtualAddress = 0x016C9AF8;
    public const uint EntryEcxBranchDestinationRawFileOffset = 0x012C8E45;
    public const uint EntryEcxBranchDestinationVirtualAddress = 0x016C9A45;
    public const uint ComputedEcxZeroBranchDestinationRawFileOffset = 0x012C8E3A;
    public const uint ComputedEcxZeroBranchDestinationVirtualAddress = 0x016C9A3A;

    public const uint GlobalDispatchCellVirtualAddress = 0x01AC42B4;
    public const uint CallCellVirtualAddress = 0x01AC42A8;
    public const uint EntryEsiDwordDisplacement = 0;
    public const uint FrameLocalMinusEightDisplacement = 8;
    public const uint FrameLocalMinusTwelveDisplacement = 12;
    public const uint FrameLocalMinusSixteenDisplacement = 16;
    public const uint FrameLocalMinusFortyEightDisplacement = 48;
    public const uint StackArgumentSize = 0x1C;
    public const uint ComputedEcxOffset = 0x10;

    public const string CapturedWindowBytes =
        "8B F8 C7 45 F8 01 00 00 00 83 E7 F8 8D 14 37 89 55 F4 A8 01 " +
        "0F 85 37 01 00 00 8B 06 2B F0 85 C9 75 7C 8D 48 10 03 CF 89 4D F0 " +
        "8B F9 85 C9 74 63 8B 1D B4 42 AC 01 8D 49 00 6A 1C 8D 45 D0 50 56 " +
        "FF 15 A8 42 AC 01";

    public static ImmutableArray<CapacityFinalCalleeAlignmentDispatchReadDescriptor>
        OrderedReadDescriptors { get; } =
        ImmutableArray.Create(
            new CapacityFinalCalleeAlignmentDispatchReadDescriptor(
                EntryEsiReadRawFileOffset,
                CapacityFinalCalleeAlignmentDispatchReadKind.EntryEsiDword,
                null,
                "ESI",
                EntryEsiDwordDisplacement),
            new CapacityFinalCalleeAlignmentDispatchReadDescriptor(
                GlobalDispatchReadRawFileOffset,
                CapacityFinalCalleeAlignmentDispatchReadKind.GlobalDispatchDword,
                GlobalDispatchCellVirtualAddress,
                null,
                0),
            new CapacityFinalCalleeAlignmentDispatchReadDescriptor(
                CallRawFileOffset,
                CapacityFinalCalleeAlignmentDispatchReadKind.CallCellTargetVirtualAddress,
                CallCellVirtualAddress,
                null,
                0));

    public static ImmutableArray<CapacityFinalCalleeAlignmentDispatchInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Copy entry EAX to EDI",
                EntryRawFileOffset, "8B F8"),
            Instruction("Store 1 at [EBP-8]",
                0x012C8DA9, "C7 45 F8 01 00 00 00", immediateValue: 1),
            Instruction("Align EDI down to an eight-byte boundary",
                0x012C8DB0, "83 E7 F8", immediateValue: 0xF8),
            Instruction("Compute EDX = EDI + entry ESI",
                0x012C8DB3, "8D 14 37"),
            Instruction("Store EDX at [EBP-C]",
                0x012C8DB6, "89 55 F4"),
            Instruction("TEST entry AL with 1",
                EntryAlTestRawFileOffset, "A8 01", immediateValue: 1),
            Instruction("JNZ to 12C8EF8 when entry AL bit 0 is set",
                EntryAlBranchRawFileOffset, "0F 85 37 01 00 00",
                branchTargetRawFileOffset: EntryAlBranchDestinationRawFileOffset,
                branchTargetVirtualAddress: EntryAlBranchDestinationVirtualAddress),
            Instruction("Read the dword at [entry ESI] into EAX on fallthrough",
                EntryEsiReadRawFileOffset, "8B 06"),
            Instruction("Subtract loaded EAX from ESI",
                0x012C8DC3, "2B F0"),
            Instruction("TEST entry ECX with itself",
                EntryEcxTestRawFileOffset, "85 C9"),
            Instruction("JNZ to 12C8E45 when entry ECX is nonzero",
                EntryEcxBranchRawFileOffset, "75 7C",
                branchTargetRawFileOffset: EntryEcxBranchDestinationRawFileOffset,
                branchTargetVirtualAddress: EntryEcxBranchDestinationVirtualAddress),
            Instruction("Compute ECX = EAX + 10h",
                0x012C8DC9, "8D 48 10"),
            Instruction("Add EDI to ECX",
                0x012C8DCC, "03 CF"),
            Instruction("Store ECX at [EBP-10]",
                0x012C8DCE, "89 4D F0"),
            Instruction("Copy ECX to EDI",
                0x012C8DD1, "8B F9"),
            Instruction("TEST computed ECX with itself",
                ComputedEcxTestRawFileOffset, "85 C9"),
            Instruction("JZ to 12C8E3A when computed ECX is zero",
                ComputedEcxBranchRawFileOffset, "74 63",
                branchTargetRawFileOffset: ComputedEcxZeroBranchDestinationRawFileOffset,
                branchTargetVirtualAddress: ComputedEcxZeroBranchDestinationVirtualAddress),
            Instruction("Read PE VA 1AC42B4 into EBX",
                GlobalDispatchReadRawFileOffset, "8B 1D B4 42 AC 01",
                peVirtualAddressOperand: GlobalDispatchCellVirtualAddress),
            Instruction("LEA ECX,[ECX+0] leaves ECX unchanged",
                0x012C8DDD, "8D 49 00"),
            Instruction("Push immediate 1Ch",
                FirstPushRawFileOffset, "6A 1C", immediateValue: 0x1C),
            Instruction("Set EAX = EBP-30h",
                0x012C8DE2, "8D 45 D0"),
            Instruction("Push EAX",
                SecondPushRawFileOffset, "50"),
            Instruction("Push ESI",
                ThirdPushRawFileOffset, "56"),
            Instruction("Call through the cell at PE VA 1AC42A8",
                CallRawFileOffset, "FF 15 A8 42 AC 01",
                peVirtualAddressOperand: CallCellVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint EntryEsiAddress(uint entryEsi) =>
        unchecked(entryEsi + EntryEsiDwordDisplacement);

    public static uint FrameLocalAddress(uint entryEbp, uint displacement) =>
        unchecked(entryEbp - displacement);

    public static CapacityFinalCalleeAlignmentDispatchPlan Project(
        CapacityFinalCalleeAlignmentDispatchCapture capture)
    {
        var state = new ProjectionState(capture);
        if (!capture.CaptureValidatedEntry)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlignmentDispatchOutcome.EntryNotValidated,
                state,
                unreachedEntryEsiDwordCaptureProvided: capture.EntryEsiDword.HasValue,
                unreachedGlobalDispatchDwordCaptureProvided:
                    capture.GlobalDispatchDword.HasValue,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided: capture.CallReturnedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        state.EDI = capture.EntryEAX;
        state.FrameWrites.Add(new(
            0x012C8DA9,
            FrameLocalAddress(capture.EntryEBP, FrameLocalMinusEightDisplacement),
            1));
        state.EDI = state.EDI.Value & 0xFFFFFFF8;
        state.EDX = unchecked(state.EDI.Value + capture.EntryESI);
        state.FrameWrites.Add(new(
            0x012C8DB6,
            FrameLocalAddress(capture.EntryEBP, FrameLocalMinusTwelveDisplacement),
            state.EDX.Value));
        state.TestAlReached = true;
        state.TestAlFlags = AndFlags((byte)capture.EntryEAX, 1);
        state.TestAlBranchReached = true;
        bool entryAlLowBitSet = state.TestAlFlags.Value.Zero == false;
        if (entryAlLowBitSet)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlignmentDispatchOutcome.EntryAlLowBitSet,
                state,
                testAlBranchTaken: true,
                testAlSelectedDestinationRawFileOffset:
                    EntryAlBranchDestinationRawFileOffset,
                testAlSelectedDestinationVirtualAddress:
                    EntryAlBranchDestinationVirtualAddress,
                unreachedEntryEsiDwordCaptureProvided: capture.EntryEsiDword.HasValue,
                unreachedGlobalDispatchDwordCaptureProvided:
                    capture.GlobalDispatchDword.HasValue,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided: capture.CallReturnedNormally.HasValue,
                stopAt: EntryAlBranchDestinationRawFileOffset);
        }

        uint entryEsiAddress = EntryEsiAddress(capture.EntryESI);
        state.Reads.Add(new(
            EntryEsiReadRawFileOffset,
            CapacityFinalCalleeAlignmentDispatchReadKind.EntryEsiDword,
            entryEsiAddress,
            capture.EntryEsiDword.HasValue,
            capture.EntryEsiDword));
        if (capture.EntryEsiDword is not uint entryEsiDword)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlignmentDispatchOutcome.MissingEntryEsiDword,
                state,
                testAlBranchTaken: false,
                entryEsiReadReached: true,
                unreachedGlobalDispatchDwordCaptureProvided:
                    capture.GlobalDispatchDword.HasValue,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided: capture.CallReturnedNormally.HasValue,
                stopAt: EntryEsiReadRawFileOffset);
        }

        state.EAX = entryEsiDword;
        state.ESI = unchecked(capture.EntryESI - entryEsiDword);
        state.TestEntryEcxReached = true;
        state.TestEntryEcxFlags = TestFlags(capture.EntryECX);
        state.TestEntryEcxBranchReached = true;
        bool entryEcxNonzero = !state.TestEntryEcxFlags.Value.Zero;
        if (entryEcxNonzero)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlignmentDispatchOutcome.EntryEcxNonzero,
                state,
                testAlBranchTaken: false,
                entryEsiReadReached: true,
                entryEsiDwordCaptured: true,
                entryEsiDword: entryEsiDword,
                testEntryEcxBranchTaken: true,
                testEntryEcxSelectedDestinationRawFileOffset:
                    EntryEcxBranchDestinationRawFileOffset,
                testEntryEcxSelectedDestinationVirtualAddress:
                    EntryEcxBranchDestinationVirtualAddress,
                unreachedGlobalDispatchDwordCaptureProvided:
                    capture.GlobalDispatchDword.HasValue,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided: capture.CallReturnedNormally.HasValue,
                stopAt: EntryEcxBranchDestinationRawFileOffset);
        }

        state.ECX = unchecked(entryEsiDword + ComputedEcxOffset);
        state.ECX = unchecked(state.ECX.Value + state.EDI!.Value);
        state.FrameWrites.Add(new(
            0x012C8DCE,
            FrameLocalAddress(capture.EntryEBP, FrameLocalMinusSixteenDisplacement),
            state.ECX.Value));
        state.EDI = state.ECX;
        state.TestComputedEcxReached = true;
        state.TestComputedEcxFlags = TestFlags(state.ECX.Value);
        state.TestComputedEcxBranchReached = true;
        bool computedEcxIsZero = state.TestComputedEcxFlags.Value.Zero;
        if (computedEcxIsZero)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlignmentDispatchOutcome.ComputedEcxZero,
                state,
                testAlBranchTaken: false,
                entryEsiReadReached: true,
                entryEsiDwordCaptured: true,
                entryEsiDword: entryEsiDword,
                testEntryEcxBranchTaken: false,
                testComputedEcxBranchTaken: true,
                testComputedEcxSelectedDestinationRawFileOffset:
                    ComputedEcxZeroBranchDestinationRawFileOffset,
                testComputedEcxSelectedDestinationVirtualAddress:
                    ComputedEcxZeroBranchDestinationVirtualAddress,
                unreachedGlobalDispatchDwordCaptureProvided:
                    capture.GlobalDispatchDword.HasValue,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided: capture.CallReturnedNormally.HasValue,
                stopAt: ComputedEcxZeroBranchDestinationRawFileOffset);
        }

        state.Reads.Add(new(
            GlobalDispatchReadRawFileOffset,
            CapacityFinalCalleeAlignmentDispatchReadKind.GlobalDispatchDword,
            GlobalDispatchCellVirtualAddress,
            capture.GlobalDispatchDword.HasValue,
            capture.GlobalDispatchDword));
        if (capture.GlobalDispatchDword is not uint globalDispatchDword)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlignmentDispatchOutcome.MissingGlobalDispatchDword,
                state,
                testAlBranchTaken: false,
                entryEsiReadReached: true,
                entryEsiDwordCaptured: true,
                entryEsiDword: entryEsiDword,
                testEntryEcxBranchTaken: false,
                testComputedEcxBranchTaken: false,
                globalDispatchReadReached: true,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided: capture.CallReturnedNormally.HasValue,
                stopAt: GlobalDispatchReadRawFileOffset);
        }

        state.EBX = globalDispatchDword;
        Push(state, FirstPushRawFileOffset, StackArgumentSize);
        state.EAX = FrameLocalAddress(capture.EntryEBP, FrameLocalMinusFortyEightDisplacement);
        Push(state, SecondPushRawFileOffset, state.EAX.Value);
        Push(state, ThirdPushRawFileOffset, state.ESI!.Value);

        state.Reads.Add(new(
            CallRawFileOffset,
            CapacityFinalCalleeAlignmentDispatchReadKind.CallCellTargetVirtualAddress,
            CallCellVirtualAddress,
            capture.CallCellTargetVirtualAddress.HasValue,
            capture.CallCellTargetVirtualAddress));
        if (capture.CallCellTargetVirtualAddress is not uint callCellTarget)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlignmentDispatchOutcome.MissingCallCellTarget,
                state,
                testAlBranchTaken: false,
                entryEsiReadReached: true,
                entryEsiDwordCaptured: true,
                entryEsiDword: entryEsiDword,
                testEntryEcxBranchTaken: false,
                testComputedEcxBranchTaken: false,
                globalDispatchReadReached: true,
                globalDispatchDwordCaptured: true,
                globalDispatchDword: globalDispatchDword,
                callCellReadReached: true,
                unreachedCallReturnStatusProvided: capture.CallReturnedNormally.HasValue,
                stopAt: CallRawFileOffset);
        }

        if (capture.CallReturnedNormally is not bool callReturnedNormally)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlignmentDispatchOutcome.MissingCallReturnStatus,
                state,
                testAlBranchTaken: false,
                entryEsiReadReached: true,
                entryEsiDwordCaptured: true,
                entryEsiDword: entryEsiDword,
                testEntryEcxBranchTaken: false,
                testComputedEcxBranchTaken: false,
                globalDispatchReadReached: true,
                globalDispatchDwordCaptured: true,
                globalDispatchDword: globalDispatchDword,
                callCellReadReached: true,
                callCellTargetCaptured: true,
                callCellTargetVirtualAddress: callCellTarget,
                callReached: true,
                stopAt: CallRawFileOffset);
        }

        return BuildPlan(
            capture,
            callReturnedNormally
                ? CapacityFinalCalleeAlignmentDispatchOutcome.CallReturnedNormally
                : CapacityFinalCalleeAlignmentDispatchOutcome.CallDidNotReturnNormally,
            state,
            testAlBranchTaken: false,
            entryEsiReadReached: true,
            entryEsiDwordCaptured: true,
            entryEsiDword: entryEsiDword,
            testEntryEcxBranchTaken: false,
            testComputedEcxBranchTaken: false,
            globalDispatchReadReached: true,
            globalDispatchDwordCaptured: true,
            globalDispatchDword: globalDispatchDword,
            callCellReadReached: true,
            callCellTargetCaptured: true,
            callCellTargetVirtualAddress: callCellTarget,
            callReached: true,
            callReturnedNormally: callReturnedNormally,
            unreachedCallReturnStatusProvided: false,
            stopAt: callReturnedNormally ? BoundaryRawFileOffsetExclusive : CallRawFileOffset);
    }

    private static void Push(
        ProjectionState state,
        uint rawFileOffset,
        uint value)
    {
        uint stackAddress = unchecked(state.ESP!.Value - sizeof(uint));
        state.ESP = stackAddress;
        state.Pushes.Add(new(rawFileOffset, stackAddress, value));
    }

    private static CapacityFinalCalleeAlignmentDispatchFlags TestFlags(uint value) =>
        AndFlags(value);

    private static CapacityFinalCalleeAlignmentDispatchFlags AndFlags(
        byte left,
        byte right) =>
        AndFlags((uint)(left & right));

    private static CapacityFinalCalleeAlignmentDispatchFlags AndFlags(uint result) =>
        new(
            false,
            HasEvenParity((byte)result),
            null,
            result == 0,
            (result & 0x80000000) != 0,
            false);

    private static bool HasEvenParity(byte value) =>
        BitOperations.PopCount((uint)value) % 2 == 0;

    private static CapacityFinalCalleeAlignmentDispatchInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null,
        byte? immediateValue = null,
        uint? branchTargetRawFileOffset = null,
        uint? branchTargetVirtualAddress = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            peVirtualAddressOperand,
            immediateValue,
            branchTargetRawFileOffset,
            branchTargetVirtualAddress);

    private static CapacityFinalCalleeAlignmentDispatchPlan BuildPlan(
        CapacityFinalCalleeAlignmentDispatchCapture capture,
        CapacityFinalCalleeAlignmentDispatchOutcome outcome,
        ProjectionState state,
        bool testAlBranchTaken = false,
        uint? testAlSelectedDestinationRawFileOffset = null,
        uint? testAlSelectedDestinationVirtualAddress = null,
        bool entryEsiReadReached = false,
        bool entryEsiDwordCaptured = false,
        uint? entryEsiDword = null,
        bool testEntryEcxBranchTaken = false,
        uint? testEntryEcxSelectedDestinationRawFileOffset = null,
        uint? testEntryEcxSelectedDestinationVirtualAddress = null,
        bool testComputedEcxBranchTaken = false,
        uint? testComputedEcxSelectedDestinationRawFileOffset = null,
        uint? testComputedEcxSelectedDestinationVirtualAddress = null,
        bool globalDispatchReadReached = false,
        bool globalDispatchDwordCaptured = false,
        uint? globalDispatchDword = null,
        bool callCellReadReached = false,
        bool callCellTargetCaptured = false,
        uint? callCellTargetVirtualAddress = null,
        bool callReached = false,
        bool? callReturnedNormally = null,
        bool unreachedEntryEsiDwordCaptureProvided = false,
        bool unreachedGlobalDispatchDwordCaptureProvided = false,
        bool unreachedCallCellTargetCaptureProvided = false,
        bool unreachedCallReturnStatusProvided = false,
        uint stopAt = EntryRawFileOffset)
    {
        bool testAlBranchReached = state.TestAlBranchReached;
        bool testEntryEcxBranchReached = state.TestEntryEcxBranchReached;
        bool testComputedEcxBranchReached = state.TestComputedEcxBranchReached;
        return new(
            capture,
            outcome,
            state.Reads.ToImmutableArray(),
            state.FrameWrites.ToImmutableArray(),
            state.Pushes.ToImmutableArray(),
            state.EAX,
            state.ESI,
            state.ECX,
            state.EDI,
            state.EDX,
            state.EBX,
            state.ESP,
            state.TestAlReached,
            state.TestAlReached ? state.TestAlFlags : null,
            testAlBranchReached,
            testAlBranchReached ? testAlBranchTaken : null,
            testAlBranchReached ? testAlSelectedDestinationRawFileOffset : null,
            testAlBranchReached ? testAlSelectedDestinationVirtualAddress : null,
            entryEsiReadReached,
            entryEsiReadReached && entryEsiDwordCaptured,
            entryEsiReadReached && entryEsiDwordCaptured ? entryEsiDword : null,
            state.TestEntryEcxReached,
            state.TestEntryEcxReached ? state.TestEntryEcxFlags : null,
            testEntryEcxBranchReached,
            testEntryEcxBranchReached ? testEntryEcxBranchTaken : null,
            testEntryEcxBranchReached ? testEntryEcxSelectedDestinationRawFileOffset : null,
            testEntryEcxBranchReached ? testEntryEcxSelectedDestinationVirtualAddress : null,
            state.TestComputedEcxReached,
            state.TestComputedEcxReached ? state.TestComputedEcxFlags : null,
            testComputedEcxBranchReached,
            testComputedEcxBranchReached ? testComputedEcxBranchTaken : null,
            testComputedEcxBranchReached
                ? testComputedEcxSelectedDestinationRawFileOffset
                : null,
            testComputedEcxBranchReached
                ? testComputedEcxSelectedDestinationVirtualAddress
                : null,
            globalDispatchReadReached,
            globalDispatchReadReached && globalDispatchDwordCaptured,
            globalDispatchReadReached && globalDispatchDwordCaptured
                ? globalDispatchDword
                : null,
            callCellReadReached,
            callCellReadReached && callCellTargetCaptured,
            callCellReadReached && callCellTargetCaptured
                ? callCellTargetVirtualAddress
                : null,
            callReached,
            callReached ? callReturnedNormally : null,
            callReached,
            (!entryEsiReadReached && capture.EntryEsiDword.HasValue)
                || unreachedEntryEsiDwordCaptureProvided,
            (!globalDispatchReadReached && capture.GlobalDispatchDword.HasValue)
                || unreachedGlobalDispatchDwordCaptureProvided,
            (!callCellReadReached && capture.CallCellTargetVirtualAddress.HasValue)
                || unreachedCallCellTargetCaptureProvided,
            (!callReached && capture.CallReturnedNormally.HasValue)
                || unreachedCallReturnStatusProvided,
            stopAt);
    }

    private sealed class ProjectionState
    {
        public ProjectionState(CapacityFinalCalleeAlignmentDispatchCapture capture)
        {
            EAX = capture.EntryEAX;
            ESI = capture.EntryESI;
            ECX = capture.EntryECX;
            ESP = capture.EntryESP;
        }

        public List<CapacityFinalCalleeAlignmentDispatchRead> Reads { get; } = new();
        public List<CapacityFinalCalleeAlignmentDispatchWrite> FrameWrites { get; } = new();
        public List<CapacityFinalCalleeAlignmentDispatchPush> Pushes { get; } = new();
        public uint? EAX { get; set; }
        public uint? ESI { get; set; }
        public uint? ECX { get; set; }
        public uint? EDI { get; set; }
        public uint? EDX { get; set; }
        public uint? EBX { get; set; }
        public uint? ESP { get; set; }
        public bool TestAlReached { get; set; }
        public CapacityFinalCalleeAlignmentDispatchFlags? TestAlFlags { get; set; }
        public bool TestAlBranchReached { get; set; }
        public bool TestEntryEcxReached { get; set; }
        public CapacityFinalCalleeAlignmentDispatchFlags? TestEntryEcxFlags { get; set; }
        public bool TestEntryEcxBranchReached { get; set; }
        public bool TestComputedEcxReached { get; set; }
        public CapacityFinalCalleeAlignmentDispatchFlags? TestComputedEcxFlags { get; set; }
        public bool TestComputedEcxBranchReached { get; set; }
    }
}

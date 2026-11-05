using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkIoErrorCallbackLoopGateInputs(
    NetworkIoErrorCallbackReturnInputs ParentReturnInputs,
    uint? FreshPostCallEBP,
    uint? FreshLocalMinus8DWORD,
    uint? FreshLocalMinus10DWORD,
    uint? FreshPostCallEDI);

public enum NetworkIoErrorCallbackLoopGateOutcome
{
    ParentReturnNotAtExact9BB,
    ParentReturnNotClean,
    MissingFreshPostCallEBP,
    MissingFreshLocalMinus8DWORD,
    MissingFreshLocalMinus10DWORD,
    MissingFreshPostCallEDI,
    BranchTo8F0,
    FallThroughTo9CB
}

public enum NetworkIoErrorCallbackLoopGateCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct NetworkIoErrorCallbackLoopGateCaptureStates(
    NetworkIoErrorCallbackLoopGateCaptureState FreshPostCallEBP,
    NetworkIoErrorCallbackLoopGateCaptureState FreshLocalMinus8DWORD,
    NetworkIoErrorCallbackLoopGateCaptureState FreshLocalMinus10DWORD,
    NetworkIoErrorCallbackLoopGateCaptureState FreshPostCallEDI);

public enum NetworkIoErrorCallbackLoopGateRegister
{
    EAX,
    ESI
}

public readonly record struct NetworkIoErrorCallbackLoopGateRead(
    uint RawFileOffset,
    NetworkIoErrorCallbackLoopGateRegister Register,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool NativeReadExecuted);

public enum NetworkIoErrorCallbackLoopGateInstructionKind
{
    MoveEAXFromEBPMinus8,
    MoveESIFromEBPMinus10,
    IncrementEDI,
    CompareEDIWith64,
    SignedLessThanBranch
}

public readonly record struct NetworkIoErrorCallbackLoopGateInstruction(
    NetworkIoErrorCallbackLoopGateInstructionKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? Value = null,
    uint? BranchTargetRawFileOffset = null,
    bool? BranchTaken = null);

public readonly record struct NetworkIoErrorCallbackLoopGateFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct NetworkIoErrorCallbackLoopGateOrderedEffect(
    int Sequence,
    NetworkIoErrorCallbackReturnOrderedEffect? ParentReturnEffect,
    NetworkIoErrorCallbackLoopGateRead? Read,
    NetworkIoErrorCallbackLoopGateInstruction? Instruction);

public sealed record NetworkIoErrorCallbackLoopGatePlan(
    NetworkIoErrorCallbackReturnPlan ParentReturnPlan,
    NetworkIoErrorCallbackLoopGateInputs Capture,
    NetworkIoErrorCallbackLoopGateOutcome Outcome,
    ImmutableArray<NetworkIoErrorCallbackLoopGateRead> Reads,
    ImmutableArray<NetworkIoErrorCallbackLoopGateInstruction> InstructionOrder,
    ImmutableArray<NetworkIoErrorCallbackLoopGateOrderedEffect> OrderedEffects,
    NetworkIoErrorCallbackLoopGateCaptureStates CaptureStates,
    uint? EBP,
    uint? EAX,
    uint? ESI,
    uint? EDI,
    NetworkIoErrorCallbackLoopGateFlags? FlagsAfterMoveEAX,
    NetworkIoErrorCallbackLoopGateFlags? FlagsAfterMoveESI,
    NetworkIoErrorCallbackLoopGateFlags? FlagsAfterIncrement,
    NetworkIoErrorCallbackLoopGateFlags? FlagsAfterCompare,
    NetworkIoErrorCallbackLoopGateFlags? FlagsAtStop,
    bool ChildGateAdmitted,
    bool IncrementExecuted,
    bool CompareExecuted,
    bool BranchExecuted,
    bool BranchTaken,
    bool BranchTargetEntered,
    bool MemoryWriteExecuted,
    bool NativeInstructionsExecuted,
    bool BoundedCompletionBoundaryReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    uint? LastReachedRawFileOffset,
    uint? StopBeforeRawFileOffset);

/// <summary>
/// Reprojects the clean return jump to 9BB and models only the bounded loop
/// gate's fresh scalar reads and signed branch; it never executes native code.
/// </summary>
public static class NetworkIoErrorCallbackLoopGateProjection
{
    public const uint EntryRawFileOffset = 0x00BBC9BB;
    public const uint MoveEAXRawFileOffset = 0x00BBC9BB;
    public const uint MoveESIRawFileOffset = 0x00BBC9BE;
    public const uint IncrementEDIRawFileOffset = 0x00BBC9C1;
    public const uint CompareEDIRawFileOffset = 0x00BBC9C2;
    public const uint SignedBranchRawFileOffset = 0x00BBC9C5;
    public const uint SignedBranchNextInstructionRawFileOffset = 0x00BBC9CB;
    public const uint BranchTargetRawFileOffset = 0x00BBC8F0;
    public const uint FallThroughRawFileOffset = 0x00BBC9CB;
    public const uint EBPMinus8Displacement = 8;
    public const uint EBPMinus10Displacement = 0x10;
    public const uint EDICompareImmediate = 0x64;
    public const byte DwordWidth = 4;

    public static bool ExecutesNativeInstructions => false;

    public static bool ExecutesNativeReads => false;

    public static bool ExecutesMemoryWrites => false;

    public static bool EntersSelectedBranchTarget => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x8B, 0x45, 0xF8,
            0x8B, 0x75, 0xF0,
            0x47,
            0x83, 0xFF, 0x64,
            0x0F, 0x8C, 0x25, 0xFF, 0xFF, 0xFF);

    public static NetworkIoErrorCallbackLoopGatePlan Project(
        NetworkIoErrorCallbackLoopGateInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(inputs.ParentReturnInputs);

        NetworkIoErrorCallbackReturnPlan parent =
            NetworkIoErrorCallbackReturnProjection.Project(inputs.ParentReturnInputs);
        var reads =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackLoopGateRead>();
        var instructions =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackLoopGateInstruction>();
        var ordered =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackLoopGateOrderedEffect>();

        foreach (NetworkIoErrorCallbackReturnOrderedEffect effect in parent.OrderedEffects)
        {
            ordered.Add(new(ordered.Count, effect, null, null));
        }

        bool expectedParentEndpoint =
            parent.Outcome == NetworkIoErrorCallbackReturnOutcome.OpaqueCallReturnedNormally
            && parent.OpaqueCallReturnedNormally == true
            && parent.LastReachedRawFileOffset
                == NetworkIoErrorCallbackReturnProjection.ReturnJumpRawFileOffset
            && parent.StopBeforeRawFileOffset == EntryRawFileOffset
            && parent.ExactBoundedCompletion
            && !parent.ReturnJumpTargetEntered;
        bool cleanParent = expectedParentEndpoint
            && parent.ChildGateAdmitted
            && parent.ExactCleanCompletion;

        NetworkIoErrorCallbackLoopGateCaptureState ebpState =
            StateBeforeAdmission(inputs.FreshPostCallEBP.HasValue);
        NetworkIoErrorCallbackLoopGateCaptureState local8State =
            StateBeforeAdmission(inputs.FreshLocalMinus8DWORD.HasValue);
        NetworkIoErrorCallbackLoopGateCaptureState local10State =
            StateBeforeAdmission(inputs.FreshLocalMinus10DWORD.HasValue);
        NetworkIoErrorCallbackLoopGateCaptureState ediState =
            StateBeforeAdmission(inputs.FreshPostCallEDI.HasValue);
        NetworkIoErrorCallbackLoopGateOutcome outcome;
        uint? ebp = null;
        uint? eax = null;
        uint? esi = null;
        uint? edi = null;
        NetworkIoErrorCallbackLoopGateFlags? flagsAfterMoveEAX = null;
        NetworkIoErrorCallbackLoopGateFlags? flagsAfterMoveESI = null;
        NetworkIoErrorCallbackLoopGateFlags? flagsAfterIncrement = null;
        NetworkIoErrorCallbackLoopGateFlags? flagsAfterCompare = null;
        NetworkIoErrorCallbackLoopGateFlags? flags = ToLoopFlags(parent.FlagsAtStop);
        bool incrementExecuted = false;
        bool compareExecuted = false;
        bool branchExecuted = false;
        bool branchTaken = false;
        bool boundedCompletion = false;
        bool exactBoundedCompletion = false;
        bool exactCleanCompletion = false;
        uint? lastReached = parent.LastReachedRawFileOffset;
        uint? stopBefore = parent.StopBeforeRawFileOffset;

        if (!expectedParentEndpoint)
        {
            outcome = NetworkIoErrorCallbackLoopGateOutcome.ParentReturnNotAtExact9BB;
            MarkUnusedInputs(inputs, ref ebpState, ref local8State, ref local10State, ref ediState);
            boundedCompletion = parent.BoundedCompletionBoundaryReached;
            exactBoundedCompletion = parent.ExactBoundedCompletion;
        }
        else if (!cleanParent)
        {
            outcome = NetworkIoErrorCallbackLoopGateOutcome.ParentReturnNotClean;
            MarkUnusedInputs(inputs, ref ebpState, ref local8State, ref local10State, ref ediState);
            boundedCompletion = parent.BoundedCompletionBoundaryReached;
            exactBoundedCompletion = parent.ExactBoundedCompletion;
        }
        else
        {
            flags = UnknownFlags();
            ebpState = inputs.FreshPostCallEBP.HasValue
                ? NetworkIoErrorCallbackLoopGateCaptureState.Captured
                : NetworkIoErrorCallbackLoopGateCaptureState.Missing;
            lastReached = EntryRawFileOffset;
            stopBefore = MoveEAXRawFileOffset;

            if (inputs.FreshPostCallEBP is not uint capturedEBP)
            {
                local8State = UnreachedOrUnused(inputs.FreshLocalMinus8DWORD.HasValue);
                local10State = UnreachedOrUnused(inputs.FreshLocalMinus10DWORD.HasValue);
                ediState = UnreachedOrUnused(inputs.FreshPostCallEDI.HasValue);
                outcome = NetworkIoErrorCallbackLoopGateOutcome.MissingFreshPostCallEBP;
            }
            else
            {
                ebp = capturedEBP;
                uint local8Address = unchecked(capturedEBP - EBPMinus8Displacement);
                local8State = inputs.FreshLocalMinus8DWORD.HasValue
                    ? NetworkIoErrorCallbackLoopGateCaptureState.Captured
                    : NetworkIoErrorCallbackLoopGateCaptureState.Missing;
                NetworkIoErrorCallbackLoopGateRead local8Read = AddRead(
                    reads,
                    instructions,
                    ordered,
                    NetworkIoErrorCallbackLoopGateInstructionKind.MoveEAXFromEBPMinus8,
                    MoveEAXRawFileOffset,
                    NetworkIoErrorCallbackLoopGateRegister.EAX,
                    local8Address,
                    inputs.FreshLocalMinus8DWORD);
                flagsAfterMoveEAX = UnknownFlags();
                flags = flagsAfterMoveEAX;
                lastReached = MoveEAXRawFileOffset;
                stopBefore = MoveEAXRawFileOffset;

                if (!local8Read.Captured)
                {
                    local10State = UnreachedOrUnused(inputs.FreshLocalMinus10DWORD.HasValue);
                    ediState = UnreachedOrUnused(inputs.FreshPostCallEDI.HasValue);
                    outcome = NetworkIoErrorCallbackLoopGateOutcome.MissingFreshLocalMinus8DWORD;
                }
                else
                {
                    eax = local8Read.Value;
                    uint local10Address = unchecked(capturedEBP - EBPMinus10Displacement);
                    local10State = inputs.FreshLocalMinus10DWORD.HasValue
                        ? NetworkIoErrorCallbackLoopGateCaptureState.Captured
                        : NetworkIoErrorCallbackLoopGateCaptureState.Missing;
                    NetworkIoErrorCallbackLoopGateRead local10Read = AddRead(
                        reads,
                        instructions,
                        ordered,
                        NetworkIoErrorCallbackLoopGateInstructionKind.MoveESIFromEBPMinus10,
                        MoveESIRawFileOffset,
                        NetworkIoErrorCallbackLoopGateRegister.ESI,
                        local10Address,
                        inputs.FreshLocalMinus10DWORD);
                    flagsAfterMoveESI = UnknownFlags();
                    flags = flagsAfterMoveESI;
                    lastReached = MoveESIRawFileOffset;
                    stopBefore = MoveESIRawFileOffset;

                    if (!local10Read.Captured)
                    {
                        ediState = UnreachedOrUnused(inputs.FreshPostCallEDI.HasValue);
                        outcome = NetworkIoErrorCallbackLoopGateOutcome.MissingFreshLocalMinus10DWORD;
                    }
                    else
                    {
                        esi = local10Read.Value;
                        ediState = inputs.FreshPostCallEDI.HasValue
                            ? NetworkIoErrorCallbackLoopGateCaptureState.Captured
                            : NetworkIoErrorCallbackLoopGateCaptureState.Missing;
                        lastReached = IncrementEDIRawFileOffset;
                        stopBefore = IncrementEDIRawFileOffset;

                        if (inputs.FreshPostCallEDI is not uint capturedEDI)
                        {
                            outcome = NetworkIoErrorCallbackLoopGateOutcome.MissingFreshPostCallEDI;
                        }
                        else
                        {
                            edi = unchecked(capturedEDI + 1u);
                            incrementExecuted = true;
                            flagsAfterIncrement =
                                FlagsAfterIncrement(capturedEDI, edi.Value);
                            flags = flagsAfterIncrement;
                            AddInstruction(
                                instructions,
                                ordered,
                                NetworkIoErrorCallbackLoopGateInstructionKind.IncrementEDI,
                                IncrementEDIRawFileOffset,
                                value: edi);

                            compareExecuted = true;
                            NetworkIoErrorCallbackLoopGateFlags compareFlags =
                                FlagsAfterCompare(edi.Value, EDICompareImmediate);
                            flagsAfterCompare = compareFlags;
                            flags = compareFlags;
                            AddInstruction(
                                instructions,
                                ordered,
                                NetworkIoErrorCallbackLoopGateInstructionKind.CompareEDIWith64,
                                CompareEDIRawFileOffset,
                                value: edi);

                            branchExecuted = true;
                            branchTaken = compareFlags.Sign != compareFlags.Overflow;
                            uint target = branchTaken
                                ? BranchTargetRawFileOffset
                                : FallThroughRawFileOffset;
                            AddInstruction(
                                instructions,
                                ordered,
                                NetworkIoErrorCallbackLoopGateInstructionKind.SignedLessThanBranch,
                                SignedBranchRawFileOffset,
                                branchTarget: target,
                                branchTaken: branchTaken);
                            outcome = branchTaken
                                ? NetworkIoErrorCallbackLoopGateOutcome.BranchTo8F0
                                : NetworkIoErrorCallbackLoopGateOutcome.FallThroughTo9CB;
                            lastReached = SignedBranchRawFileOffset;
                            stopBefore = target;
                            boundedCompletion = true;
                            exactBoundedCompletion = true;
                            exactCleanCompletion = true;
                        }
                    }
                }
            }
        }

        return new(
            parent,
            inputs,
            outcome,
            reads.ToImmutable(),
            instructions.ToImmutable(),
            ordered.ToImmutable(),
            new(ebpState, local8State, local10State, ediState),
            ebp,
            eax,
            esi,
            edi,
            flagsAfterMoveEAX,
            flagsAfterMoveESI,
            flagsAfterIncrement,
            flagsAfterCompare,
            flags,
            cleanParent,
            incrementExecuted,
            compareExecuted,
            branchExecuted,
            branchTaken,
            BranchTargetEntered: false,
            MemoryWriteExecuted: false,
            NativeInstructionsExecuted: false,
            boundedCompletion,
            exactBoundedCompletion,
            exactCleanCompletion,
            lastReached,
            stopBefore);
    }

    private static NetworkIoErrorCallbackLoopGateRead AddRead(
        ImmutableArray<NetworkIoErrorCallbackLoopGateRead>.Builder reads,
        ImmutableArray<NetworkIoErrorCallbackLoopGateInstruction>.Builder instructions,
        ImmutableArray<NetworkIoErrorCallbackLoopGateOrderedEffect>.Builder ordered,
        NetworkIoErrorCallbackLoopGateInstructionKind kind,
        uint rawFileOffset,
        NetworkIoErrorCallbackLoopGateRegister register,
        uint address,
        uint? value)
    {
        var read = new NetworkIoErrorCallbackLoopGateRead(
            rawFileOffset,
            register,
            address,
            DwordWidth,
            Attempted: true,
            Captured: value.HasValue,
            Value: value,
            NativeReadExecuted: false);
        var instruction = new NetworkIoErrorCallbackLoopGateInstruction(
            kind,
            rawFileOffset,
            address,
            value);
        reads.Add(read);
        instructions.Add(instruction);
        ordered.Add(new(ordered.Count, null, read, instruction));
        return read;
    }

    private static void AddInstruction(
        ImmutableArray<NetworkIoErrorCallbackLoopGateInstruction>.Builder instructions,
        ImmutableArray<NetworkIoErrorCallbackLoopGateOrderedEffect>.Builder ordered,
        NetworkIoErrorCallbackLoopGateInstructionKind kind,
        uint rawFileOffset,
        uint? value = null,
        uint? branchTarget = null,
        bool? branchTaken = null)
    {
        var instruction = new NetworkIoErrorCallbackLoopGateInstruction(
            kind,
            rawFileOffset,
            Value: value,
            BranchTargetRawFileOffset: branchTarget,
            BranchTaken: branchTaken);
        instructions.Add(instruction);
        ordered.Add(new(ordered.Count, null, null, instruction));
    }

    private static NetworkIoErrorCallbackLoopGateFlags FlagsAfterIncrement(
        uint before,
        uint after) =>
        new(
            Carry: null,
            Parity: HasEvenParity((byte)after),
            AuxiliaryCarry: ((before ^ after) & 0x10) != 0,
            Zero: after == 0,
            Sign: (after & 0x80000000) != 0,
            Overflow: before == 0x7FFFFFFF);

    private static NetworkIoErrorCallbackLoopGateFlags UnknownFlags() =>
        new(
            Carry: null,
            Parity: null,
            AuxiliaryCarry: null,
            Zero: null,
            Sign: null,
            Overflow: null);

    private static NetworkIoErrorCallbackLoopGateFlags? ToLoopFlags(
        NetworkIoErrorCallbackPreparationCompareFlags? flags) =>
        flags is NetworkIoErrorCallbackPreparationCompareFlags known
            ? new(
                known.Carry,
                known.Parity,
                known.AuxiliaryCarry,
                known.Zero,
                known.Sign,
                known.Overflow)
            : null;

    private static NetworkIoErrorCallbackLoopGateFlags FlagsAfterCompare(
        uint left,
        uint right)
    {
        uint result = unchecked(left - right);
        return new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: (((left ^ right) & (left ^ result)) & 0x80000000) != 0);
    }

    private static bool HasEvenParity(byte value)
    {
        value ^= (byte)(value >> 4);
        value &= 0x0F;
        return ((0x6996 >> value) & 1) == 0;
    }

    private static NetworkIoErrorCallbackLoopGateCaptureState StateBeforeAdmission(
        bool supplied) =>
        supplied
            ? NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            : NetworkIoErrorCallbackLoopGateCaptureState.Unreached;

    private static NetworkIoErrorCallbackLoopGateCaptureState UnreachedOrUnused(
        bool supplied) => StateBeforeAdmission(supplied);

    private static void MarkUnusedInputs(
        NetworkIoErrorCallbackLoopGateInputs inputs,
        ref NetworkIoErrorCallbackLoopGateCaptureState ebp,
        ref NetworkIoErrorCallbackLoopGateCaptureState local8,
        ref NetworkIoErrorCallbackLoopGateCaptureState local10,
        ref NetworkIoErrorCallbackLoopGateCaptureState edi)
    {
        ebp = StateBeforeAdmission(inputs.FreshPostCallEBP.HasValue);
        local8 = StateBeforeAdmission(inputs.FreshLocalMinus8DWORD.HasValue);
        local10 = StateBeforeAdmission(inputs.FreshLocalMinus10DWORD.HasValue);
        edi = StateBeforeAdmission(inputs.FreshPostCallEDI.HasValue);
    }
}

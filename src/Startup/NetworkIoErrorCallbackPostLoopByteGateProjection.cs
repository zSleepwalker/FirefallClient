using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkIoErrorCallbackPostLoopByteGateInputs(
    NetworkIoErrorCallbackLoopGateInputs ParentLoopGateInputs,
    uint? FreshLocalMinus24DWORD,
    byte? FreshComparedBYTE,
    uint? FreshLocalMinus28DWORD);

public enum NetworkIoErrorCallbackPostLoopByteGateOutcome
{
    ParentLoopGateNotAtCleanFallthrough9CB,
    MissingFreshLocalMinus24DWORD,
    MissingFreshComparedByte,
    MissingFreshLocalMinus28DWORD,
    BranchTo8B0,
    FallThroughTo9DB
}

public enum NetworkIoErrorCallbackPostLoopByteGateCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct NetworkIoErrorCallbackPostLoopByteGateCaptureStates(
    NetworkIoErrorCallbackPostLoopByteGateCaptureState FreshLocalMinus24DWORD,
    NetworkIoErrorCallbackPostLoopByteGateCaptureState FreshComparedBYTE,
    NetworkIoErrorCallbackPostLoopByteGateCaptureState FreshLocalMinus28DWORD);

public enum NetworkIoErrorCallbackPostLoopByteGateReadKind
{
    FreshLocalMinus24DWORD,
    FreshComparedByte,
    FreshLocalMinus28DWORD
}

public readonly record struct NetworkIoErrorCallbackPostLoopByteGateRead(
    uint RawFileOffset,
    NetworkIoErrorCallbackPostLoopByteGateReadKind Kind,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool NativeReadExecuted);

public enum NetworkIoErrorCallbackPostLoopByteGateInstructionKind
{
    MoveEAXFromEBPMinus24,
    CompareByteAtEAXPlus4WithZero,
    MoveEAXFromEBPMinus28,
    ZeroBranch
}

public readonly record struct NetworkIoErrorCallbackPostLoopByteGateInstruction(
    NetworkIoErrorCallbackPostLoopByteGateInstructionKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? Value = null,
    uint? BranchTargetRawFileOffset = null,
    bool? BranchTaken = null);

public readonly record struct NetworkIoErrorCallbackPostLoopByteGateOrderedEffect(
    int Sequence,
    NetworkIoErrorCallbackLoopGateOrderedEffect? ParentLoopGateEffect,
    NetworkIoErrorCallbackPostLoopByteGateRead? Read,
    NetworkIoErrorCallbackPostLoopByteGateInstruction? Instruction);

public sealed record NetworkIoErrorCallbackPostLoopByteGatePlan(
    NetworkIoErrorCallbackLoopGatePlan ParentLoopGatePlan,
    NetworkIoErrorCallbackPostLoopByteGateInputs Capture,
    NetworkIoErrorCallbackPostLoopByteGateOutcome Outcome,
    ImmutableArray<NetworkIoErrorCallbackPostLoopByteGateRead> Reads,
    ImmutableArray<NetworkIoErrorCallbackPostLoopByteGateInstruction> InstructionOrder,
    ImmutableArray<NetworkIoErrorCallbackPostLoopByteGateOrderedEffect> OrderedEffects,
    NetworkIoErrorCallbackPostLoopByteGateCaptureStates CaptureStates,
    uint? EBP,
    uint? EAX,
    NetworkIoErrorCallbackLoopGateFlags? FlagsAfterComparedByte,
    NetworkIoErrorCallbackLoopGateFlags? FlagsAtStop,
    bool ChildGateAdmitted,
    bool BranchExecuted,
    bool BranchTaken,
    bool BranchTargetEntered,
    bool FallThroughEntered,
    bool MemoryWriteExecuted,
    bool CallExecuted,
    bool PopExecuted,
    bool NativeInstructionsExecuted,
    bool ChildBoundedCompletionBoundaryReached,
    bool ExactChildCompletion,
    bool ExactCleanCompletion,
    uint? LastReachedRawFileOffset,
    uint? StopBeforeRawFileOffset);

/// <summary>
/// Reprojects the clean loop-gate fallthrough and records the post-loop byte
/// comparison without entering either branch destination or executing code.
/// </summary>
public static class NetworkIoErrorCallbackPostLoopByteGateProjection
{
    public const uint EntryRawFileOffset = 0x00BBC9CB;
    public const uint MoveEAXFromEBPMinus24RawFileOffset = 0x00BBC9CB;
    public const uint CompareByteAtEAXPlus4RawFileOffset = 0x00BBC9CE;
    public const uint MoveEAXFromEBPMinus28RawFileOffset = 0x00BBC9D2;
    public const uint ZeroBranchRawFileOffset = 0x00BBC9D5;
    public const uint ZeroBranchNextInstructionRawFileOffset = 0x00BBC9DB;
    public const uint ZeroBranchTargetRawFileOffset = 0x00BBC8B0;
    public const uint FallThroughRawFileOffset = 0x00BBC9DB;
    public const uint EBPMinus24Displacement = 0x24;
    public const uint EBPMinus28Displacement = 0x28;
    public const uint ComparedByteOffset = 4;
    public const byte DwordWidth = 4;
    public const byte ByteWidth = 1;

    public static bool ExecutesNativeInstructions => false;

    public static bool ExecutesNativeReads => false;

    public static bool ExecutesMemoryWrites => false;

    public static bool ExecutesCalls => false;

    public static bool ExecutesPops => false;

    public static bool EntersSelectedBranchDestination => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x8B, 0x45, 0xDC,
            0x80, 0x78, 0x04, 0x00,
            0x8B, 0x45, 0xD8,
            0x0F, 0x84, 0xD5, 0xFE, 0xFF, 0xFF);

    public static NetworkIoErrorCallbackPostLoopByteGatePlan Project(
        NetworkIoErrorCallbackPostLoopByteGateInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(inputs.ParentLoopGateInputs);

        NetworkIoErrorCallbackLoopGatePlan parent =
            NetworkIoErrorCallbackLoopGateProjection.Project(
                inputs.ParentLoopGateInputs);
        var reads =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackPostLoopByteGateRead>();
        var instructions =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackPostLoopByteGateInstruction>();
        var ordered =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackPostLoopByteGateOrderedEffect>();

        foreach (NetworkIoErrorCallbackLoopGateOrderedEffect effect in parent.OrderedEffects)
        {
            ordered.Add(new(ordered.Count, effect, null, null));
        }

        bool cleanParent = parent.Outcome
                == NetworkIoErrorCallbackLoopGateOutcome.FallThroughTo9CB
            && parent.ChildGateAdmitted
            && parent.ExactBoundedCompletion
            && parent.ExactCleanCompletion
            && parent.BranchExecuted
            && !parent.BranchTaken
            && !parent.BranchTargetEntered
            && parent.EBP.HasValue
            && parent.LastReachedRawFileOffset
                == NetworkIoErrorCallbackLoopGateProjection.SignedBranchRawFileOffset
            && parent.StopBeforeRawFileOffset
                == EntryRawFileOffset;

        NetworkIoErrorCallbackPostLoopByteGateCaptureState local24State =
            StateBeforeAdmission(inputs.FreshLocalMinus24DWORD.HasValue);
        NetworkIoErrorCallbackPostLoopByteGateCaptureState comparedByteState =
            StateBeforeAdmission(inputs.FreshComparedBYTE.HasValue);
        NetworkIoErrorCallbackPostLoopByteGateCaptureState local28State =
            StateBeforeAdmission(inputs.FreshLocalMinus28DWORD.HasValue);
        NetworkIoErrorCallbackPostLoopByteGateOutcome outcome;
        uint? ebp = parent.EBP;
        uint? eax = null;
        NetworkIoErrorCallbackLoopGateFlags? flags = parent.FlagsAtStop;
        NetworkIoErrorCallbackLoopGateFlags? flagsAfterComparedByte = null;
        bool branchExecuted = false;
        bool branchTaken = false;
        bool childComplete = false;
        uint? lastReached = parent.LastReachedRawFileOffset;
        uint? stopBefore = parent.StopBeforeRawFileOffset;

        if (!cleanParent)
        {
            local24State = StateBeforeAdmission(inputs.FreshLocalMinus24DWORD.HasValue);
            comparedByteState = StateBeforeAdmission(inputs.FreshComparedBYTE.HasValue);
            local28State = StateBeforeAdmission(inputs.FreshLocalMinus28DWORD.HasValue);
            outcome =
                NetworkIoErrorCallbackPostLoopByteGateOutcome
                    .ParentLoopGateNotAtCleanFallthrough9CB;
        }
        else
        {
            local24State = inputs.FreshLocalMinus24DWORD.HasValue
                ? NetworkIoErrorCallbackPostLoopByteGateCaptureState.Captured
                : NetworkIoErrorCallbackPostLoopByteGateCaptureState.Missing;
            lastReached = EntryRawFileOffset;
            stopBefore = MoveEAXFromEBPMinus24RawFileOffset;
            uint firstAddress = unchecked(parent.EBP!.Value - EBPMinus24Displacement);
            NetworkIoErrorCallbackPostLoopByteGateRead firstRead = AddRead(
                reads,
                instructions,
                ordered,
                NetworkIoErrorCallbackPostLoopByteGateInstructionKind
                    .MoveEAXFromEBPMinus24,
                NetworkIoErrorCallbackPostLoopByteGateReadKind.FreshLocalMinus24DWORD,
                MoveEAXFromEBPMinus24RawFileOffset,
                firstAddress,
                DwordWidth,
                inputs.FreshLocalMinus24DWORD);

            if (!firstRead.Captured)
            {
                comparedByteState = UnreachedOrUnused(inputs.FreshComparedBYTE.HasValue);
                local28State = UnreachedOrUnused(inputs.FreshLocalMinus28DWORD.HasValue);
                outcome =
                    NetworkIoErrorCallbackPostLoopByteGateOutcome
                        .MissingFreshLocalMinus24DWORD;
            }
            else
            {
                eax = firstRead.Value;
                uint comparedByteAddress =
                    unchecked(firstRead.Value!.Value + ComparedByteOffset);
                comparedByteState = inputs.FreshComparedBYTE.HasValue
                    ? NetworkIoErrorCallbackPostLoopByteGateCaptureState.Captured
                    : NetworkIoErrorCallbackPostLoopByteGateCaptureState.Missing;
                lastReached = MoveEAXFromEBPMinus24RawFileOffset;
                stopBefore = CompareByteAtEAXPlus4RawFileOffset;
                uint? comparedByteValue = inputs.FreshComparedBYTE is byte comparedByte
                    ? comparedByte
                    : null;
                NetworkIoErrorCallbackPostLoopByteGateRead byteRead = AddRead(
                    reads,
                    instructions,
                    ordered,
                    NetworkIoErrorCallbackPostLoopByteGateInstructionKind
                        .CompareByteAtEAXPlus4WithZero,
                    NetworkIoErrorCallbackPostLoopByteGateReadKind.FreshComparedByte,
                    CompareByteAtEAXPlus4RawFileOffset,
                    comparedByteAddress,
                    ByteWidth,
                    comparedByteValue);

                if (!byteRead.Captured)
                {
                    local28State = UnreachedOrUnused(inputs.FreshLocalMinus28DWORD.HasValue);
                    outcome =
                        NetworkIoErrorCallbackPostLoopByteGateOutcome
                            .MissingFreshComparedByte;
                }
                else
                {
                    byte value = unchecked((byte)byteRead.Value!.Value);
                    flagsAfterComparedByte = FlagsAfterCompareWithZero(value);
                    flags = flagsAfterComparedByte;
                    uint secondAddress = unchecked(parent.EBP!.Value - EBPMinus28Displacement);
                    local28State = inputs.FreshLocalMinus28DWORD.HasValue
                        ? NetworkIoErrorCallbackPostLoopByteGateCaptureState.Captured
                        : NetworkIoErrorCallbackPostLoopByteGateCaptureState.Missing;
                    lastReached = CompareByteAtEAXPlus4RawFileOffset;
                    stopBefore = MoveEAXFromEBPMinus28RawFileOffset;
                    NetworkIoErrorCallbackPostLoopByteGateRead secondRead = AddRead(
                        reads,
                        instructions,
                        ordered,
                        NetworkIoErrorCallbackPostLoopByteGateInstructionKind
                            .MoveEAXFromEBPMinus28,
                        NetworkIoErrorCallbackPostLoopByteGateReadKind
                            .FreshLocalMinus28DWORD,
                        MoveEAXFromEBPMinus28RawFileOffset,
                        secondAddress,
                        DwordWidth,
                        inputs.FreshLocalMinus28DWORD);

                    if (!secondRead.Captured)
                    {
                        outcome =
                            NetworkIoErrorCallbackPostLoopByteGateOutcome
                                .MissingFreshLocalMinus28DWORD;
                    }
                    else
                    {
                        eax = secondRead.Value;
                        branchExecuted = true;
                        branchTaken = flagsAfterComparedByte.Value.Zero == true;
                        uint target = branchTaken
                            ? ZeroBranchTargetRawFileOffset
                            : FallThroughRawFileOffset;
                        AddInstruction(
                            instructions,
                            ordered,
                            NetworkIoErrorCallbackPostLoopByteGateInstructionKind
                                .ZeroBranch,
                            ZeroBranchRawFileOffset,
                            value: eax,
                            branchTarget: target,
                            branchTaken: branchTaken);
                        outcome = branchTaken
                            ? NetworkIoErrorCallbackPostLoopByteGateOutcome.BranchTo8B0
                            : NetworkIoErrorCallbackPostLoopByteGateOutcome.FallThroughTo9DB;
                        lastReached = ZeroBranchRawFileOffset;
                        stopBefore = target;
                        childComplete = true;
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
            new(local24State, comparedByteState, local28State),
            ebp,
            eax,
            flagsAfterComparedByte,
            flags,
            cleanParent,
            branchExecuted,
            branchTaken,
            BranchTargetEntered: false,
            FallThroughEntered: false,
            MemoryWriteExecuted: false,
            CallExecuted: false,
            PopExecuted: false,
            NativeInstructionsExecuted: false,
            ChildBoundedCompletionBoundaryReached: childComplete,
            ExactChildCompletion: childComplete,
            ExactCleanCompletion: childComplete,
            lastReached,
            stopBefore);
    }

    private static NetworkIoErrorCallbackPostLoopByteGateRead AddRead(
        ImmutableArray<NetworkIoErrorCallbackPostLoopByteGateRead>.Builder reads,
        ImmutableArray<NetworkIoErrorCallbackPostLoopByteGateInstruction>.Builder instructions,
        ImmutableArray<NetworkIoErrorCallbackPostLoopByteGateOrderedEffect>.Builder ordered,
        NetworkIoErrorCallbackPostLoopByteGateInstructionKind instructionKind,
        NetworkIoErrorCallbackPostLoopByteGateReadKind readKind,
        uint rawFileOffset,
        uint address,
        byte width,
        uint? value)
    {
        var read = new NetworkIoErrorCallbackPostLoopByteGateRead(
            rawFileOffset,
            readKind,
            address,
            width,
            Attempted: true,
            Captured: value.HasValue,
            Value: value,
            NativeReadExecuted: false);
        var instruction = new NetworkIoErrorCallbackPostLoopByteGateInstruction(
            instructionKind,
            rawFileOffset,
            address,
            value);
        reads.Add(read);
        instructions.Add(instruction);
        ordered.Add(new(ordered.Count, null, read, instruction));
        return read;
    }

    private static void AddInstruction(
        ImmutableArray<NetworkIoErrorCallbackPostLoopByteGateInstruction>.Builder instructions,
        ImmutableArray<NetworkIoErrorCallbackPostLoopByteGateOrderedEffect>.Builder ordered,
        NetworkIoErrorCallbackPostLoopByteGateInstructionKind kind,
        uint rawFileOffset,
        uint? value = null,
        uint? branchTarget = null,
        bool? branchTaken = null)
    {
        var instruction = new NetworkIoErrorCallbackPostLoopByteGateInstruction(
            kind,
            rawFileOffset,
            Value: value,
            BranchTargetRawFileOffset: branchTarget,
            BranchTaken: branchTaken);
        instructions.Add(instruction);
        ordered.Add(new(ordered.Count, null, null, instruction));
    }

    private static NetworkIoErrorCallbackLoopGateFlags FlagsAfterCompareWithZero(
        byte value) =>
        new(
            Carry: false,
            Parity: HasEvenParity(value),
            AuxiliaryCarry: false,
            Zero: value == 0,
            Sign: (value & 0x80) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value)
    {
        value ^= (byte)(value >> 4);
        value &= 0x0F;
        return ((0x6996 >> value) & 1) == 0;
    }

    private static NetworkIoErrorCallbackPostLoopByteGateCaptureState StateBeforeAdmission(
        bool supplied) =>
        supplied
            ? NetworkIoErrorCallbackPostLoopByteGateCaptureState.SuppliedButUnused
            : NetworkIoErrorCallbackPostLoopByteGateCaptureState.Unreached;

    private static NetworkIoErrorCallbackPostLoopByteGateCaptureState UnreachedOrUnused(
        bool supplied) => StateBeforeAdmission(supplied);
}

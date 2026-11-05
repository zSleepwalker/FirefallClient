using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleCapture(
    uint? FreshGlobalValue,
    uint? EntryEDX,
    uint? EntryECX,
    uint? LateEBX,
    uint? LateESI);

public enum CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
{
    MissingFreshGlobalValue,
    MissingEntryEDX,
    MissingEntryECX,
    MissingEntryEDXAndEntryECX,
    ParentHandoffNotExact,
    NonzeroTestResultBypassesChild,
    MissingBothLatePointers,
    MissingLateEBX,
    MissingLateESI,
    Completed
}

public enum CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind
{
    FreshGlobalRead,
    ChildGlobalWrite,
    ChildLocalStore
}

public readonly record struct CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffect(
    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    uint? Value,
    bool InstructionReached,
    bool Attempted,
    bool ValueCaptured,
    bool Committed);

public readonly record struct CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    ImmutableArray<byte> Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public enum CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationKind
{
    ZeroTestFallthrough,
    NonzeroTestBranch,
    ChildDirectJump
}

public readonly record struct CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationEvidence(
    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationKind Kind,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    bool BeforeTargetInstruction);

public sealed record CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan(
    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleCapture Capture,
    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonFreshMaskGatePlan Parent { get; init; } = null!;
    public CapacityFinalCalleeCommonSelfRingTailPlan? Child { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffect>.Empty;

    public CapacityFinalCalleeCommonFreshMaskGateOutcome ParentOutcome => Parent.Outcome;
    public uint? ParentSelectedDestinationRawFileOffset =>
        Parent.JnzSelectedDestinationRawFileOffset;
    public uint? ChildEntryEAX => Child?.OrEAX;
    public uint? ChildEntryEDX => Child?.OrEDX;
    public bool ChildReached => Child is not null;
    public bool ChildORReached => Child?.OrReached == true;
    public bool ChildFlagsOverrideParentTest => ChildORReached;
    public bool ParentTestFlagsPreserved =>
        !ChildFlagsOverrideParentTest
        && (Parent.TestFlags is null
            || FlagsAtStop == ConvertParentFlags(Parent.TestFlags.Value));
    public bool ExactZeroChildHandoff =>
        CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
            .HasExactZeroChildHandoff(Parent);
    public bool ExactNonzeroParentBypass =>
        ParentOutcome
            == CapacityFinalCalleeCommonFreshMaskGateOutcome
                .NonzeroTestResultToBounded93C0
        && Parent.ExactCleanBoundedDestination
        && Parent.ExactCleanExitAdmitted
        && Parent.FreshGlobalReadReached
        && Parent.FreshGlobalValueCaptured
        && Parent.ShiftExecuted
        && Parent.ShiftResult.HasValue
        && Parent.EntryEDXOperandConsumed
        && Parent.EntryECXOperandConsumed
        && !Parent.EntryEDXCaptureProvidedButUnused
        && !Parent.EntryECXCaptureProvidedButUnused
        && !Parent.UnreachedEntryEDXCaptureProvided
        && !Parent.UnreachedEntryECXCaptureProvided
        && Parent.TestReached
        && Parent.TestResult is uint testResult
        && testResult != 0
        && Parent.JnzReached
        && Parent.JnzTaken == true
        && Parent.JnzSelectedDestinationRawFileOffset
            == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                .NonzeroDestinationRawFileOffset
        && Parent.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                .NonzeroDestinationRawFileOffset
        && !LateEBXCaptureProvidedButUnused
        && !LateESICaptureProvidedButUnused;
    public bool ExactChildCompletion => IsExactChildCompletion();
    public bool ChildStoresAndJumpPreserveFlags =>
        Child is null || !Child.OrReached || Child.FlagsPreserved;
    public CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleFlags? ParentFlagsAtStop =>
        Parent.FlagsAtStop is { } flags ? ConvertParentFlags(flags) : null;
    public CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleFlags? ChildORFlags =>
        Child?.OrFlags is { } flags ? ConvertChildFlags(flags) : null;
    public CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleFlags? FlagsAtStop =>
        ChildORReached
            ? ChildORFlags
            : ParentFlagsAtStop;
    public bool FlagsPreserved =>
        ChildFlagsOverrideParentTest
            ? ChildStoresAndJumpPreserveFlags
            : ParentTestFlagsPreserved;
    public bool LateEBXCaptureProvidedButUnused =>
        !ChildReached && Capture.LateEBX.HasValue;
    public bool LateESICaptureProvidedButUnused =>
        !ChildReached && Capture.LateESI.HasValue;
    public bool ChildGlobalStoreReached => Child?.GlobalStoreReached == true;
    public bool ChildGlobalStoreCommitted =>
        Child?.Writes.Any(write =>
            write.RawFileOffset
                == CapacityFinalCalleeCommonSelfRingTailProjection.GlobalStoreRawFileOffset)
            == true;
    public bool FirstChildLocalStoreAttempted =>
        Child?.FirstLocalStoreAttempted == true;
    public bool FirstChildLocalStoreCommitted =>
        Child?.FirstLocalStoreCommitted == true;
    public ImmutableArray<CapacityFinalCalleeCommonSelfRingTailWrite> ChildWrites =>
        Child?.Writes ?? ImmutableArray<CapacityFinalCalleeCommonSelfRingTailWrite>.Empty;
    public bool ParentExactCleanDestination => Parent.ExactCleanBoundedDestination;
    public bool ChildJumpReached => Child?.JumpReached == true;
    public bool ChildJumpTargetEntered => false;
    public uint? ChildJumpTargetRawFileOffset => Child?.JumpTargetRawFileOffset;
    public uint? ChildJumpTargetVirtualAddress => Child?.JumpTargetVirtualAddress;
    public bool ExactBoundedExitAdmitted =>
        ExactNonzeroParentBypass || ExactChildCompletion;
    public uint StopAtRawFileOffsetExclusive =>
        Child?.StopAtRawFileOffsetExclusive ?? Parent.StopAtRawFileOffsetExclusive;

    private bool IsExactChildCompletion()
    {
        CapacityFinalCalleeCommonSelfRingTailPlan? child = Child;
        return ExactZeroChildHandoff
            && child is
            {
                Outcome: CapacityFinalCalleeCommonSelfRingTailOutcome.Completed
            }
            && child.OrReached
            && child.OrEAX == Parent.Capture.FreshGlobalValue
            && child.OrEDX == Parent.ShiftResult
            && child.Capture.EntryEAX == Parent.Capture.FreshGlobalValue
            && child.Capture.EntryEDX == Parent.ShiftResult
            && child.GlobalStoreReached
            && child.Writes.Select(write => write.RawFileOffset).SequenceEqual(new[]
            {
                CapacityFinalCalleeCommonSelfRingTailProjection.GlobalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.FirstLocalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.SecondLocalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.ThirdLocalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.FourthLocalStoreRawFileOffset
            })
            && child.FirstLocalStoreReached
            && child.SecondLocalStoreReached
            && child.ThirdLocalStoreReached
            && child.FourthLocalStoreReached
            && child.LateEBXCaptured
            && child.LateESICaptured
            && !child.UnreachedLateEBXCaptureProvided
            && !child.UnreachedLateESICaptureProvided
            && child.JumpReached
            && !child.JumpTargetEntered
            && child.JumpTargetRawFileOffset
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                    .ChildJumpTargetRawFileOffset
            && child.JumpTargetVirtualAddress
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                    .ChildJumpTargetVirtualAddress
            && child.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                    .ChildJumpTargetRawFileOffset
            && child.ExactBoundedExitAdmitted;
    }

    private static CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleFlags
        ConvertParentFlags(CapacityFinalCalleeCommonFreshMaskGateFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.Auxiliary,
            flags.Zero,
            flags.Sign,
            flags.Overflow);

    private static CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleFlags
        ConvertChildFlags(CapacityFinalCalleeCommonSelfRingTailFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.AuxiliaryCarry,
            flags.Zero,
            flags.Sign,
            flags.Overflow);
}

/// <summary>
/// Composes the committed fresh-mask gate with its zero-branch self-ring tail.
/// The parent gate and child tail are projected only through their direct
/// destinations; neither destination target is executed.
/// </summary>
public static class CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonFreshMaskGateProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        CapacityFinalCalleeCommonFreshMaskGateProjection.EntryVirtualAddress;
    public const uint ZeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonFreshMaskGateProjection.ZeroDestinationRawFileOffset;
    public const uint NonzeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonFreshMaskGateProjection.NonzeroDestinationRawFileOffset;
    public const uint ChildJumpTargetRawFileOffset =
        CapacityFinalCalleeCommonSelfRingTailProjection.JumpTargetRawFileOffset;
    public const uint ChildJumpTargetVirtualAddress =
        CapacityFinalCalleeCommonSelfRingTailProjection.JumpTargetVirtualAddress;
    public const uint FreshGlobalVirtualAddress =
        CapacityFinalCalleeCommonFreshMaskGateProjection.FreshGlobalVirtualAddress;

    public static ImmutableArray<CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructionEvidence();

    public static ImmutableArray<CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationEvidence>
        BoundedDestinations { get; } =
            ImmutableArray.Create(
                Destination(
                    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationKind
                        .ZeroTestFallthrough,
                    ZeroDestinationRawFileOffset),
                Destination(
                    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationKind
                        .NonzeroTestBranch,
                    NonzeroDestinationRawFileOffset),
                Destination(
                    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationKind
                        .ChildDirectJump,
                    ChildJumpTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static bool TryGetInstructionEvidence(
        uint rawFileOffset,
        out CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleInstructionEvidence evidence)
    {
        foreach (CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleInstructionEvidence
            instruction in Instructions)
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

    public static bool TryGetBoundedDestinationEvidence(
        uint rawFileOffset,
        out CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationEvidence evidence)
    {
        foreach (CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationEvidence
            destination in BoundedDestinations)
        {
            if (destination.RawFileOffset == rawFileOffset)
            {
                evidence = destination;
                return true;
            }
        }

        evidence = default;
        return false;
    }

    internal static bool HasExactZeroChildHandoff(
        CapacityFinalCalleeCommonFreshMaskGatePlan parent) =>
        parent.Outcome
            == CapacityFinalCalleeCommonFreshMaskGateOutcome
                .ZeroTestResultToBounded93AC
        && parent.ExactCleanBoundedDestination
        && parent.ExactCleanExitAdmitted
        && parent.FreshGlobalReadReached
        && parent.FreshGlobalValueCaptured
        && parent.ShiftExecuted
        && parent.ShiftResult.HasValue
        && parent.EntryEDXOperandConsumed
        && parent.EntryECXOperandConsumed
        && !parent.EntryEDXCaptureProvidedButUnused
        && !parent.EntryECXCaptureProvidedButUnused
        && !parent.UnreachedEntryEDXCaptureProvided
        && !parent.UnreachedEntryECXCaptureProvided
        && parent.TestReached
        && parent.TestResult == 0
        && parent.JnzReached
        && parent.JnzTaken == false
        && parent.JnzSelectedDestinationRawFileOffset == ZeroDestinationRawFileOffset
        && parent.StopAtRawFileOffsetExclusive == ZeroDestinationRawFileOffset;

    public static CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan Project(
        CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonFreshMaskGatePlan parent =
            CapacityFinalCalleeCommonFreshMaskGateProjection.Project(
                new(
                    capture.FreshGlobalValue,
                    capture.EntryEDX,
                    capture.EntryECX));

        if (parent.Outcome
            != CapacityFinalCalleeCommonFreshMaskGateOutcome
                .ZeroTestResultToBounded93AC)
        {
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome outcome =
                parent.Outcome switch
                {
                    CapacityFinalCalleeCommonFreshMaskGateOutcome
                        .MissingFreshGlobalValue =>
                            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                                .MissingFreshGlobalValue,
                    CapacityFinalCalleeCommonFreshMaskGateOutcome.MissingEntryEDX =>
                        CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                            .MissingEntryEDX,
                    CapacityFinalCalleeCommonFreshMaskGateOutcome.MissingEntryECX =>
                        CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                            .MissingEntryECX,
                    CapacityFinalCalleeCommonFreshMaskGateOutcome
                        .MissingEntryEDXAndEntryECX =>
                            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                                .MissingEntryEDXAndEntryECX,
                    CapacityFinalCalleeCommonFreshMaskGateOutcome
                        .NonzeroTestResultToBounded93C0 =>
                            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                                .NonzeroTestResultBypassesChild,
                    _ => throw new InvalidOperationException(
                        "The parent gate returned an unsupported outcome.")
                };
            return CreatePlan(capture, parent, null, outcome);
        }

        if (!HasExactZeroChildHandoff(parent))
        {
            return CreatePlan(
                capture,
                parent,
                null,
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .ParentHandoffNotExact);
        }

        CapacityFinalCalleeCommonSelfRingTailPlan child =
            CapacityFinalCalleeCommonSelfRingTailProjection.Project(
                new(
                    parent.Capture.FreshGlobalValue,
                    parent.ShiftResult,
                    capture.LateEBX,
                    capture.LateESI));
        CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome childOutcome =
            child.Outcome switch
            {
                CapacityFinalCalleeCommonSelfRingTailOutcome.MissingBothLatePointers =>
                    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                        .MissingBothLatePointers,
                CapacityFinalCalleeCommonSelfRingTailOutcome.MissingLateEBX =>
                    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                        .MissingLateEBX,
                CapacityFinalCalleeCommonSelfRingTailOutcome.MissingLateESI =>
                    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                        .MissingLateESI,
                CapacityFinalCalleeCommonSelfRingTailOutcome.Completed =>
                    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome.Completed,
                _ => throw new InvalidOperationException(
                    "The child tail returned an unsupported outcome.")
            };
        return CreatePlan(capture, parent, child, childOutcome);
    }

    private static CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan CreatePlan(
        CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleCapture capture,
        CapacityFinalCalleeCommonFreshMaskGatePlan parent,
        CapacityFinalCalleeCommonSelfRingTailPlan? child,
        CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome outcome)
    {
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffect>();
        CapacityFinalCalleeCommonFreshMaskGateReadAttempt read =
            parent.ReadAttempts[0];
        effects.Add(new(
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind
                .FreshGlobalRead,
            read.RawFileOffset,
            read.AbsoluteAddress,
            read.Value,
            read.ReadReached,
            read.ReadReached,
            read.ValueCaptured,
            Committed: false));

        if (child is not null)
        {
            if (child.GlobalStoreReached)
            {
                effects.Add(new(
                    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind
                        .ChildGlobalWrite,
                    CapacityFinalCalleeCommonSelfRingTailProjection.GlobalStoreRawFileOffset,
                    child.GlobalStoreAddress,
                    child.GlobalStoreValue,
                    InstructionReached: true,
                    Attempted: true,
                    ValueCaptured: child.GlobalStoreValue.HasValue,
                    Committed: child.GlobalStoreValue.HasValue));
            }

            if (child.FirstLocalStoreAttempted)
            {
                effects.Add(new(
                    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind
                        .ChildLocalStore,
                    CapacityFinalCalleeCommonSelfRingTailProjection
                        .FirstLocalStoreRawFileOffset,
                    child.FirstLocalStoreAddress,
                    child.FirstLocalStoreValue,
                    child.FirstLocalStoreReached,
                    Attempted: true,
                    ValueCaptured: child.FirstLocalStoreValue.HasValue,
                    Committed: child.FirstLocalStoreCommitted));
            }

            AddLocalStoreEffect(
                effects,
                CapacityFinalCalleeCommonSelfRingTailProjection
                    .SecondLocalStoreRawFileOffset,
                child.SecondLocalStoreReached,
                child.SecondLocalStoreAddress,
                child.SecondLocalStoreValue);
            AddLocalStoreEffect(
                effects,
                CapacityFinalCalleeCommonSelfRingTailProjection
                    .ThirdLocalStoreRawFileOffset,
                child.ThirdLocalStoreReached,
                child.ThirdLocalStoreAddress,
                child.ThirdLocalStoreValue);
            AddLocalStoreEffect(
                effects,
                CapacityFinalCalleeCommonSelfRingTailProjection
                    .FourthLocalStoreRawFileOffset,
                child.FourthLocalStoreReached,
                child.FourthLocalStoreAddress,
                child.FourthLocalStoreValue);
        }

        return new(capture, outcome)
        {
            Parent = parent,
            Child = child,
            OrderedMemoryEffects = effects.ToImmutable()
        };
    }

    private static void AddLocalStoreEffect(
        ImmutableArray<CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffect>.Builder
            effects,
        uint rawFileOffset,
        bool reached,
        uint? address,
        uint? value)
    {
        if (!reached)
        {
            return;
        }

        effects.Add(new(
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind.ChildLocalStore,
            rawFileOffset,
            address,
            value,
            InstructionReached: true,
            Attempted: true,
            ValueCaptured: value.HasValue,
            Committed: address.HasValue && value.HasValue));
    }

    private static ImmutableArray<CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleInstructionEvidence>
        BuildInstructionEvidence()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleInstructionEvidence>();

        foreach (CapacityFinalCalleeCommonFreshMaskGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonFreshMaskGateProjection.ProductionInstructionEvidence)
        {
            instructions.Add(new(
                instruction.Mnemonic,
                instruction.RawFileOffset,
                instruction.FlatIdaAddress,
                instruction.PEVirtualAddress,
                instruction.Bytes,
                instruction.BranchTargetRawFileOffset,
                instruction.BranchTargetPEVirtualAddress));
        }

        foreach (CapacityFinalCalleeCommonSelfRingTailInstructionEvidence instruction
            in CapacityFinalCalleeCommonSelfRingTailProjection.Instructions)
        {
            instructions.Add(new(
                instruction.Observation,
                instruction.RawFileOffset,
                instruction.FlatIdaAddress,
                instruction.VirtualAddress,
                ParseBytes(instruction.Bytes),
                instruction.BranchTargetRawFileOffset,
                instruction.BranchTargetVirtualAddress));
        }

        return instructions.ToImmutable();
    }

    private static ImmutableArray<byte> ParseBytes(string bytes)
    {
        var parsed = ImmutableArray.CreateBuilder<byte>();
        foreach (string part in bytes.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries))
        {
            parsed.Add(Convert.ToByte(part, 16));
        }

        return parsed.ToImmutable();
    }

    private static CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationEvidence
        Destination(
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationKind kind,
            uint rawFileOffset) =>
        new(
            kind,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            BeforeTargetInstruction: true);
}

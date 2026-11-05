using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonSelfRingCleanupLifecycleCapture(
    uint? FreshGlobalValue,
    uint? EntryEDX,
    uint? EntryECX,
    uint? LateEBX,
    uint? LateESI,
    uint? FreshCounterDword);

public enum CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
{
    MissingFreshGlobalValue,
    MissingEntryEDX,
    MissingEntryECX,
    MissingEntryEDXAndEntryECX,
    ParentHandoffNotExact,
    NonzeroMaskBypassesChild,
    MissingBothLatePointers,
    MissingLateEBX,
    MissingLateESI,
    MissingFreshCounterDword,
    CounterDecrementedToZero,
    CounterDecrementedNonzero
}

public enum CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
{
    FreshGlobalRead,
    ChildGlobalWrite,
    ChildLocalStore,
    CleanupCounterRead,
    CleanupCounterWrite
}

public readonly record struct CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffect(
    CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    uint? Value,
    bool InstructionReached,
    bool Attempted,
    bool ValueCaptured,
    bool Committed);

public readonly record struct CapacityFinalCalleeCommonSelfRingCleanupLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonSelfRingCleanupLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? MemoryOperandVirtualAddress = null,
    uint? ImmediateValue = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan(
    CapacityFinalCalleeCommonSelfRingCleanupLifecycleCapture Capture,
    CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan SelfRing { get; init; } =
        null!;
    public CapacityFinalCalleeCleanupCounterGatePlan? CleanupCounterGate { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffect>.Empty;

    public CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome SelfRingOutcome =>
        SelfRing.Outcome;
    public CapacityFinalCalleeCommonFreshMaskGateOutcome FreshMaskOutcome =>
        SelfRing.ParentOutcome;
    public CapacityFinalCalleeCommonSelfRingTailPlan? SelfRingTail => SelfRing.Child;
    public bool SelfRingReached => SelfRing.ChildReached;
    public bool SelfRingORReached => SelfRing.ChildORReached;
    public bool ChildJumpReached => SelfRing.ChildJumpReached;
    public bool Internal942FEntered =>
        CleanupCounterGate?.CounterReadReached == true;
    public bool CounterDecrementExecuted =>
        CleanupCounterGate is
        {
            CounterReadReached: true,
            FreshCounterDwordCaptured: true,
            CounterWriteReached: true
        };
    public bool ChildJumpTargetEntered => Internal942FEntered;
    public bool DestinationEntered => false;
    public bool CleanupJnzTargetEntered => false;
    public bool CleanupCounterCallReached => false;
    public bool CalleeEntered => CleanupCounterGate?.CalleeEntered == true;
    public bool StackAccessPerformed => false;
    public bool EcxImmediateReadAsMemory => false;
    public uint? CleanupEcxValue => CleanupCounterGate?.EcxValue;
    public bool FreshCounterDwordCaptured =>
        CleanupCounterGate?.FreshCounterDwordCaptured == true;
    public bool LateFreshCounterCaptureProvidedButUnused =>
        !Internal942FEntered && Capture.FreshCounterDword.HasValue;
    public bool LateEBXCaptureProvidedButUnused => SelfRing.LateEBXCaptureProvidedButUnused;
    public bool LateESICaptureProvidedButUnused => SelfRing.LateESICaptureProvidedButUnused;
    public bool ParentFlagsPreservedThroughSelfRing =>
        SelfRing.FlagsPreserved;
    public bool CleanupIncomingCarryFlagDerivedFromSelfRingOR =>
        SelfRingORReached
        && CleanupCounterGate?.Capture.IncomingCarryFlag
            == SelfRing.ChildORFlags?.Carry;
    public bool CleanupCarryPreserved =>
        CleanupCounterGate is { CounterReadReached: true }
        && CleanupCounterGate.CarryAtStop
            == CleanupCounterGate.Capture.IncomingCarryFlag;
    public CapacityFinalCalleeCommonSelfRingCleanupLifecycleFlags? FlagsAtStop =>
        CleanupCounterGate?.FlagsAfterDecrement is { } flags
            ? ConvertCleanupFlags(flags)
            : SelfRing.FlagsAtStop is { } parentFlags
                ? ConvertSelfRingFlags(parentFlags)
                : null;
    public uint? ParentSelectedDestinationRawFileOffset =>
        SelfRing.ParentSelectedDestinationRawFileOffset;
    public uint? CleanupSelectedDestinationRawFileOffset =>
        CleanupCounterGate?.SelectedDestinationRawFileOffset;
    public uint? SelectedDestinationRawFileOffset =>
        CleanupCounterGate is { } cleanupCounterGate
            ? cleanupCounterGate.SelectedDestinationRawFileOffset
            : SelfRing.ParentSelectedDestinationRawFileOffset;
    public uint StopAtRawFileOffsetExclusive =>
        CleanupCounterGate?.StopAtRawFileOffsetExclusive
        ?? SelfRing.StopAtRawFileOffsetExclusive;
    public bool AllSelfRingWritesCommitted =>
        SelfRing.ExactChildCompletion
        && SelfRing.ChildWrites.Length == 5
        && SelfRing.ChildWrites.Select(write => write.RawFileOffset).SequenceEqual(
            new[]
            {
                CapacityFinalCalleeCommonSelfRingTailProjection.GlobalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.FirstLocalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.SecondLocalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.ThirdLocalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.FourthLocalStoreRawFileOffset
            });
    public bool ExactNonzeroMaskBypass =>
        SelfRing.ExactNonzeroParentBypass
        && !LateEBXCaptureProvidedButUnused
        && !LateESICaptureProvidedButUnused
        && !LateFreshCounterCaptureProvidedButUnused
        && CleanupCounterGate is null;
    public bool ExactCounterBranchCompletion =>
        AllSelfRingWritesCommitted
        && SelfRing.ChildJumpReached
        && !SelfRing.ChildJumpTargetEntered
        && CleanupCounterGate is
        {
            ExactBoundedGateCompletion: true,
            CounterReadReached: true,
            FreshCounterDwordCaptured: true,
            CounterWriteReached: true,
            JnzReached: true,
            CallReached: false,
            CalleeEntered: false
        }
        && CleanupCounterGate.Capture.EntryValidated
        && Outcome is
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                .CounterDecrementedToZero
            or CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                .CounterDecrementedNonzero
        && CleanupIncomingCarryFlagDerivedFromSelfRingOR
        && CleanupCounterGate.Capture.IncomingCarryFlag == false
        && !LateEBXCaptureProvidedButUnused
        && !LateESICaptureProvidedButUnused
        && !LateFreshCounterCaptureProvidedButUnused
        && CleanupCounterGate.CounterAccesses.Length == 2
        && CleanupCounterGate.CounterAccesses.Select(access => access.Kind).SequenceEqual(
            new[]
            {
                CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                CapacityFinalCalleeCleanupCounterGateAccessKind.Write
            })
        && CleanupCounterGate.CounterAccesses[0].Address
            == CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress
        && CleanupCounterGate.CounterAccesses[0].ValueCaptured
        && CleanupCounterGate.CounterAccesses[0].Value == Capture.FreshCounterDword
        && CleanupCounterGate.CounterAccesses[1].Address
            == CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress
        && CleanupCounterGate.CounterAccesses[1].ValueCaptured
        && CleanupCounterGate.JnzTaken is bool jnzTaken
        && CleanupCounterGate.SelectedDestinationRawFileOffset
            == (jnzTaken
                ? CapacityFinalCalleeCleanupCounterGateProjection
                    .NonzeroBranchTargetRawFileOffset
                : CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset)
        && CleanupCounterGate.StopAtRawFileOffsetExclusive
            == (jnzTaken
                ? CapacityFinalCalleeCleanupCounterGateProjection
                    .NonzeroBranchTargetRawFileOffset
                : CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive)
        && (jnzTaken
            ? CleanupCounterGate.Outcome
                == CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero
                && !CleanupCounterGate.MovEcxReached
                && CleanupCounterGate.EcxValue is null
            : CleanupCounterGate.Outcome
                == CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero
                && CleanupCounterGate.MovEcxReached
                && CleanupCounterGate.EcxValue
                    == CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection
                        .CleanupZeroPathEcxValue)
        && !CleanupJnzTargetEntered
        && !CleanupCounterCallReached
        && !CalleeEntered
        && !StackAccessPerformed;
    public bool ExactBoundedExitAdmitted =>
        ExactNonzeroMaskBypass || ExactCounterBranchCompletion;

    private static CapacityFinalCalleeCommonSelfRingCleanupLifecycleFlags
        ConvertSelfRingFlags(
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.Auxiliary,
            flags.Zero,
            flags.Sign,
            flags.Overflow);

    private static CapacityFinalCalleeCommonSelfRingCleanupLifecycleFlags
        ConvertCleanupFlags(CapacityFinalCalleeCleanupCounterGateFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.AuxiliaryCarry,
            flags.Zero,
            flags.Sign,
            flags.Overflow);
}

/// <summary>
/// Joins the fresh-mask self-ring path to its later cleanup DEC/JNZ gate.
/// The cleanup counter is an independent late read after the ordered self-ring
/// stores; neither the JNZ destination nor the following call is entered.
/// </summary>
public static class CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.EntryRawFileOffset;
    public const uint CleanupCounterEntryRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset;
    public const uint CleanupCounterVirtualAddress =
        CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress;
    public const uint CleanupZeroPathEcxValue =
        CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue;
    public const uint CleanupNonzeroDestinationRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.NonzeroBranchTargetRawFileOffset;
    public const uint CleanupCallRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.CallRawFileOffset;
    public const uint SelfRingJumpTargetRawFileOffset =
        CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
            .ChildJumpTargetRawFileOffset;
    public const string CleanupCounterWindowBytes =
        CapacityFinalCalleeCleanupCounterGateProjection.CapturedWindowBytes;

    public static ImmutableArray<CapacityFinalCalleeCommonSelfRingCleanupLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructionEvidence();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static bool IsExactSelfRingHandoff(
        CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan selfRing)
    {
        uint[] expectedWrites =
        {
            CapacityFinalCalleeCommonSelfRingTailProjection.GlobalStoreRawFileOffset,
            CapacityFinalCalleeCommonSelfRingTailProjection.FirstLocalStoreRawFileOffset,
            CapacityFinalCalleeCommonSelfRingTailProjection.SecondLocalStoreRawFileOffset,
            CapacityFinalCalleeCommonSelfRingTailProjection.ThirdLocalStoreRawFileOffset,
            CapacityFinalCalleeCommonSelfRingTailProjection.FourthLocalStoreRawFileOffset
        };

        return selfRing.Outcome
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome.Completed
            && selfRing.ExactZeroChildHandoff
            && selfRing.ExactChildCompletion
            && selfRing.Child is { } child
            && child.Outcome == CapacityFinalCalleeCommonSelfRingTailOutcome.Completed
            && child.OrReached
            && child.OrFlags is { Carry: false }
            && child.Writes.Length == expectedWrites.Length
            && child.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(expectedWrites)
            && selfRing.ChildWrites.Select(write => write.RawFileOffset)
                .SequenceEqual(expectedWrites)
            && child.FirstLocalStoreReached
            && child.SecondLocalStoreReached
            && child.ThirdLocalStoreReached
            && child.FourthLocalStoreReached
            && child.LateEBXCaptured
            && child.LateESICaptured
            && !child.UnreachedLateEBXCaptureProvided
            && !child.UnreachedLateESICaptureProvided
            && !selfRing.LateEBXCaptureProvidedButUnused
            && !selfRing.LateESICaptureProvidedButUnused
            && !selfRing.Parent.EntryEDXCaptureProvidedButUnused
            && !selfRing.Parent.EntryECXCaptureProvidedButUnused
            && !selfRing.Parent.UnreachedEntryEDXCaptureProvided
            && !selfRing.Parent.UnreachedEntryECXCaptureProvided
            && selfRing.ChildJumpReached
            && child.JumpReached
            && !child.JumpTargetEntered
            && child.JumpTargetRawFileOffset
                == CleanupCounterEntryRawFileOffset
            && child.JumpTargetVirtualAddress
                == CapacityFinalCalleeCleanupCounterGateProjection.EntryVirtualAddress
            && selfRing.StopAtRawFileOffsetExclusive
                == CleanupCounterEntryRawFileOffset;
    }

    public static CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan Project(
        CapacityFinalCalleeCommonSelfRingCleanupLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan selfRing =
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Project(
                new(
                    capture.FreshGlobalValue,
                    capture.EntryEDX,
                    capture.EntryECX,
                    capture.LateEBX,
                    capture.LateESI));

        bool exactSelfRingHandoff = IsExactSelfRingHandoff(selfRing);
        CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounterGate = null;
        if (exactSelfRingHandoff)
        {
            cleanupCounterGate =
                CapacityFinalCalleeCleanupCounterGateProjection.Project(
                    new(
                        EntryValidated: true,
                        FreshCounterDword: capture.FreshCounterDword,
                        IncomingCarryFlag: selfRing.ChildORFlags?.Carry));
        }

        CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome outcome =
            MapOutcome(selfRing, cleanupCounterGate, exactSelfRingHandoff);

        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffect>();
        foreach (CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffect effect
            in selfRing.OrderedMemoryEffects)
        {
            effects.Add(new(
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind
                        .FreshGlobalRead =>
                            CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                                .FreshGlobalRead,
                    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind
                        .ChildGlobalWrite =>
                            CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                                .ChildGlobalWrite,
                    CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind
                        .ChildLocalStore =>
                            CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                                .ChildLocalStore,
                    _ => throw new InvalidOperationException(
                        "The self-ring lifecycle returned an unsupported effect.")
                },
                effect.RawFileOffset,
                effect.Address,
                effect.Value,
                effect.InstructionReached,
                effect.Attempted,
                effect.ValueCaptured,
                effect.Committed));
        }

        if (cleanupCounterGate is not null)
        {
            foreach (CapacityFinalCalleeCleanupCounterGateCounterAccess access
                in cleanupCounterGate.CounterAccesses)
            {
                bool isWrite =
                    access.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Write;
                effects.Add(new(
                    isWrite
                        ? CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                            .CleanupCounterWrite
                        : CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                            .CleanupCounterRead,
                    access.RawFileOffset,
                    access.Address,
                    access.Value,
                    cleanupCounterGate.ReachedInstructions.Contains(access.RawFileOffset),
                    Attempted: true,
                    access.ValueCaptured,
                    Committed: isWrite && access.ValueCaptured));
            }
        }

        return new(capture, outcome)
        {
            SelfRing = selfRing,
            CleanupCounterGate = cleanupCounterGate,
            OrderedMemoryEffects = effects.ToImmutable()
        };
    }

    private static CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome MapOutcome(
        CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan selfRing,
        CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounterGate,
        bool exactSelfRingHandoff)
    {
        if (selfRing.Outcome
            != CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome.Completed)
        {
            return selfRing.Outcome switch
            {
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .MissingFreshGlobalValue =>
                        CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                            .MissingFreshGlobalValue,
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .MissingEntryEDX =>
                        CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                            .MissingEntryEDX,
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .MissingEntryECX =>
                        CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                            .MissingEntryECX,
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .MissingEntryEDXAndEntryECX =>
                        CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                            .MissingEntryEDXAndEntryECX,
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .ParentHandoffNotExact =>
                        CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                            .ParentHandoffNotExact,
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .NonzeroTestResultBypassesChild =>
                        CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                            .NonzeroMaskBypassesChild,
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .MissingBothLatePointers =>
                        CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                            .MissingBothLatePointers,
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .MissingLateEBX =>
                        CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                            .MissingLateEBX,
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .MissingLateESI =>
                        CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                            .MissingLateESI,
                _ => throw new InvalidOperationException(
                    "The self-ring lifecycle returned an unsupported outcome.")
            };
        }

        if (!exactSelfRingHandoff || cleanupCounterGate is null)
        {
            return CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                .ParentHandoffNotExact;
        }

        return cleanupCounterGate?.Outcome switch
        {
            CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword =>
                CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                    .MissingFreshCounterDword,
            CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero =>
                CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                    .CounterDecrementedToZero,
            CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero =>
                CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                    .CounterDecrementedNonzero,
            _ => throw new InvalidOperationException(
                "The cleanup-counter gate returned an unsupported outcome.")
        };
    }

    private static ImmutableArray<
        CapacityFinalCalleeCommonSelfRingCleanupLifecycleInstructionEvidence>
        BuildInstructionEvidence()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonSelfRingCleanupLifecycleInstructionEvidence>();

        foreach (CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleInstructionEvidence instruction
            in CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Instructions)
        {
            instructions.Add(new(
                instruction.Observation,
                instruction.RawFileOffset,
                instruction.FlatIdaAddress,
                instruction.VirtualAddress,
                Convert.ToHexString(instruction.Bytes.AsSpan()),
                BranchTargetRawFileOffset: instruction.BranchTargetRawFileOffset,
                BranchTargetVirtualAddress: instruction.BranchTargetVirtualAddress));
        }

        foreach (CapacityFinalCalleeCleanupCounterGateInstructionEvidence instruction
            in CapacityFinalCalleeCleanupCounterGateProjection.Instructions)
        {
            instructions.Add(new(
                instruction.Observation,
                instruction.RawFileOffset,
                instruction.FlatIdaAddress,
                instruction.VirtualAddress,
                instruction.Bytes,
                instruction.PeVirtualAddressOperand,
                instruction.ImmediateValue,
                instruction.BranchTargetRawFileOffset,
                instruction.BranchTargetVirtualAddress));
        }

        return instructions.ToImmutable();
    }
}

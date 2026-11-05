namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeEaxBacklinkLifecycleCapture(
    uint? EntryEAX,
    uint? FreshGlobalValue,
    uint? ChildEntryECX,
    uint? ChildEntryEDX,
    uint? FreshEDXPlus10);

public enum CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome
{
    MissingBothParentCompareOperands,
    MissingParentEntryEAX,
    MissingParentFreshGlobalValue,
    ParentUnsignedBelow,
    ParentLifecycleIncomplete,
    MissingChildEntryECX,
    MissingChildEntryEDX,
    MissingChildFreshEDXPlus10,
    ChildZeroTo92A4,
    ChildNonzeroTo9292
}

public readonly record struct CapacityFinalCalleeLargeEaxBacklinkLifecycleFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public sealed record CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan(
    CapacityFinalCalleeLargeEaxBacklinkLifecycleCapture Capture,
    CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan ParentGate { get; init; } = null!;
    public CapacityFinalCalleeLargeBacklinkNextFieldGatePlan? ChildGate { get; init; }
    public bool ChildReached => ChildGate is not null;
    public bool NoChildReached => !ChildReached;
    public bool ParentEAXCaptured => Capture.EntryEAX.HasValue;
    public uint? RetainedParentEAX => Capture.EntryEAX;
    public bool ChildEAXDerivedFromParentEntry { get; init; }
    public uint? ChildEntryEAX { get; init; }
    public bool UnreachedChildECXCaptureProvided { get; init; }
    public bool UnreachedChildEDXCaptureProvided { get; init; }
    public bool UnreachedFreshEDXPlus10CaptureProvided { get; init; }
    public bool UnreachedChildLateCapturesProvided =>
        UnreachedChildECXCaptureProvided
        || UnreachedChildEDXCaptureProvided
        || UnreachedFreshEDXPlus10CaptureProvided;
    public bool UnusableFreshEDXPlus10CaptureProvided =>
        ChildGate?.UnusableFreshEDXPlus10CaptureProvided == true;
    public bool FirstStoreAttempted => ChildGate?.FirstStoreAttempted == true;
    public uint? FirstStoreAddress => ChildGate?.FirstStoreAddress;
    public bool FirstStoreAddressCaptured => ChildGate?.FirstStoreAddressCaptured == true;
    public uint? FirstStoreValue => ChildGate?.FirstStoreValue;
    public bool FirstStoreValueCaptured => ChildGate?.FirstStoreValueCaptured == true;
    public bool FirstStoreCommitted => ChildGate?.FirstStoreCommitted == true;
    public bool FreshReadReached => ChildGate?.FreshReadReached == true;
    public uint? FreshReadAddress => ChildGate?.FreshReadAddress;
    public bool FreshReadAddressCaptured => ChildGate?.FreshReadAddressCaptured == true;
    public uint? FreshEDXPlus10Value => ChildGate?.FreshECX;
    public bool TestReached => ChildGate?.TestReached == true;
    public CapacityFinalCalleeLargeBacklinkNextFieldGateFlags? TestFlags =>
        ChildGate?.TestFlags;
    public CapacityFinalCalleeLargeEaxBacklinkLifecycleFlags? ParentCompareFlags =>
        Convert(ParentGate.CompareFlags);
    public CapacityFinalCalleeLargeEaxBacklinkLifecycleFlags? FlagsAfterFirstStore =>
        FirstStoreAttempted ? Convert(ParentGate.CompareFlags) : null;
    public CapacityFinalCalleeLargeEaxBacklinkLifecycleFlags? FlagsAtStop =>
        ChildGate?.TestFlags is { } testFlags
            ? Convert(testFlags)
            : Convert(ParentGate.CompareFlags);
    public bool ParentCompareFlagsRemainUntilTest =>
        ChildReached
        && FirstStoreAttempted
        && ParentGate.CompareFlags is not null;
    public bool ExactBoundedCompletion { get; init; }
    public bool ChildDestinationEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }

    private static CapacityFinalCalleeLargeEaxBacklinkLifecycleFlags? Convert(
        CapacityFinalCalleeLargeEaxGlobalThresholdGateFlags? flags) =>
        flags is { } value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign,
                value.Overflow)
            : null;

    private static CapacityFinalCalleeLargeEaxBacklinkLifecycleFlags Convert(
        CapacityFinalCalleeLargeBacklinkNextFieldGateFlags flags) =>
        new(flags.Carry, flags.Parity, flags.AuxiliaryCarry, flags.Zero, flags.Sign,
            flags.Overflow);
}

/// <summary>
/// Composes the exact not-below global threshold path at 927C with the bounded
/// child backlink store and fresh EDX+10 TEST/JZ lifecycle.
/// </summary>
public static class CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.EntryRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.JzRawFileOffset + 2;
    public const uint ChildZeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.JzDestinationRawFileOffset;
    public const uint ChildNonzeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan Project(
        CapacityFinalCalleeLargeEaxBacklinkLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan parent =
            CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Project(new(
                capture.EntryEAX,
                capture.FreshGlobalValue));
        bool parentNotBelow =
            parent.Outcome
                == CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
                    .UnsignedNotBelowFallthrough
            && parent.ExactCleanAdmission
            && parent.CompareReached
            && parent.JBReached
            && parent.JBTaken == false
            && parent.JBSelectedDestinationRawFileOffset == ChildEntryRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset;

        CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan Finish(
            CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome outcome,
            uint stopAt,
            CapacityFinalCalleeLargeBacklinkNextFieldGatePlan? child = null,
            bool exactBoundedCompletion = false) =>
            new(capture, outcome)
            {
                ParentGate = parent,
                ChildGate = child,
                ChildEAXDerivedFromParentEntry =
                    child is not null && capture.EntryEAX.HasValue
                        && child.Capture.EntryEAX == capture.EntryEAX,
                ChildEntryEAX = child?.Capture.EntryEAX,
                UnreachedChildECXCaptureProvided =
                    !parentNotBelow && capture.ChildEntryECX.HasValue,
                UnreachedChildEDXCaptureProvided =
                    child is null
                        ? !parentNotBelow && capture.ChildEntryEDX.HasValue
                        : child.UnreachedEntryEDXCaptureProvided,
                UnreachedFreshEDXPlus10CaptureProvided =
                    child is null
                        ? !parentNotBelow && capture.FreshEDXPlus10.HasValue
                        : child.UnreachedFreshEDXPlus10CaptureProvided,
                ExactBoundedCompletion = exactBoundedCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (!parentNotBelow)
        {
            CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome parentOutcome =
                parent.Outcome switch
                {
                    CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
                        .MissingBothCompareOperands =>
                        CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome
                            .MissingBothParentCompareOperands,
                    CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome.MissingEntryEAX =>
                        CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome
                            .MissingParentEntryEAX,
                    CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
                        .MissingFreshGlobalValue =>
                        CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome
                            .MissingParentFreshGlobalValue,
                    CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
                        .UnsignedBelowBranchTarget =>
                        CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome.ParentUnsignedBelow,
                    _ => CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome
                        .ParentLifecycleIncomplete
                };
            return Finish(parentOutcome, parent.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan child =
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Project(new(
                EntryEAX: capture.EntryEAX,
                EntryECX: capture.ChildEntryECX,
                EntryEDX: capture.ChildEntryEDX,
                FreshEDXPlus10: capture.FreshEDXPlus10));
        CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome outcome = child.Outcome switch
        {
            CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.MissingEntryECXForFirstStore =>
                CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome.MissingChildEntryECX,
            CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.MissingEntryEDXForFreshRead =>
                CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome.MissingChildEntryEDX,
            CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.MissingFreshEDXPlus10 =>
                CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome.MissingChildFreshEDXPlus10,
            CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.ZeroTo92A4 =>
                CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome.ChildZeroTo92A4,
            CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.NonzeroTo9292 =>
                CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome.ChildNonzeroTo9292,
            _ => CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome.ParentLifecycleIncomplete
        };
        bool boundedCompletion =
            child.ExactCleanCompletion
            && ((child.Outcome == CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.ZeroTo92A4
                    && child.JzTaken == true
                    && child.JzSelectedDestinationRawFileOffset == ChildZeroDestinationRawFileOffset)
                || (child.Outcome == CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.NonzeroTo9292
                    && child.JzTaken == false
                    && child.JzSelectedDestinationRawFileOffset == ChildNonzeroDestinationRawFileOffset))
            && child.TestReached
            && child.JzReached
            && !child.UnreachedEntryEDXCaptureProvided
            && !child.UnreachedFreshEDXPlus10CaptureProvided
            && !child.UnusableFreshEDXPlus10CaptureProvided
            && child.StopAtRawFileOffsetExclusive
                == child.JzSelectedDestinationRawFileOffset
            && !child.TargetReadReached
            && !child.FollowingCMPReached;
        return Finish(outcome, child.StopAtRawFileOffsetExclusive, child, boundedCompletion);
    }
}

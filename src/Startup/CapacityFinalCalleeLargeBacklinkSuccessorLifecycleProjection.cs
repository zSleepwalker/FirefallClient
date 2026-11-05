using System;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeBacklinkSuccessorLifecycleCapture(
    uint? EntryEBP,
    uint? FreshFrameMinus14,
    uint? EntryEDX,
    uint? FreshBacklink,
    uint? ChildEntryEAX,
    uint? ChildFreshEaxPlus8);

public enum CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
{
    MissingUpstreamEntryEBP,
    MissingUpstreamFreshFrameMinus14,
    MissingUpstreamEntryEDX,
    MissingUpstreamFreshBacklink,
    UpstreamUnequalTo8E76,
    MissingChildEntryEAX,
    MissingChildFreshField,
    ChildUnequalTo8E76,
    MissingChildLateEBX,
    ChildEqualTo9229
}

public readonly record struct CapacityFinalCalleeLargeBacklinkSuccessorLifecycleFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public sealed record CapacityFinalCalleeLargeBacklinkSuccessorLifecyclePlan(
    CapacityFinalCalleeLargeBacklinkSuccessorLifecycleCapture Capture,
    CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeLargeFrameBacklinkGatePlan BacklinkGate { get; init; } = null!;
    public CapacityFinalCalleeLargeSuccessorWritesPlan? SuccessorWrites { get; init; }
    public bool ChildReached { get; init; }
    public bool UpstreamEDXRetainedForChild { get; init; }
    public uint? ChildEntryEDX { get; init; }
    public bool ChildLateEBXDerivedFromFreshFrameMinus14 { get; init; }
    public uint? ChildLateEBX { get; init; }
    public bool UnreachedChildEntryEAXCaptureProvided { get; init; }
    public bool UnreachedChildFreshFieldCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public CapacityFinalCalleeLargeBacklinkSuccessorLifecycleFlags? FlagsAtStop =>
        SuccessorWrites?.CompareFlags is { } childFlags
            ? Convert(childFlags)
            : BacklinkGate.CompareFlags is { } parentFlags
                ? Convert(parentFlags)
                : null;
    public uint StopAtRawFileOffsetExclusive { get; init; }

    private static CapacityFinalCalleeLargeBacklinkSuccessorLifecycleFlags Convert(
        CapacityFinalCalleeLargeSuccessorWritesCompareFlags flags) =>
        new(flags.Carry, flags.Parity, flags.AuxiliaryCarry, flags.Zero, flags.Sign,
            flags.Overflow);

    private static CapacityFinalCalleeLargeBacklinkSuccessorLifecycleFlags Convert(
        CapacityFinalCalleeLargeFrameBacklinkGateFlags flags) =>
        new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign,
            flags.Overflow);
}

/// <summary>
/// Composes the clean backlink equality at 91C5 with the child fresh-field
/// comparison and its equal-path stores, retaining only values consumed by each step.
/// </summary>
public static class CapacityFinalCalleeLargeBacklinkSuccessorLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeFrameBacklinkGateProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeLargeSuccessorWritesProjection.EntryRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeLargeSuccessorWritesProjection.BoundaryRawFileOffsetExclusive;
    public const uint UpstreamUnequalDestinationRawFileOffset =
        CapacityFinalCalleeLargeFrameBacklinkGateProjection.NonzeroDestinationRawFileOffset;
    public const uint ChildUnequalDestinationRawFileOffset =
        CapacityFinalCalleeLargeSuccessorWritesProjection.UnequalDestinationRawFileOffset;
    public const uint ChildEqualDestinationRawFileOffset =
        CapacityFinalCalleeLargeSuccessorWritesProjection.EqualDestinationRawFileOffset;

    public static CapacityFinalCalleeLargeBacklinkSuccessorLifecyclePlan Project(
        CapacityFinalCalleeLargeBacklinkSuccessorLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeFrameBacklinkGatePlan backlinkGate =
            CapacityFinalCalleeLargeFrameBacklinkGateProjection.Project(new(
                capture.EntryEBP,
                capture.FreshFrameMinus14,
                capture.EntryEDX,
                capture.FreshBacklink));

        bool upstreamEqual =
            backlinkGate.Outcome
                == CapacityFinalCalleeLargeFrameBacklinkGateOutcome.EqualToFallthrough
            && backlinkGate.ExactCleanExitAdmitted;
        upstreamEqual = upstreamEqual
            && backlinkGate.JnzReached && backlinkGate.JnzTaken == false
            && backlinkGate.JnzSelectedDestinationRawFileOffset == ChildEntryRawFileOffset
            && backlinkGate.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset;
        bool childEntryEAXUnreached = !upstreamEqual && capture.ChildEntryEAX.HasValue;
        bool childFreshFieldUnreached = !upstreamEqual && capture.ChildFreshEaxPlus8.HasValue;

        if (!upstreamEqual)
        {
            return new(capture, MapUpstreamOutcome(backlinkGate.Outcome))
            {
                BacklinkGate = backlinkGate,
                UnreachedChildEntryEAXCaptureProvided = childEntryEAXUnreached,
                UnreachedChildFreshFieldCaptureProvided = childFreshFieldUnreached,
                ExactCleanExitAdmitted = backlinkGate.ExactCleanExitAdmitted
                    && !childEntryEAXUnreached
                    && !childFreshFieldUnreached,
                StopAtRawFileOffsetExclusive = backlinkGate.StopAtRawFileOffsetExclusive
            };
        }

        bool childEAXKnown = capture.ChildEntryEAX.HasValue;
        uint? retainedEDX = childEAXKnown ? capture.EntryEDX : null;
        CapacityFinalCalleeLargeSuccessorWritesPlan child =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                capture.ChildEntryEAX,
                capture.ChildFreshEaxPlus8,
                retainedEDX,
                LateEntryEBX: null));

        bool derivedLateEBX = false;
        uint? childLateEBX = null;
        CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome outcome =
            MapChildOutcome(child.Outcome);

        if (child.CompareReached && child.JnzTaken == false
            && child.FirstStoreReached
            && backlinkGate.FrameMinus14Captured
            && backlinkGate.FrameMinus14 is uint freshFrameMinus14)
        {
            derivedLateEBX = true;
            childLateEBX = freshFrameMinus14;
            child = CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                capture.ChildEntryEAX,
                capture.ChildFreshEaxPlus8,
                retainedEDX,
                childLateEBX));
            outcome = CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                .ChildEqualTo9229;
        }

        return new(capture, outcome)
        {
            BacklinkGate = backlinkGate,
            SuccessorWrites = child,
            ChildReached = true,
            UpstreamEDXRetainedForChild = childEAXKnown && retainedEDX.HasValue,
            ChildEntryEDX = retainedEDX,
            ChildLateEBXDerivedFromFreshFrameMinus14 = derivedLateEBX,
            ChildLateEBX = childLateEBX,
            UnreachedChildEntryEAXCaptureProvided = false,
            UnreachedChildFreshFieldCaptureProvided =
                !child.FreshFieldReadReached && capture.ChildFreshEaxPlus8.HasValue,
            ExactCleanExitAdmitted = child.ExactCleanExitAdmitted,
            StopAtRawFileOffsetExclusive = child.StopAtRawFileOffsetExclusive
        };
    }

    private static CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
        MapUpstreamOutcome(CapacityFinalCalleeLargeFrameBacklinkGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingEntryEBP =>
                CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .MissingUpstreamEntryEBP,
            CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingFreshFrameMinus14 =>
                CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .MissingUpstreamFreshFrameMinus14,
            CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingEntryEDX =>
                CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .MissingUpstreamEntryEDX,
            CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingFreshBacklink =>
                CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .MissingUpstreamFreshBacklink,
            CapacityFinalCalleeLargeFrameBacklinkGateOutcome.UnequalToTarget =>
                CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .UpstreamUnequalTo8E76,
            _ => throw new InvalidOperationException(
                "Only a missing or unequal upstream gate can stop before the child.")
        };

    private static CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
        MapChildOutcome(CapacityFinalCalleeLargeSuccessorWritesOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingEntryEAX =>
                CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .MissingChildEntryEAX,
            CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingFreshField
                or CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingFreshFieldAndEntryEDX
                or CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingEntryEDX =>
                CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .MissingChildFreshField,
            CapacityFinalCalleeLargeSuccessorWritesOutcome.UnequalTo8E76 =>
                CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .ChildUnequalTo8E76,
            CapacityFinalCalleeLargeSuccessorWritesOutcome.EqualTo9229 =>
                CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .ChildEqualTo9229,
            CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingLateEntryEBXForEqualStores =>
                CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .MissingChildLateEBX,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome))
        };
}

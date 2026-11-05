using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryFallbackCounterCapture(
    NetworkDispatcherEntryFallbackImportReturnCapture ParentFallbackImportReturnCapture,
    uint? FreshPostFallbackEBX,
    uint? FreshCounterDword);

public enum NetworkDispatcherEntryFallbackCounterCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackCounterOutcome
{
    ParentFallbackImportReturnNotAccepted,
    MissingFreshPostFallbackEBX,
    MissingFreshCounterDword,
    Incremented
}

public enum NetworkDispatcherEntryFallbackCounterAccessKind
{
    Read,
    Write
}

public readonly record struct NetworkDispatcherEntryFallbackCounterAccess(
    uint RawFileOffset,
    NetworkDispatcherEntryFallbackCounterAccessKind Kind,
    uint Address,
    bool Attempted,
    bool Captured,
    uint? Value);

public readonly record struct NetworkDispatcherEntryFallbackCounterFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed record NetworkDispatcherEntryFallbackCounterPlan(
    NetworkDispatcherEntryFallbackImportReturnPlan ParentFallbackImportReturnPlan,
    NetworkDispatcherEntryFallbackCounterCapture Capture,
    NetworkDispatcherEntryFallbackCounterOutcome Outcome,
    NetworkDispatcherEntryFallbackCounterCaptureState FreshPostFallbackEBXCaptureState,
    NetworkDispatcherEntryFallbackCounterCaptureState FreshCounterDwordCaptureState,
    ImmutableArray<NetworkDispatcherEntryFallbackCounterAccess> CounterAccesses,
    ImmutableArray<uint> CompletedInstructionOffsets,
    uint EntryRawFileOffset,
    uint? StopAtRawFileOffsetExclusive,
    uint? FreshPostFallbackEBX,
    uint? CounterAddress,
    uint? CounterBefore,
    uint? CounterAfter,
    NetworkDispatcherEntryFallbackCounterFlags? FlagsAtStop,
    bool CounterIncrementReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    bool ProjectionEntered)
{
    public bool CounterIncrementProjected => CounterIncrementReached;
    public bool CarryPreserved => true;
    public bool PostCallESPForwarded => false;
    public bool PostCallEBXForwarded => false;
    public bool PostCallESIForwarded => false;
    public bool PostCallEBPForwarded => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
}

/// <summary>
/// Reprojects the fallback INC dword [EBX+30] and stops at BBDDF9.
/// </summary>
public static class NetworkDispatcherEntryFallbackCounterProjection
{
    public const uint EntryRawFileOffset = 0x00BBDDF6;
    public const uint IncrementRawFileOffset = EntryRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive = 0x00BBDDF9;
    public const uint CounterAddressDisplacement = 0x30;
    public const byte CounterDwordWidth = sizeof(uint);
    public const byte IncrementInstructionByteLength = 3;
    public const string IncrementInstructionBytes = "FF4330";
    public const string CapturedWindowBytes =
        "FF158442AC0185C0750753FF157842AC01FF4330";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackCounterPlan Project(
        NetworkDispatcherEntryFallbackCounterCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentFallbackImportReturnCapture);

        NetworkDispatcherEntryFallbackImportReturnPlan parent =
            NetworkDispatcherEntryFallbackImportReturnProjection.Project(
                capture.ParentFallbackImportReturnCapture);
        bool admitted =
            parent.ExactCleanCompletion
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.CounterIncrementBoundaryReached
            && parent.ParentFallbackPushPlan.ZeroFallthroughEntered
            && parent.ParentFallbackPushPlan.ExactCleanCompletion
            && parent.ParentFallbackPushPlan.Push is not null;

        if (!admitted)
        {
            return new(
                parent,
                capture,
                NetworkDispatcherEntryFallbackCounterOutcome
                    .ParentFallbackImportReturnNotAccepted,
                StateForUnreachedParent(capture.FreshPostFallbackEBX),
                StateForUnreachedParent(capture.FreshCounterDword),
                ImmutableArray<NetworkDispatcherEntryFallbackCounterAccess>.Empty,
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive ?? EntryRawFileOffset,
                parent.StopAtRawFileOffsetExclusive,
                null,
                null,
                null,
                null,
                parent.FlagsAtStop is { } parentFlags
                    ? new(parentFlags.Carry, parentFlags.Parity, parentFlags.Auxiliary,
                        parentFlags.Zero, parentFlags.Sign, parentFlags.Overflow)
                    : null,
                false,
                false,
                false,
                false);
        }

        if (capture.FreshPostFallbackEBX is not uint freshEBX)
        {
            return new(
                parent,
                capture,
                NetworkDispatcherEntryFallbackCounterOutcome.MissingFreshPostFallbackEBX,
                NetworkDispatcherEntryFallbackCounterCaptureState.Missing,
                StateForUnused(capture.FreshCounterDword),
                ImmutableArray<NetworkDispatcherEntryFallbackCounterAccess>.Empty,
                parent.CompletedInstructionOffsets,
                EntryRawFileOffset,
                IncrementRawFileOffset,
                null,
                null,
                null,
                null,
                UnknownFlags,
                false,
                false,
                false,
                true);
        }

        uint counterAddress = unchecked(freshEBX + CounterAddressDisplacement);
        if (capture.FreshCounterDword is not uint counterBefore)
        {
            var attemptedRead = new NetworkDispatcherEntryFallbackCounterAccess(
                IncrementRawFileOffset,
                NetworkDispatcherEntryFallbackCounterAccessKind.Read,
                counterAddress,
                Attempted: true,
                Captured: false,
                Value: null);
            return new(
                parent,
                capture,
                NetworkDispatcherEntryFallbackCounterOutcome.MissingFreshCounterDword,
                NetworkDispatcherEntryFallbackCounterCaptureState.Captured,
                NetworkDispatcherEntryFallbackCounterCaptureState.Missing,
                ImmutableArray.Create(attemptedRead),
                parent.CompletedInstructionOffsets,
                EntryRawFileOffset,
                IncrementRawFileOffset,
                freshEBX,
                counterAddress,
                null,
                null,
                UnknownFlags,
                false,
                false,
                false,
                true);
        }

        uint counterAfter = unchecked(counterBefore + 1);
        ImmutableArray<NetworkDispatcherEntryFallbackCounterAccess> accesses =
            ImmutableArray.Create(
                new NetworkDispatcherEntryFallbackCounterAccess(
                    IncrementRawFileOffset,
                    NetworkDispatcherEntryFallbackCounterAccessKind.Read,
                    counterAddress,
                    Attempted: true,
                    Captured: true,
                    Value: counterBefore),
                new NetworkDispatcherEntryFallbackCounterAccess(
                    IncrementRawFileOffset,
                    NetworkDispatcherEntryFallbackCounterAccessKind.Write,
                    counterAddress,
                    Attempted: true,
                    Captured: true,
                    Value: counterAfter));
        NetworkDispatcherEntryFallbackCounterFlags flags =
            new(
                Carry: parent.FlagsAtStop?.Carry,
                Parity: (BitOperations.PopCount((uint)(byte)counterAfter) & 1) == 0,
                Auxiliary: (counterBefore & 0x0F) == 0x0F,
                Zero: counterAfter == 0,
                Sign: (counterAfter & 0x80000000) != 0,
                Overflow: counterBefore == 0x7FFFFFFF);

        bool exactBoundedCompletion =
            parent.ExactCleanCompletion
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && BoundaryRawFileOffsetExclusive
                == IncrementRawFileOffset + IncrementInstructionByteLength;
        bool exactCleanCompletion =
            exactBoundedCompletion
            && capture.FreshPostFallbackEBX.HasValue
            && capture.FreshCounterDword.HasValue;

        return new(
            parent,
            capture,
            NetworkDispatcherEntryFallbackCounterOutcome.Incremented,
            NetworkDispatcherEntryFallbackCounterCaptureState.Captured,
            NetworkDispatcherEntryFallbackCounterCaptureState.Captured,
            accesses,
            parent.CompletedInstructionOffsets.Add(IncrementRawFileOffset),
            EntryRawFileOffset,
            BoundaryRawFileOffsetExclusive,
            freshEBX,
            counterAddress,
            counterBefore,
            counterAfter,
            flags,
            true,
            exactBoundedCompletion,
            exactCleanCompletion,
            true);
    }

    private static NetworkDispatcherEntryFallbackCounterFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);

    private static NetworkDispatcherEntryFallbackCounterCaptureState StateForUnused(
        uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackCounterCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackCounterCaptureState.Unreached;

    private static NetworkDispatcherEntryFallbackCounterCaptureState StateForUnreachedParent(
        uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackCounterCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackCounterCaptureState.Unreached;
}

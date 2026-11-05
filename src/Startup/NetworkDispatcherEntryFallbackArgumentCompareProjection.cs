using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryFallbackArgumentCompareCapture(
    NetworkDispatcherEntryFallbackCounterCapture ParentFallbackCounterCapture,
    uint? FreshPostFallbackEBP,
    byte? FreshArgumentByte);

public enum NetworkDispatcherEntryFallbackArgumentCompareCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackArgumentCompareOutcome
{
    ParentFallbackCounterNotAccepted,
    MissingFreshPostFallbackEBP,
    MissingFreshArgumentByte,
    Compared
}

public readonly record struct NetworkDispatcherEntryFallbackArgumentCompareRead(
    uint RawFileOffset,
    uint Address,
    byte Width,
    bool Attempted,
    bool Captured,
    byte? Value,
    bool Committed);

public sealed record NetworkDispatcherEntryFallbackArgumentComparePlan(
    NetworkDispatcherEntryFallbackCounterPlan ParentFallbackCounterPlan,
    NetworkDispatcherEntryFallbackArgumentCompareCapture Capture,
    NetworkDispatcherEntryFallbackArgumentCompareOutcome Outcome,
    NetworkDispatcherEntryFallbackArgumentCompareCaptureState FreshPostFallbackEBPCaptureState,
    NetworkDispatcherEntryFallbackArgumentCompareCaptureState FreshArgumentByteCaptureState,
    NetworkDispatcherEntryFallbackArgumentCompareRead? ArgumentRead,
    ImmutableArray<uint> CompletedInstructionOffsets,
    uint EntryRawFileOffset,
    uint? StopAtRawFileOffsetExclusive,
    uint? FreshPostFallbackEBP,
    uint? ArgumentAddress,
    byte? ArgumentByte,
    NetworkDispatcherLockGateFlags? CompareFlags,
    NetworkDispatcherLockGateFlags? FlagsAtStop,
    bool CompareReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    bool ProjectionEntered)
{
    public bool ArgumentCompareProjected => CompareReached;
    public bool BranchProjected => false;
    public bool EntryFrameClearProjected => false;
    public bool ReceiverByteClearProjected => false;
    public bool PostCallEBPForwarded => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryReadsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
}

/// <summary>
/// Reprojects the fresh byte comparison at BBDDF9 and stops before the
/// subsequent frame and receiver clears.
/// </summary>
public static class NetworkDispatcherEntryFallbackArgumentCompareProjection
{
    public const uint EntryRawFileOffset = 0x00BBDDF9;
    public const uint CompareRawFileOffset = EntryRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive = 0x00BBDDFD;
    public const uint ArgumentOffsetFromEBP = 0x08;
    public const byte ArgumentByteWidth = sizeof(byte);
    public const byte ComparedImmediate = 0;
    public const byte CompareInstructionByteLength = 4;
    public const string CompareInstructionBytes = "807D0800";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackArgumentComparePlan Project(
        NetworkDispatcherEntryFallbackArgumentCompareCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentFallbackCounterCapture);

        NetworkDispatcherEntryFallbackCounterPlan parent =
            NetworkDispatcherEntryFallbackCounterProjection.Project(
                capture.ParentFallbackCounterCapture);
        bool admitted =
            parent.ExactCleanCompletion
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.CounterIncrementReached
            && parent.Outcome == NetworkDispatcherEntryFallbackCounterOutcome.Incremented;

        if (!admitted)
        {
            return new(
                parent,
                capture,
                NetworkDispatcherEntryFallbackArgumentCompareOutcome
                    .ParentFallbackCounterNotAccepted,
                StateForUnreachedParent(capture.FreshPostFallbackEBP),
                StateForUnreachedParent(capture.FreshArgumentByte),
                null,
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive ?? EntryRawFileOffset,
                parent.StopAtRawFileOffsetExclusive,
                null,
                null,
                null,
                null,
                ToLockGateFlags(parent.FlagsAtStop),
                false,
                false,
                false,
                false);
        }

        if (capture.FreshPostFallbackEBP is not uint freshEBP)
        {
            return new(
                parent,
                capture,
                NetworkDispatcherEntryFallbackArgumentCompareOutcome
                    .MissingFreshPostFallbackEBP,
                NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Missing,
                StateForUnused(capture.FreshArgumentByte),
                null,
                parent.CompletedInstructionOffsets,
                EntryRawFileOffset,
                CompareRawFileOffset,
                null,
                null,
                null,
                null,
                ToLockGateFlags(parent.FlagsAtStop),
                false,
                false,
                false,
                true);
        }

        uint argumentAddress = unchecked(freshEBP + ArgumentOffsetFromEBP);
        if (capture.FreshArgumentByte is not byte argumentByte)
        {
            var attemptedRead = new NetworkDispatcherEntryFallbackArgumentCompareRead(
                CompareRawFileOffset,
                argumentAddress,
                ArgumentByteWidth,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false);
            return new(
                parent,
                capture,
                NetworkDispatcherEntryFallbackArgumentCompareOutcome
                    .MissingFreshArgumentByte,
                NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Captured,
                NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Missing,
                attemptedRead,
                parent.CompletedInstructionOffsets,
                EntryRawFileOffset,
                CompareRawFileOffset,
                freshEBP,
                argumentAddress,
                null,
                null,
                ToLockGateFlags(parent.FlagsAtStop),
                false,
                false,
                false,
                true);
        }

        var argumentRead = new NetworkDispatcherEntryFallbackArgumentCompareRead(
            CompareRawFileOffset,
            argumentAddress,
            ArgumentByteWidth,
            Attempted: true,
            Captured: true,
            Value: argumentByte,
            Committed: false);
        NetworkDispatcherLockGateFlags compareFlags = CompareByteToZeroFlags(argumentByte);
        ImmutableArray<uint> completedOffsets =
            parent.CompletedInstructionOffsets.Add(CompareRawFileOffset);
        bool exactBoundedCompletion =
            parent.ExactCleanCompletion
            && completedOffsets[^1] == CompareRawFileOffset
            && BoundaryRawFileOffsetExclusive
                == CompareRawFileOffset + CompareInstructionByteLength;
        bool exactCleanCompletion =
            exactBoundedCompletion
            && capture.FreshPostFallbackEBP.HasValue
            && capture.FreshArgumentByte.HasValue;

        return new(
            parent,
            capture,
            NetworkDispatcherEntryFallbackArgumentCompareOutcome.Compared,
            NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Captured,
            NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Captured,
            argumentRead,
            completedOffsets,
            EntryRawFileOffset,
            BoundaryRawFileOffsetExclusive,
            freshEBP,
            argumentAddress,
            argumentByte,
            compareFlags,
            compareFlags,
            true,
            exactBoundedCompletion,
            exactCleanCompletion,
            true);
    }

    private static NetworkDispatcherLockGateFlags? ToLockGateFlags(
        NetworkDispatcherEntryFallbackCounterFlags? flags) =>
        flags is { } value
            ? new(value.Carry, value.Parity, value.Auxiliary,
                value.Zero, value.Sign, value.Overflow)
            : null;

    private static NetworkDispatcherLockGateFlags CompareByteToZeroFlags(byte value)
    {
        byte result = unchecked((byte)(value - ComparedImmediate));
        return new(
            Carry: value < ComparedImmediate,
            Parity: (BitOperations.PopCount((uint)result) & 1) == 0,
            Auxiliary: ((value ^ ComparedImmediate ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80) != 0,
            Overflow: ((value ^ ComparedImmediate) & (value ^ result) & 0x80) != 0);
    }

    private static NetworkDispatcherEntryFallbackArgumentCompareCaptureState StateForUnused(
        byte? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackArgumentCompareCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Unreached;

    private static NetworkDispatcherEntryFallbackArgumentCompareCaptureState StateForUnreachedParent(
        uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackArgumentCompareCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Unreached;

    private static NetworkDispatcherEntryFallbackArgumentCompareCaptureState StateForUnreachedParent(
        byte? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackArgumentCompareCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Unreached;
}

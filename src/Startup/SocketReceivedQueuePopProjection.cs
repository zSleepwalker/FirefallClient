using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct SocketReceivedQueuePopCapture(
    uint ReceiverPointer,
    uint? OpaquePopResult);

public enum SocketReceivedQueuePopOutcome
{
    MissingPopResultAtCallBoundary,
    ZeroResultTakesZeroBranch,
    UnsignedAtLeast16TakesBranch,
    UnsignedBelow16FallsThrough
}

public readonly record struct SocketReceivedQueuePopPlan(
    SocketReceivedQueuePopCapture Capture,
    uint QueueReceiver,
    SocketReceivedQueuePopOutcome Outcome,
    uint? BranchTargetRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct SocketReceivedQueuePopInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null);

/// <summary>
/// Projects the queue-pop call result through the first conditional branch
/// after the caller's comparison with 0x10. The result is opaque status/control
/// data, not a dereferenced pointer. This does not model the callee, retry,
/// event delivery, or later handling of small values.
/// </summary>
public static class SocketReceivedQueuePopProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x00BD3EA5;
    public const uint EntryEndRawFileOffsetExclusive = 0x00BD3EBF;
    public const uint QueueReceiverOffset = 0x260;
    public const uint PopCallRawFileOffset = 0x00BD3EAB;
    public const uint PopCallReturnRawFileOffset = 0x00BD3EB0;
    public const uint PopCallTargetRawFileOffset = 0x012CE400;
    public const uint SavedPopResultRawFileOffset = 0x00BD3EB0;
    public const uint ZeroTestRawFileOffset = 0x00BD3EB2;
    public const uint ZeroBranchRawFileOffset = 0x00BD3EB4;
    public const uint ZeroBranchTargetRawFileOffset = 0x00BD3E60;
    public const uint CompareTenRawFileOffset = 0x00BD3EB6;
    public const uint UnsignedThresholdBranchRawFileOffset = 0x00BD3EB9;
    public const uint UnsignedThresholdBranchTargetRawFileOffset = 0x00BD40BF;
    public const uint UnsignedThresholdFallThroughRawFileOffset = 0x00BD3EBF;

    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EntryEndVirtualAddressExclusive =
        EntryEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint PopCallTargetVirtualAddress =
        PopCallTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedFunctionBytes =
        "8D 8E 60 02 00 00 E8 50 A5 6F 00 8B F8 85 FF 74 AA "
            + "83 FF 10 0F 83 00 02 00 00";

    public const string UnsignedThresholdBranchMnemonic = "JNB";
    public const string UnsignedThresholdCondition = "EDI >= 0x10 (unsigned)";
    public const bool SignedThresholdComparisonObserved = false;

    public static ReadOnlyCollection<SocketReceivedQueuePopInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new SocketReceivedQueuePopInstructionEvidence(
                "Form the queue receiver argument as caller this+0x260",
                EntryRawFileOffset,
                "8D 8E 60 02 00 00"),
            new SocketReceivedQueuePopInstructionEvidence(
                "Call the queue-pop helper",
                PopCallRawFileOffset,
                "E8 50 A5 6F 00",
                PopCallTargetRawFileOffset),
            new SocketReceivedQueuePopInstructionEvidence(
                "Save the opaque return value from EAX in EDI",
                SavedPopResultRawFileOffset,
                "8B F8"),
            new SocketReceivedQueuePopInstructionEvidence(
                "Test the saved result for zero",
                ZeroTestRawFileOffset,
                "85 FF"),
            new SocketReceivedQueuePopInstructionEvidence(
                "Branch on zero to raw EA 0xBD3E60",
                ZeroBranchRawFileOffset,
                "74 AA",
                ZeroBranchTargetRawFileOffset),
            new SocketReceivedQueuePopInstructionEvidence(
                "Compare the nonzero saved result with 0x10",
                CompareTenRawFileOffset,
                "83 FF 10"),
            new SocketReceivedQueuePopInstructionEvidence(
                "Unsigned JNB branches to raw EA 0xBD40BF when EDI is at least 0x10",
                UnsignedThresholdBranchRawFileOffset,
                "0F 83 00 02 00 00",
                UnsignedThresholdBranchTargetRawFileOffset)
        });

    public static SocketReceivedQueuePopPlan Project(
        SocketReceivedQueuePopCapture capture)
    {
        uint queueReceiver = unchecked(capture.ReceiverPointer + QueueReceiverOffset);

        if (capture.OpaquePopResult is not uint result)
        {
            return new SocketReceivedQueuePopPlan(
                capture,
                queueReceiver,
                SocketReceivedQueuePopOutcome.MissingPopResultAtCallBoundary,
                null,
                PopCallReturnRawFileOffset);
        }

        if (result == 0)
        {
            return new SocketReceivedQueuePopPlan(
                capture,
                queueReceiver,
                SocketReceivedQueuePopOutcome.ZeroResultTakesZeroBranch,
                ZeroBranchTargetRawFileOffset,
                ZeroBranchRawFileOffset + 2);
        }

        if (result >= 0x10)
        {
            return new SocketReceivedQueuePopPlan(
                capture,
                queueReceiver,
                SocketReceivedQueuePopOutcome.UnsignedAtLeast16TakesBranch,
                UnsignedThresholdBranchTargetRawFileOffset,
                UnsignedThresholdFallThroughRawFileOffset);
        }

        return new SocketReceivedQueuePopPlan(
            capture,
            queueReceiver,
            SocketReceivedQueuePopOutcome.UnsignedBelow16FallsThrough,
            null,
            UnsignedThresholdFallThroughRawFileOffset);
    }
}

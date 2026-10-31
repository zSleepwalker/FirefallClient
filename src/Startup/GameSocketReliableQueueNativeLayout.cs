namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Shared channel-1/channel-2 queue, acknowledgment, promotion and retry observations.
/// This does not implement native queue mutation, timers, or socket transmission.
/// </summary>
public static class GameSocketReliableQueueNativeLayout
{
    public const int AllocationSize = 0x5A8;
    public const uint ConstructorFileOffset = 0xBE3CE0;
    public const uint ActivateMethodFileOffset = 0xBE3F60;
    public const uint DatagramAssemblerMethodFileOffset = 0xBD4340;
    public const uint OutgoingAcknowledgmentBuilderFileOffset = 0xBE4F20;
    public const uint PromotePendingAndPrepareRetriesFileOffset = 0xBE49E0;
    public const uint PrepareRetryFileOffset = 0xBE4DE0;
    public const uint AcknowledgmentHandlerFileOffset = 0xBE43B0;
    public const int SocketPointerOffset = 0x00;
    public const int ChannelOffset = 0x04;
    public const int StatisticsPointerOffset = 0x08;
    public const int ActiveFlagOffset = 0x0C;
    public const int RetryIntervalOffset = 0x18;
    public const int FailureFlagOffset = 0x1C;
    public const int FailureCodeOffset = 0x20;
    public const int NextSequenceOffset = 0x28;
    public const int NextReceiveSequenceOffset = 0x2A;
    public const int PeerWindowSequenceOffset = 0x2C;
    public const int CumulativeAcknowledgmentSequenceOffset = 0x2E;
    public const int LocalCumulativeAcknowledgmentSequenceOffset = 0x30;
    public const int AcknowledgmentPendingFlagOffset = 0x32;
    public const int PendingQueueOffset = 0x38;
    public const int OutstandingTreeHeadOffset = 0x50;
    public const int OutstandingTreeCountOffset = 0x54;
    public const int PendingReceivedSequenceTrackerOffset = 0x47C;
    public const int PendingReceivedSequenceTrackerCursorOffset = 0x488;
    public const int PendingReceivedSequenceTrackerCountOffset = 0x48C;
    public const int SelectiveAcknowledgmentBitmapOffset = 0x490;
    public const int SelectiveAcknowledgmentBitmapHeadOffset = 0x590;
    public const int EmittedAcknowledgmentBaseChunkLength = 7;
    public const int MaximumEmittedAcknowledgmentChunkLength = byte.MaxValue;
    public const int MaximumEmittedSelectiveAcknowledgmentCount =
        (MaximumEmittedAcknowledgmentChunkLength - EmittedAcknowledgmentBaseChunkLength) / sizeof(ushort);
    public const int MaximumOutstandingCount = 512;
    public const int SequenceWindowSize = 256;
    public const uint MinimumRetryIntervalMilliseconds = 250;
    public const uint InitialRetryIntervalMilliseconds = 1000;
    public const uint MaximumRetryIntervalMilliseconds = 2500;
    public const int RetryAttemptFailureThreshold = 16;
    public const uint FullWindowTimeoutMilliseconds = 15000;
    public const int SocketFailureSuppressionFlagOffset = 0x100;

    public static uint NextRetryIntervalMilliseconds(uint currentInterval)
    {
        if (currentInterval > MaximumRetryIntervalMilliseconds)
            throw new ArgumentOutOfRangeException(nameof(currentInterval),
                "Only the observed nonnegative, capped interval range is reconstructed.");
        return Math.Clamp(currentInterval + (currentInterval >> 1),
            MinimumRetryIntervalMilliseconds, MaximumRetryIntervalMilliseconds);
    }

    public static bool IsWithinSendWindow(ushort sequence, ushort peerWindowSequence) =>
        unchecked((ushort)(sequence - peerWindowSequence)) < SequenceWindowSize;
}

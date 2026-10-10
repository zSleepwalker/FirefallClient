namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixPvpQueueUpdateSelection(bool CopyRecords, bool ClearRecords, bool QueuedProperty);

/// <summary>Msg42's native log identifies PvP queue status; record field identities remain opaque.</summary>
public static class MatrixPvpQueueNativeLayout
{
    public const uint MessageCode = 42;
    public const uint HandlerFileOffset = 0x4918F0;
    public const uint ConsumerFileOffset = 0x85D560;
    public const uint RuntimeMessageId = 289;
    public const string QueuedPropertyName = "queued";
    public const int RequiredDownstreamPointerOffset = 0x910;
    public const int DownstreamMember08Offset = 8;
    public const int Member08ConsumerPointerOffset = 0x0C;
    public const int ConsumerComparedWordOffset = 0x78;
    public const uint AcceptAnyComparedWord = 9;
    public const int NativeRecordStride = 16;
    public const uint FoundMatchHandlerFileOffset = 0x4919B0;
    public const uint FoundMatchConsumerFileOffset = 0x85DB10;

    public static bool SelectHandlerConsumer(bool hasDownstream, bool hasMember08, bool hasConsumer)
    {
        if (!hasDownstream)
            throw new ArgumentException("Native msg42 requires connection +910.", nameof(hasDownstream));
        return hasMember08 && hasConsumer;
    }

    public static MatrixPvpQueueUpdateSelection SelectUpdate(bool inputHasRecords, uint wordAt34, uint consumerWordAt78)
    {
        bool copy = inputHasRecords && (wordAt34 == AcceptAnyComparedWord || wordAt34 == consumerWordAt78);
        return new(copy, !copy, inputHasRecords);
    }

    public static bool SelectFoundMatchNotificationSuppression(byte previousState, byte incomingState) =>
        previousState is 2 or 3 && previousState == incomingState;

    /// <summary>Ordinary update branch only; the matchmaker-zero reset branch is separate.</summary>
    public static bool SelectFoundMatchDeadlineRefresh(byte previousState, byte incomingState, uint currentDeadline) =>
        incomingState is 1 or 2 or 3 && (previousState != incomingState || currentDeadline == 0);

    public static uint ComputeFoundMatchDeadline(uint millisecondTick, ushort wordAt40) =>
        unchecked(millisecondTick + 1000u * wordAt40);
}

public static class MatrixMessages41And43NativeLayout
{
    public const uint Message41HandlerFileOffset = 0x4914A0;
    public const int Message41ScalarDestinationOffset = 0xA48;
    public const int Message41OptionalDestinationOffset = 0xA58;
    public const int Message41NativeEntryStride = 16;
    public const int Message41WireEntrySize = 12;
    public const int Message41TransportByteOffset = 0x100;
    public const uint Message43HandlerFileOffset = 0x491960;
    public const int Message43RequiredDownstreamPointerOffset = 0x910;

    public static byte SelectMessage41TransportByte(byte byteAt34) => (byte)(byteAt34 & 1);
}

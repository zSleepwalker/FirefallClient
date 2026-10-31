namespace Firefall.Reconstruction.Startup;

public enum GameSocketReceivedFragmentRoute
{
    DeferredControl,
    ReliableChannel1,
    ReliableChannel2,
    UnreliableChannel3,
    ReleaseWithoutDelivery
}

public enum GameSocketReceivedQueueEvent
{
    CloseCompletion = 1,
    PumpCloseRequest = 2,
    SenderBindingSucceeded = 3
}

public readonly record struct GameSocketOwnerDelivery(int HandlerVirtualOffset, int? DeliveryFlag);

/// <summary>
/// Queued receive event/fragment routing and the fragment-record field offsets.
/// Packet assembly and callback lifetimes remain native observations.
/// </summary>
public static class GameSocketQueuedReceiveNativeLayout
{
    public const uint ParseNonzeroContextChunksMethodFileOffset = 0xBD3140;
    public const uint ConsumeMethodFileOffset = 0xBD3E50;
    public const uint ChannelJumpTableFileOffset = 0xBD422C;
    public const uint ReliableReceiveMethodFileOffset = 0xBE51F0;
    public const uint UnreliableReceiveMethodFileOffset = 0xBE5A40;
    public const uint DeliverOwnerCallbacksMethodFileOffset = 0xBD5170;
    public const int DeliverOwnerCallbacksVirtualOffset = 0x14;
    public const int ReceivedQueueOffset = 0x260;
    public const int QueueEventPointerThreshold = 0x10;
    public const int FragmentHeaderWordOffset = 0;
    public const int FragmentSequenceWordOffset = 2;
    public const int FragmentBodySliceOffsetFieldOffset = 4;
    public const int FragmentBodyObjectOffset = 8;
    public const int FragmentArrivalTickOffset = 0x18;
    public const int Channel0BodySliceOffset = 2;
    public const int DataChannelBodySliceOffset = 4;
    public const int DataChannelSequencePrefixSize = 4;
    public const int Channel1QueueOffset = 0xD6C;
    public const int Channel2QueueOffset = 0xD70;
    public const int Channel3QueueOffset = 0xD74;
    public const int SuppressionFlagOffset = 0xD95;
    public const int DisconnectHandlerVirtualOffset = 0x0C;
    public const uint TryEnableConnectedSendMethodFileOffset = 0xBD4A20;
    public const int ConnectedSendFlagOffset = 0x259;
    public const uint CheckReliableQueueFailuresMethodFileOffset = 0xBD5080;

    public static GameSocketReceivedFragmentRoute SelectRoute(
        NativeGameSocketState state, int channel, bool suppressed)
    {
        if (state is not (NativeGameSocketState.Connecting or NativeGameSocketState.Connected
            or NativeGameSocketState.Destroyed))
            throw new ArgumentOutOfRangeException(nameof(state),
                "The native queued-fragment path asserts outside states 3, 5 and 8.");
        if (channel is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(channel));
        if (suppressed || (state == NativeGameSocketState.Connecting && channel != 0))
            return GameSocketReceivedFragmentRoute.ReleaseWithoutDelivery;
        return channel switch
        {
            0 => GameSocketReceivedFragmentRoute.DeferredControl,
            1 => GameSocketReceivedFragmentRoute.ReliableChannel1,
            2 => GameSocketReceivedFragmentRoute.ReliableChannel2,
            3 => GameSocketReceivedFragmentRoute.UnreliableChannel3,
            _ => throw new ArgumentOutOfRangeException(nameof(channel))
        };
    }

    public static GameSocketOwnerDelivery GetOwnerDelivery(int channel) => channel switch
    {
        1 => new(4, null),
        2 => new(8, 1),
        3 => new(8, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(channel),
            "Only data channels 1, 2 and 3 reach these owner callbacks.")
    };
}

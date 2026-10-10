namespace Firefall.Reconstruction.Startup;

public sealed record MatrixQueueStatusPayload(uint QueuePositionBits, uint QueueLengthBits) : MatrixScalarUpdatePayload;

public sealed record MatrixQueueStatusRuntimeNotification(
    uint RuntimeMessageId, uint QueuePositionBits, uint QueueLengthBits);

public static class MatrixQueueStatusNativeLayout
{
    public const ushort MessageCode = 62;
    public const uint HandlerFileOffset = 0x491140;
    public const uint FactoryCaseFileOffset = 0x33DF77;
    public const uint ConstructorFileOffset = 0x33C310;
    public const uint BodyReaderFileOffset = 0x34B610;
    public const int DescriptorAllocationSize = 0x30;
    public const int PacketSize = 9;
    public const uint RuntimeMessageId = 295;
    public const string PositionPropertyName = "queue_position";
    public const string LengthPropertyName = "queue_length";
    public const int RuntimeReceiverVirtualOffset = 0xAC;
    public const uint RuntimeNumericPropertyBuilderFileOffset = 0xC24210;
    public const uint RuntimeNumericVariantTag = 1;

    /// <summary>Preserves the DWORD bits passed to runtime properties; delivery is not simulated.</summary>
    public static MatrixQueueStatusRuntimeNotification CaptureNotification(MatrixQueueStatusPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return new(RuntimeMessageId, payload.QueuePositionBits, payload.QueueLengthBits);
    }
}

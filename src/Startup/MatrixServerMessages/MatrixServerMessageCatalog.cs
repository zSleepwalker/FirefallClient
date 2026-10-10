namespace Firefall.Reconstruction.Startup;

public sealed record MatrixServerMessageEntry(
    uint DecodedMessageId, string NativeLogName, int ReceiverVirtualOffset, uint HandlerFileOffset)
{
    public MatrixServerMessageNativeType OriginalType => MatrixServerMessageNativeTypes.Get(DecodedMessageId);
}

/// <summary>
/// Descriptor IDs and secondary-interface routes, not a wire decoder.
/// Native logs provide only numeric msg-N names for these entries.
/// </summary>
public static class MatrixServerMessageCatalog
{
    public const uint DispatchMethodFileOffset = 0x33CE40;
    public const uint DispatchJumpTableFileOffset = 0x33D424;
    public const int DescriptorMessageIdOffset = 0x10;
    public const int ReceiverSubobjectOffset = 4;
    public const uint ReceiverVtableVirtualAddress = 0x1C6423C;
    public const uint ReceiverVtableFileOffset = 0x1862E3C;
    public const uint FirstMessageId = 34;
    public const uint LastMessageId = 68;

    public static IReadOnlyList<MatrixServerMessageEntry> Entries { get; } = Array.AsReadOnly(
        new uint[]
        {
            0x490AA0, 0x490AD0, 0x491400, 0x490C10, 0x491380, 0x4913A0, 0x491230,
            0x4914A0, 0x4918F0, 0x491960, 0x4919B0, 0x491A40, 0x491A90, 0x491BA0,
            0x491C60, 0x491C80, 0x491CF0, 0x491D30, 0x491D90, 0x491E10, 0x491E60,
            0x491EC0, 0x496460, 0x491F10, 0x496680, 0x496690, 0x4967B0, 0x4967F0,
            0x491140, 0x491F90, 0x4920B0, 0x492400, 0x4924D0, 0x492550, 0x4925F0
        }.Select((handler, i) => new MatrixServerMessageEntry(
            FirstMessageId + (uint)i, $"msg-{FirstMessageId + (uint)i}", 4 * (i + 1), handler)).ToArray());

    public static MatrixServerMessageEntry Get(uint decodedMessageId)
    {
        if (decodedMessageId is < FirstMessageId or > LastMessageId)
            throw new ArgumentOutOfRangeException(nameof(decodedMessageId),
                "The native dispatcher asserts for IDs outside 34..68.");
        return Entries[(int)(decodedMessageId - FirstMessageId)];
    }
}

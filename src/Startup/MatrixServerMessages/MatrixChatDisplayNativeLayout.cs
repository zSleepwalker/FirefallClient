namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixChatDisplayGateSnapshot(
    bool CommunityEnabled, bool ModeByte14, bool DebugEnabled);

public sealed record MatrixChatDisplayNotification(
    ReadOnlyMemory<byte>? ChannelBytes, ReadOnlyMemory<byte> AuthorBytes,
    ReadOnlyMemory<byte> TextBytes, ulong? AuthorId,
    bool Dev, bool Mentor, bool Ranger, bool Publisher, bool UseEncounterIcon)
{
    public uint RuntimeMessageId => 335;
}

public static class MatrixChatDisplayNativeLayout
{
    private static readonly ReadOnlyMemory<byte> ZoneChannelBytes = "zone"u8.ToArray();
    private static readonly ReadOnlyMemory<byte> BroadcastChannelBytes = "broadcast"u8.ToArray();

    public const uint ReceiveZoneHandlerFileOffset = 0x492550;
    public const uint SelectChannelLabelFileOffset = 0x80CC90;
    public const uint ParseChannelLabelFileOffset = 0x80CD70;
    public const uint DisplayFileOffset = 0x6E3460;
    public const uint PostNotificationConsumerFileOffset = 0x6F46F0;
    public const uint CommunityReceiverVirtualAddress = 0x1E5E42C;
    public const uint DebugReceiverVirtualAddress = 0x1E5E4BC;
    public const int ReceiverEnabledByteOffset = 0x40;
    public const int ModeGateByteOffset = 0x14;

    /// <summary>Input category is the native parser result; null means it did not recognize the channel.</summary>
    public static bool SelectDispatch(byte? nativeParsedChannel, MatrixChatDisplayGateSnapshot gates) =>
        nativeParsedChannel switch
        {
            2 => gates.CommunityEnabled,
            4 => gates.ModeByte14,
            11 => gates.DebugEnabled,
            _ => true
        };

    public static MatrixChatDisplayNotification CaptureZoneNotification(MatrixZoneChatPayload payload) =>
        Capture(ZoneChannelBytes, payload.AuthorNameBytes, payload.TextBytes, payload.AuthorId, payload.RawFlags);

    public static MatrixChatDisplayNotification CaptureAnnouncement(MatrixAnnouncementPayload payload)
    {
        if (!MatrixAnnouncementNativeLayout.SelectDisplay(payload.TypeBytesAt28.Span, true))
            throw new ArgumentException("Only the native sysmsg announcement branch selects broadcast display.", nameof(payload));
        return Capture(BroadcastChannelBytes, ReadOnlyMemory<byte>.Empty, payload.DataBytesAt2C, 0, 0);
    }

    public static MatrixChatDisplayNotification Capture(ReadOnlyMemory<byte> channelBytes,
        ReadOnlyMemory<byte> authorBytes, ReadOnlyMemory<byte> textBytes, ulong authorId, uint flags)
    {
        ReadOnlyMemory<byte>? channel = null;
        if (!channelBytes.IsEmpty)
            channel = channelBytes;
        return new(channel, authorBytes, textBytes, authorId == 0 ? null : authorId,
            (flags & 1) != 0, (flags & 2) != 0, (flags & 4) != 0, (flags & 8) != 0, (flags & 0x40) != 0);
    }
}

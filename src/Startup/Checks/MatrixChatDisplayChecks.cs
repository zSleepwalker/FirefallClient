using Firefall.Reconstruction.Startup;

internal static class MatrixChatDisplayChecks
{
    public static void Run(Action<string, bool> check)
    {
        var payload = new MatrixZoneChatPayload(ulong.MaxValue, new byte[] { 0x80 }, new byte[] { 0xFF }, uint.MaxValue);
        var zone = MatrixChatDisplayNativeLayout.CaptureZoneNotification(payload);
        check("Server67 fixed native channel1 is zone; author/text/ID and five flag bits reach runtime335",
            zone.RuntimeMessageId == 335 && zone.ChannelBytes is { } channel && channel.Span.SequenceEqual("zone"u8)
            && zone.AuthorBytes.Span.SequenceEqual(new byte[] { 0x80 }) && zone.TextBytes.Span.SequenceEqual(new byte[] { 0xFF })
            && zone.AuthorId == ulong.MaxValue && zone is { Dev: true, Mentor: true, Ranger: true, Publisher: true, UseEncounterIcon: true });
        check("Chat335 flags use only bits0/1/2/3/6; higher DWORD bits are not additional roles",
            Enumerable.Range(0, 256).All(flags =>
            {
                var n = MatrixChatDisplayNativeLayout.CaptureZoneNotification(payload with { RawFlags = (uint)flags });
                var high = MatrixChatDisplayNativeLayout.CaptureZoneNotification(payload with { RawFlags = (uint)flags | 0xFFFFFF00 });
                return n == high && n.Dev == ((flags & 1) != 0) && n.Mentor == ((flags & 2) != 0)
                    && n.Ranger == ((flags & 4) != 0) && n.Publisher == ((flags & 8) != 0)
                    && n.UseEncounterIcon == ((flags & 0x40) != 0);
            }));
        var empty = MatrixChatDisplayNativeLayout.Capture(default, default, default, 0, 0);
        check("Chat335 omits empty channel and zero author ID, while retaining empty author/text properties",
            empty.ChannelBytes is null && empty.AuthorId is null
            && empty.AuthorBytes.IsEmpty && empty.TextBytes.IsEmpty
            && empty is { Dev: false, Mentor: false, Ranger: false, Publisher: false, UseEncounterIcon: false });
        var announcement = MatrixChatDisplayNativeLayout.CaptureAnnouncement(new("sysmsg"u8.ToArray(), new byte[] { 0x80 }));
        check("Announcement36 maps to broadcast/empty author/omitted ID/zero roles without text decoding",
            announcement.ChannelBytes is { } broadcast && broadcast.Span.SequenceEqual("broadcast"u8)
            && announcement.AuthorBytes.IsEmpty && announcement.AuthorId is null
            && announcement.TextBytes.Span.SequenceEqual(new byte[] { 0x80 })
            && !announcement.Dev && !announcement.UseEncounterIcon);
        bool invalidRejected = false;
        try { MatrixChatDisplayNativeLayout.CaptureAnnouncement(new("unknown"u8.ToArray(), default)); }
        catch (ArgumentException) { invalidRejected = true; }
        check("Unselected announcement type is rejected rather than fabricated as broadcast", invalidRejected);
        check("Chat display only known native categories2/4/11 are suppressed by their distinct gates",
            Enumerable.Range(0, 256).All(channelId =>
                MatrixChatDisplayNativeLayout.SelectDispatch((byte)channelId, new(false, false, false))
                    == (channelId is not (2 or 4 or 11)))
            && MatrixChatDisplayNativeLayout.SelectDispatch(null, new(false, false, false))
            && MatrixChatDisplayNativeLayout.SelectDispatch(2, new(true, false, false))
            && MatrixChatDisplayNativeLayout.SelectDispatch(4, new(false, true, false))
            && MatrixChatDisplayNativeLayout.SelectDispatch(11, new(false, false, true)));
    }
}

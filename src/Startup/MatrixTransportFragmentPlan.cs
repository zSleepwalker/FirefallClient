namespace Firefall.Reconstruction.Startup;

/// <summary>
/// A queued native fragment, before sequence assignment or final UDP framing.
/// The original application buffer is retained separately; PayloadOffset selects its slice.
/// </summary>
public sealed record MatrixTransportFragment(
    ushort InitialHeaderWord,
    int PayloadOffset,
    int PayloadLength,
    bool MoreFragments);

public static class MatrixTransportFragmentPlan
{
    public const uint SubmissionMethodFileOffset = 0xBD3B20;
    public const uint SubmissionJumpTableFileOffset = 0xBD3E3C;
    public const uint HeaderConstructorFileOffset = 0xBCFE30;
    public const uint QueueFragmentMethodFileOffset = 0xBE47C0;
    public const int MaximumSegmentSizeOffset = 0x104;
    public const int DefaultMaximumSegmentSize = 0x56C;
    public const int Channel1QueuePointerOffset = 0xD6C;
    public const int Channel2QueuePointerOffset = 0xD70;
    public const int Channel3QueuePointerOffset = 0xD74;
    public const int SuppressSubmissionFlagOffset = 0xD95;
    public const int FragmentHeaderSize = 4;
    public const int MaximumQueuedFragmentSize = 0x570;
    public const int MaximumPayloadPerFragment = MaximumQueuedFragmentSize - FragmentHeaderSize;
    public const ushort MoreFragmentsBit = 0x0800;

    /// <summary>
    /// Reconstructs only the shared channel-1/channel-2 split path. Channel 3's
    /// unreliable promotion and suppression/state gates belong to native submission.
    /// These descriptors are not datagrams; their initial sequence word is zero.
    /// </summary>
    public static IReadOnlyList<MatrixTransportFragment> Create(
        int channel, int payloadLength, int maximumSegmentSize)
    {
        if (channel is not (1 or 2))
            throw new ArgumentOutOfRangeException(nameof(channel),
                "Only the observed shared channel-1/channel-2 fragment path is reconstructed.");
        if (payloadLength < 0)
            throw new ArgumentOutOfRangeException(nameof(payloadLength));
        if (maximumSegmentSize is < 1 or > MaximumPayloadPerFragment)
            throw new ArgumentOutOfRangeException(nameof(maximumSegmentSize),
                "A queued fragment includes four header bytes and must fit the native 1392-byte limit.");

        int count = payloadLength == 0 ? 0 : (payloadLength - 1) / maximumSegmentSize + 1;
        var fragments = new MatrixTransportFragment[count];
        int offset = 0;
        for (int i = 0; i < count; i++)
        {
            int length = Math.Min(maximumSegmentSize, payloadLength - offset);
            bool more = payloadLength - offset > maximumSegmentSize;
            ushort header = (ushort)((channel << 14) | (more ? MoreFragmentsBit : 0)
                | (length + FragmentHeaderSize));
            fragments[i] = new MatrixTransportFragment(header, offset, length, more);
            offset += length;
        }
        return Array.AsReadOnly(fragments);
    }
}

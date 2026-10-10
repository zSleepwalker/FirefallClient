using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

/// <summary>
/// The fragment inside a game-socket datagram: network-order header/sequence,
/// application bytes unchanged except for the observed attempt-marker XOR.
/// This does not add the datagram prefix, acknowledgment blocks, or probe padding.
/// </summary>
public static class MatrixDatagramFragmentEncoder
{
    public const uint DatagramAssemblyFileOffset = 0xBD4340;
    public const uint CopyFragmentToDatagramFileOffset = 0xBD4240;
    public const uint TransformPayloadFileOffset = 0xBD58E0;
    public const uint PayloadMaskTableVirtualAddress = 0x1BB7490;
    public const uint PayloadMaskTableFileOffset = 0x17B6090;
    public const uint AppendProbePaddingFileOffset = 0xBD4780;
    public const uint SocketSendMethodFileOffset = 0xBD2980;
    public const int DatagramBufferOffset = 0x7F1;
    public const int OpaqueDatagramPrefixWordOffset = 0x114;
    public const int ConnectedSendFlagOffset = 0x259;
    public const int DataSendState = 5;
    public const int AlternateDataSendState = 8;

    public static byte ResolvePayloadXorMask(byte marker) => marker switch
    {
        0 => 0x00,
        1 => 0xFF,
        2 => 0xAA,
        3 => 0xCC,
        _ => throw new ArgumentOutOfRangeException(nameof(marker),
            "The native payload transform supports markers 1..3; marker zero skips transformation.")
    };

    public static byte[] Encode(
        MatrixTransportFragment fragment,
        ushort sequence,
        ReadOnlySpan<byte> applicationBuffer,
        byte marker = 0)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        byte mask = ResolvePayloadXorMask(marker);
        int channel = fragment.InitialHeaderWord >> 14;
        if (channel is not (1 or 2)
            || fragment.PayloadLength is < 1 or > MatrixTransportFragmentPlan.MaximumPayloadPerFragment
            || fragment.PayloadOffset < 0
            || fragment.PayloadOffset > applicationBuffer.Length - fragment.PayloadLength)
            throw new ArgumentException("The fragment must describe a valid channel-1/channel-2 application slice.",
                nameof(fragment));

        int length = fragment.PayloadLength + MatrixTransportFragmentPlan.FragmentHeaderSize;
        int expectedHeader = (channel << 14)
            | (fragment.MoreFragments ? MatrixTransportFragmentPlan.MoreFragmentsBit : 0) | length;
        if (fragment.InitialHeaderWord != expectedHeader)
            throw new ArgumentException("The initial fragment header does not match its payload and continuation.",
                nameof(fragment));

        var output = new byte[length];
        BinaryPrimitives.WriteUInt16BigEndian(output, (ushort)(fragment.InitialHeaderWord | (marker << 12)));
        BinaryPrimitives.WriteUInt16BigEndian(output.AsSpan(2), sequence);
        applicationBuffer.Slice(fragment.PayloadOffset, fragment.PayloadLength).CopyTo(output.AsSpan(4));
        for (int i = 4; i < output.Length; i++)
            output[i] ^= mask;
        return output;
    }
}

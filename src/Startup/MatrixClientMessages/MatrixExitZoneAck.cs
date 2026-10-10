namespace Firefall.Reconstruction.Startup;

/// <summary>Original handler log identifies msg-19 as ExitZoneAck. No context or body is encoded.</summary>
public static class MatrixExitZoneAck
{
    public const ushort MessageCode = 19;
    public const int PacketSize = 1;
    public const int TransportChannel = 1;
    public const uint SenderFileOffset = 0x498D70;
    public const uint EncoderFileOffset = 0x499970;

    public static byte[] Encode() => CompactPositiveUInt16Codec.Encode(MessageCode);
}

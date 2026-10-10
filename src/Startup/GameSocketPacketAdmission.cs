using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public enum GameSocketPacketAdmissionOutcome
{
    InvalidLength,
    MismatchedContextWord,
    InvalidSender,
    AcceptedZeroContext,
    AcceptedNonzeroContext
}

/// <summary>
/// The outer receive gate, not complete packet validation. Address-specificity
/// is injected because native address flags and conversion are modeled separately.
/// This does not perform the native optional sender-binding side effects.
/// </summary>
public static class GameSocketPacketAdmission
{
    public const uint MethodFileOffset = 0xBD3030;
    public const uint ZeroContextParserFileOffset = 0xBD39D0;
    public const uint NonzeroContextParserFileOffset = 0xBD3140;
    public const int MinimumDatagramSize = 5;
    public const int MaximumDatagramSize = 1400;
    public const int ContextWordSize = 4;
    public const int ExpectedContextWordOffset = 0x114;
    public const int BindSenderOnReceiveFlagOffset = 0x25A;
    public const int RejectedPacketCountOffset = 0xEC;

    public static GameSocketPacketAdmissionOutcome Classify(
        ReadOnlySpan<byte> datagram,
        uint expectedContextWord,
        bool senderHasSpecificAddress,
        ushort senderPort)
    {
        if (datagram.Length is < MinimumDatagramSize or > MaximumDatagramSize)
            return GameSocketPacketAdmissionOutcome.InvalidLength;
        if (BinaryPrimitives.ReadUInt32BigEndian(datagram) != expectedContextWord)
            return GameSocketPacketAdmissionOutcome.MismatchedContextWord;
        if (!senderHasSpecificAddress || senderPort == 0)
            return GameSocketPacketAdmissionOutcome.InvalidSender;
        return expectedContextWord == 0
            ? GameSocketPacketAdmissionOutcome.AcceptedZeroContext
            : GameSocketPacketAdmissionOutcome.AcceptedNonzeroContext;
    }
}

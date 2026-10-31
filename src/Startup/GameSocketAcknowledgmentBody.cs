using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public enum GameSocketAcknowledgmentDecodeError
{
    None,
    UnsupportedOpcode,
    TooFewWords,
    OddWordByteCount,
    TooManyWords
}

public sealed record GameSocketAcknowledgment(
    GameSocketControlOpcode Opcode,
    ushort PeerWindowSequence,
    ushort CumulativeAcknowledgmentSequence,
    IReadOnlyList<ushort> SelectiveAcknowledgmentSequences);

/// <summary>
/// Decodes the sequence words following deferred-control opcodes 2 and 3.
/// This parses the body only; it does not modify native queue state or schedule sends.
/// </summary>
public static class GameSocketAcknowledgmentBody
{
    public const uint DeferredControlMethodFileOffset =
        GameSocketStateAndControlCatalog.DeferredControlMethodFileOffset;
    public const uint NativeHandlerFileOffset =
        GameSocketReliableQueueNativeLayout.AcknowledgmentHandlerFileOffset;
    public const int WordSize = sizeof(ushort);
    public const int RequiredFrontierWordCount = 2;
    public const int MaximumWordCount =
        (GameSocketChunkHeaderDecoder.MaximumChunkSize
            - GameSocketChunkHeaderDecoder.HeaderSize - 1) / WordSize;

    public static bool TryDecode(
        GameSocketControlOpcode opcode,
        ReadOnlySpan<byte> encodedWords,
        out GameSocketAcknowledgment? acknowledgment,
        out GameSocketAcknowledgmentDecodeError error)
    {
        acknowledgment = null;
        if (opcode is not (GameSocketControlOpcode.AcknowledgeChannel1
            or GameSocketControlOpcode.AcknowledgeChannel2))
        {
            error = GameSocketAcknowledgmentDecodeError.UnsupportedOpcode;
            return false;
        }

        if (encodedWords.Length < RequiredFrontierWordCount * WordSize)
        {
            error = GameSocketAcknowledgmentDecodeError.TooFewWords;
            return false;
        }

        if ((encodedWords.Length & 1) != 0)
        {
            error = GameSocketAcknowledgmentDecodeError.OddWordByteCount;
            return false;
        }

        int wordCount = encodedWords.Length / WordSize;
        if (wordCount > MaximumWordCount)
        {
            error = GameSocketAcknowledgmentDecodeError.TooManyWords;
            return false;
        }

        ushort peerWindowSequence = BinaryPrimitives.ReadUInt16BigEndian(encodedWords);
        ushort cumulativeAcknowledgmentSequence =
            BinaryPrimitives.ReadUInt16BigEndian(encodedWords[WordSize..]);
        var selectiveAcknowledgments = new ushort[wordCount - RequiredFrontierWordCount];
        for (int i = 0; i < selectiveAcknowledgments.Length; i++)
        {
            int offset = (i + RequiredFrontierWordCount) * WordSize;
            selectiveAcknowledgments[i] =
                BinaryPrimitives.ReadUInt16BigEndian(encodedWords.Slice(offset, WordSize));
        }

        acknowledgment = new GameSocketAcknowledgment(
            opcode,
            peerWindowSequence,
            cumulativeAcknowledgmentSequence,
            Array.AsReadOnly(selectiveAcknowledgments));
        error = GameSocketAcknowledgmentDecodeError.None;
        return true;
    }
}

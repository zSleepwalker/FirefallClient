using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public abstract record GameSocketHandshakeReply
{
    public sealed record Hehe(uint ContextCandidate) : GameSocketHandshakeReply;
    public sealed record Hugg(ushort SequenceSeed, ushort RemotePort) : GameSocketHandshakeReply;
    public sealed record Abort(byte Reason, int? NativeErrorCode) : GameSocketHandshakeReply;
    public sealed record Rejected : GameSocketHandshakeReply;
}

/// <summary>
/// Two observed zero-context request/reply stages. The words are width-named
/// because their complete protocol meanings are not established.
/// Decoding does not perform native state writes, logging or network sends.
/// </summary>
public static class GameSocketHandshake
{
    public const uint SendPokeMethodFileOffset = 0xBD2580;
    public const uint SendKissMethodFileOffset = 0xBD26D0;
    public const uint ZeroContextReplyMethodFileOffset = 0xBD39D0;
    public const int ContextCandidateOffset = 0x224;
    public const int HuggSequenceSeedOffset = 0x228;
    public const int RemotePortOffset = 0x248;
    public const int AbortReasonOffset = 0x11C;
    public const ushort ObservedPokeWord = 4;

    public static byte[] EncodePoke(ushort wordAtSocket30)
    {
        var bytes = new byte[12];
        "POKE"u8.CopyTo(bytes.AsSpan(4));
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(8), ObservedPokeWord);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(10), wordAtSocket30);
        return bytes;
    }

    public static byte[] EncodeKiss(uint contextCandidate, ushort wordAtSocket32)
    {
        var bytes = new byte[14];
        "KISS"u8.CopyTo(bytes.AsSpan(4));
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(8), contextCandidate);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(12), wordAtSocket32);
        return bytes;
    }

    public static GameSocketHandshakeReply DecodeReplyBody(
        ReadOnlySpan<byte> body, int nativeSocketState)
    {
        if (body.Length >= 5 && body.StartsWith("ABRT"u8))
        {
            byte reason = body[4];
            int? error = reason switch
            {
                0 => 0,
                1 or 2 => 12,
                3 => 10,
                4 => 11,
                5 => 13,
                _ => null
            };
            return new GameSocketHandshakeReply.Abort(reason, error);
        }

        if (body.Length >= 8)
        {
            if (nativeSocketState == 1 && body.StartsWith("HEHE"u8))
                return new GameSocketHandshakeReply.Hehe(BinaryPrimitives.ReadUInt32BigEndian(body[4..]));
            if (nativeSocketState == 2 && body.StartsWith("HUGG"u8))
            {
                ushort port = BinaryPrimitives.ReadUInt16BigEndian(body[6..]);
                if (port != 0)
                    return new GameSocketHandshakeReply.Hugg(
                        BinaryPrimitives.ReadUInt16BigEndian(body[4..]), port);
            }
        }
        return new GameSocketHandshakeReply.Rejected();
    }
}

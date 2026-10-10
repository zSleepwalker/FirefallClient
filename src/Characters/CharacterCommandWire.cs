// Wire encoding used by the traced Character command senders. File offsets, IDA base 0.
namespace Firefall.Characters;

/// <summary>
/// Byte-level encoding reproduced from sl::serialize::blob_packer calls
/// (packer vtable file 0x187FD44, backing slBlob vtable file 0x187F8C4).
/// The packer stores raw x86 values, so multi-byte integers are little-endian.
/// </summary>
public static class CharacterCommandWire
{
    /// <summary>Channel argument passed to the connection send routine 0xBD3B20 (call at 0x90EEED). Channel semantics are UNKNOWN.</summary>
    public const int SendChannel = 2;

    /// <summary>
    /// Complete body produced by encoders 0x90FD40 / 0x90FE20 / 0x910040, in native call order:
    /// <list type="number">
    /// <item>u64 target (0x325BE0; 0x90FDB7)</item>
    /// <item>MessageID (0x320900; 0x90FDC0)</item>
    /// <item>payload (0x344440 for 175; 0x5BA110 + 0x5BA240 for 194)</item>
    /// </list>
    /// Whatever the transport adds around this body (0xBD3B20 and below) is out of scope / UNKNOWN.
    /// </summary>
    public static byte[] Encode(CharacterCommandMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        using var stream = new MemoryStream(message.NativeReserveHint);
        using var writer = new BinaryWriter(stream);
        writer.Write(message.Target);
        WriteMessageId(writer, message.MessageId);
        message.WritePayload(writer);
        writer.Flush();
        return stream.ToArray();
    }

    /// <summary>
    /// 0x320900: id 0 is rejected ("tried to pack an uninitialized MessageID (native assert line 50 / 0x32)"); ids &lt;= 0xFF
    /// are one byte; larger ids are escape byte 0x00 (rdata file 0x1756225) followed by u16.
    /// Native code asserts (does not throw) for ids &gt; 0xFFFF and still packs the low 16 bits.
    /// </summary>
    public static void WriteMessageId(BinaryWriter writer, uint id)
    {
        if (id == 0) throw new ArgumentOutOfRangeException(nameof(id), "tried to pack an uninitialized MessageID (native assert line 50 / 0x32)");
        if (id > 0xFFFF) throw new ArgumentOutOfRangeException(nameof(id), "MessageID exceeds 16 bits (native asserts at line 57 / 0x39)");
        if (id <= 0xFF)
        {
            writer.Write((byte)id);
        }
        else
        {
            writer.Write((byte)0x00);
            writer.Write((ushort)id);
        }
    }

    /// <summary>
    /// Chunked sequence shared by 0x5BA110 and 0x5BA240: repeat { u8 n = min(remaining, 255);
    /// n elements } while n == 255. A multiple of 255 therefore ends with an explicit 0 chunk,
    /// and an empty set is a single 0 byte. Native asserts (line 382) if the count mismatches.
    /// </summary>
    public static void WriteChunkedSequence<T>(BinaryWriter writer, IReadOnlyList<T> items, Action<BinaryWriter, T> writeItem)
    {
        int written = 0;
        byte chunk;
        do
        {
            chunk = (byte)Math.Min(items.Count - written, 0xFF);
            writer.Write(chunk);
            for (int i = 0; i < chunk; i++) writeItem(writer, items[written + i]);
            written += chunk;
        }
        while (chunk == 0xFF);
    }

    /// <summary>0x3265A0: non-zero and not (upper 56 bits all ones).</summary>
    public static bool IsValidTarget(ulong target) =>
        target != 0 && (target & 0xFFFF_FFFF_FFFF_FF00UL) != 0xFFFF_FFFF_FFFF_FF00UL;
}

/// <summary>
/// Pre-send gate shared by 0x90CBC0 (174), 0x90CC30 (175), 0x90CF60 (194) and inlined in 0x9095B0.
/// The connection object comes from 0x494BF0, which selects one of three fields based on an
/// UNKNOWN global mode value (1/2/3); other modes yield no connection and nothing is sent.
/// </summary>
public readonly record struct CharacterCommandConnectionState(uint Field0x820, int State0x0C, uint Flags0x14)
{
    /// <summary>Returns false (not sent) when +0x820 is zero or +0x0C &lt;= 3. Meanings of both fields are UNKNOWN.</summary>
    public bool CanSend(out string? logLine, string debugName)
    {
        logLine = null;
        if (Field0x820 == 0) return false;
        if (State0x0C <= 3)
        {
            logLine = $"Will not SendCommand {debugName} -- too early!";
            return false;
        }
        if ((Flags0x14 & 8) != 0) logLine = $"ROUTED - Sent {debugName}";
        return true;
    }
}

/// <summary>
/// 0x49B3D0: rate limiter that drives UpdateShoppingList building (0x9095B0).
/// Layout observed: +0x00 source container, +0x08 dirty byte, +0x0C interval, +0x10 last tick.
/// Uses WINMM!timeGetTime (IAT VA 0x1AC4D24 = raw 0x16C3924). Who sets the dirty flag is not traced (UNKNOWN).
/// </summary>
public sealed class ShoppingListFlushThrottle(uint intervalMs)
{
    public bool Dirty { get; set; }
    public uint IntervalMs { get; } = intervalMs;
    public uint LastTick { get; private set; }

    /// <summary>Returns true when native code would rebuild and send the message.</summary>
    public bool Poll(uint nowMs)
    {
        if (!Dirty) return false;
        uint elapsed = nowMs - LastTick;
        if (elapsed < IntervalMs) return false;
        if (IntervalMs != 0) elapsed -= elapsed % IntervalMs; // keep phase aligned to interval
        LastTick += elapsed;
        Dirty = false;
        return true;
    }
}

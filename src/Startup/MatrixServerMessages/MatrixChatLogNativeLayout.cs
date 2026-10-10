using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using Firefall.Reconstruction.Shared;

namespace Firefall.Reconstruction.Startup;

public static class MatrixChatLogNativeLayout
{
    private static readonly byte[] footer = Convert.FromHexString("73616C74696E652C637261636B657273000000000C2AFFFF00");

    public const uint WriteLogFileOffset = 0x6F46F0;
    public const uint HashByteSpanFileOffset = 0x12F1780;
    public const int ChatTextAt10Offset = 0x10;
    public const int ChatLogFileOffset = 0x18;
    public const int ModeTextAt10Offset = ChatTextAt10Offset;
    public const int ModeLogFileOffset = ChatLogFileOffset;
    public const int ModeTextByteLimit = 32;
    public const uint FooterVirtualAddress = 0x1C67C34;
    public static ReadOnlySpan<byte> FooterBytes => footer;

    public static uint ComputeRawHash(ReadOnlySpan<byte> bytes) => EngineByteHash32.Compute(bytes);
    public static string GetNativeTimestampFormat(bool detailed) => detailed ? "%c %Z" : "%X";

    /// <summary>The timestamp is already formatted by the caller; native locale/time-zone conversion is not emulated.</summary>
    public static byte[] BuildFormattedPrefix(ReadOnlySpan<byte> timestampBytes, ReadOnlySpan<byte> textBytes,
        ReadOnlyMemory<byte>? channelBytes, ReadOnlyMemory<byte>? authorBytes)
    {
        var result = new List<byte>();
        AppendCString(result, timestampBytes);
        result.Add((byte)' ');
        if (channelBytes.HasValue || authorBytes.HasValue)
        {
            if (channelBytes.HasValue)
                AppendCString(result, channelBytes.Value.Span);
            if (authorBytes.HasValue)
            {
                if (channelBytes.HasValue)
                    result.Add((byte)' ');
                AppendCString(result, authorBytes.Value.Span);
            }
            result.Add((byte)':');
            result.Add((byte)' ');
        }
        AppendCString(result, textBytes);
        result.Add((byte)' ');
        return result.ToArray();
    }

    public static byte[] BuildChecksumInput(ReadOnlySpan<byte> formattedPrefix, ulong authorId,
        ReadOnlySpan<byte> modeTextAt10)
    {
        if (formattedPrefix.IndexOf((byte)0) >= 0)
            throw new ArgumentException("The formatted native CString prefix must exclude its terminator and embedded NULs.",
                nameof(formattedPrefix));
        int nameCount = Math.Min(ModeTextByteLimit, modeTextAt10.Length);
        var result = new byte[checked(formattedPrefix.Length + 34 + nameCount)];
        formattedPrefix.CopyTo(result);
        int offset = formattedPrefix.Length + 1;
        BinaryPrimitives.WriteUInt64LittleEndian(result.AsSpan(offset), authorId & 0xFFFFFFFFFFFFFF00);
        footer.CopyTo(result.AsSpan(offset + 8));
        var source = modeTextAt10[..nameCount];
        int firstNull = source.IndexOf((byte)0);
        if (firstNull >= 0)
            source = source[..firstNull];
        // strncpy pads the rest of the sampled native shared-text length with zero bytes.
        source.CopyTo(result.AsSpan(offset + 8 + footer.Length));
        return result;
    }

    public static ushort ComputeChecksum(ReadOnlySpan<byte> formattedPrefix, ulong authorId,
        ReadOnlySpan<byte> modeTextAt10)
    {
        uint hash = ComputeRawHash(BuildChecksumInput(formattedPrefix, authorId, modeTextAt10));
        return unchecked((ushort)(hash ^ (hash >> 16)));
    }

    public static byte[] BuildLogLine(ReadOnlySpan<byte> formattedPrefix, ulong authorId, ReadOnlySpan<byte> modeTextAt10)
    {
        ushort checksum = ComputeChecksum(formattedPrefix, authorId, modeTextAt10);
        byte[] suffix = Encoding.ASCII.GetBytes(checksum.ToString("X4", CultureInfo.InvariantCulture) + "\n");
        var line = new byte[checked(formattedPrefix.Length + suffix.Length)];
        formattedPrefix.CopyTo(line);
        suffix.CopyTo(line.AsSpan(formattedPrefix.Length));
        return line;
    }

    private static void AppendCString(List<byte> target, ReadOnlySpan<byte> bytes)
    {
        foreach (byte value in bytes)
        {
            if (value == 0)
                break;
            target.Add(value);
        }
    }
}

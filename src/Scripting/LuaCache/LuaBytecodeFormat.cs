namespace Firefall.Scripting.LuaCache;

/// <summary>
/// Cache file format. The file body is exactly the byte stream emitted by the inlined lua_dump
/// (raw 0xC28B80, strip = 1) through the fwrite writer at raw 0xC3BCA0; no Firefall wrapper,
/// key, or timestamp is stored in the file. Staleness is carried by the file mtime instead.
/// The header writer is raw 0xCD3A70 and matches the Lua 5.3 luaU_dump header.
/// </summary>
public static class LuaBytecodeFormat
{
    public static ReadOnlySpan<byte> Signature => "\x1bLua"u8;               // raw 0x17C6838 (VA 0x1BC7C38)
    public const byte Version = 0x53;                                        // raw 0xCD3A9A
    public const byte Format = 0x00;                                         // raw 0xCD3ABA
    public static ReadOnlySpan<byte> LuacData => [0x19, 0x93, 0x0D, 0x0A, 0x1A, 0x0A]; // raw 0x17C6830 (VA 0x1BC7C30)
    public const byte SizeofInt = 4;         // raw 0xCD3AF5
    public const byte SizeofSizeT = 4;       // raw 0xCD3B15
    public const byte SizeofInstruction = 4; // raw 0xCD3B35
    public const byte SizeofLuaInteger = 4;  // raw 0xCD3B55 (32-bit lua_Integer build)
    public const byte SizeofLuaNumber = 8;   // raw 0xCD3B75
    public const int LuacInt = 0x5678;       // raw 0xCD3B95, written as 4 bytes
    public const double LuacNum = 370.5;     // qword at raw 0x1880AF0 (VA 0x1C81EF0)

    /// <summary>Header length before the 1-byte sizeupvalues (raw 0xC28BBB reads Proto+0x0C) and the dumped main function.</summary>
    public const int HeaderLength = 4 + 1 + 1 + 6 + 5 + 4 + 8;

    public static byte[] BuildExpectedHeader()
    {
        var h = new byte[HeaderLength];
        var w = h.AsSpan();
        Signature.CopyTo(w); w = w[4..];
        w[0] = Version; w[1] = Format; w = w[2..];
        LuacData.CopyTo(w); w = w[6..];
        w[0] = SizeofInt; w[1] = SizeofSizeT; w[2] = SizeofInstruction; w[3] = SizeofLuaInteger; w[4] = SizeofLuaNumber; w = w[5..];
        BitConverter.TryWriteBytes(w, LuacInt); w = w[4..];
        BitConverter.TryWriteBytes(w, LuacNum);
        return h;
    }

    public static bool HasExpectedHeader(ReadOnlySpan<byte> file) =>
        file.Length > HeaderLength && file[..HeaderLength].SequenceEqual(BuildExpectedHeader());
}

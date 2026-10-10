using Firefall.Reconstruction.Startup;

internal static class MatrixChatLogChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Native byte-span hash is FNV1a plus final avalanche, not plain FNV or CRC",
            MatrixChatLogNativeLayout.ComputeRawHash(ReadOnlySpan<byte>.Empty) == 0x5902879E
            && MatrixChatLogNativeLayout.ComputeRawHash("a"u8) == 0xD94AA0CF
            && MatrixChatLogNativeLayout.ComputeRawHash("hello"u8) == 0xEB22D089
            && MatrixChatLogNativeLayout.ComputeRawHash(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray()) == 0xF4E61607);
        ReadOnlyMemory<byte>? noLabel = null;
        ReadOnlyMemory<byte>? emptyLabel = ReadOnlyMemory<byte>.Empty;
        var prefix = MatrixChatLogNativeLayout.BuildFormattedPrefix("12:34"u8, "Hi!"u8, "zone"u8.ToArray(), "Alice"u8.ToArray());
        check("Chat-log prefix preserves zero/one/two-label formats, trailing space and CString termination",
            prefix.AsSpan().SequenceEqual("12:34 zone Alice: Hi! "u8)
            && MatrixChatLogNativeLayout.BuildFormattedPrefix("T"u8, "Hi"u8, noLabel, noLabel).AsSpan().SequenceEqual("T Hi "u8)
            && MatrixChatLogNativeLayout.BuildFormattedPrefix("T"u8, "Hi"u8, "broadcast"u8.ToArray(), noLabel)
                .AsSpan().SequenceEqual("T broadcast: Hi "u8)
            && MatrixChatLogNativeLayout.BuildFormattedPrefix("T"u8, "Hi"u8, "zone"u8.ToArray(), emptyLabel)
                .AsSpan().SequenceEqual("T zone : Hi "u8)
            && MatrixChatLogNativeLayout.BuildFormattedPrefix("T"u8, new byte[] { 0x80, 0, 0xFF }, noLabel, "A"u8.ToArray())
                .AsSpan().SequenceEqual(new byte[] { (byte)'T', 32, (byte)'A', (byte)':', 32, 0x80, 32 })
            && MatrixChatLogNativeLayout.GetNativeTimestampFormat(false) == "%X"
            && MatrixChatLogNativeLayout.GetNativeTimestampFormat(true) == "%c %Z");
        check("Chat-log hash input is prefix/NUL/masked LE64/static25/mode-text; folds to uppercase4hex newline",
            MatrixChatLogNativeLayout.BuildChecksumInput(prefix, 0x0102030405060708, "Bob"u8).AsSpan()
                .SequenceEqual(Convert.FromHexString("31323A3334207A6F6E6520416C6963653A204869212000000706050403020173616C74696E652C637261636B657273000000000C2AFFFF00426F62"))
            && MatrixChatLogNativeLayout.ComputeChecksum(prefix, 0x0102030405060708, "Bob"u8) == 0x7F13
            && MatrixChatLogNativeLayout.BuildLogLine(prefix, 0x0102030405060708, "Bob"u8).AsSpan()
                .SequenceEqual("12:34 zone Alice: Hi! 7F13\n"u8));
        var name32 = Enumerable.Repeat((byte)'N', 32).ToArray();
        check("Chat-log ID low8bits are cleared and mode-text clipping is32 raw bytes, not characters",
            MatrixChatLogNativeLayout.ComputeChecksum(prefix, 0x0102030405060701, name32)
                == MatrixChatLogNativeLayout.ComputeChecksum(prefix, 0x01020304050607FF, name32.Concat(new byte[] { 0xFF }).ToArray())
            && MatrixChatLogNativeLayout.BuildChecksumInput(prefix, 0, name32).Length == prefix.Length + 34 + 32
            && MatrixChatLogNativeLayout.BuildChecksumInput(prefix, 0, name32[..31]).Length == prefix.Length + 34 + 31
            && MatrixChatLogNativeLayout.BuildChecksumInput(prefix, 0,
                    Enumerable.Repeat((byte)'N', 31).Concat(new byte[] { 0xC3, 0xB1 }).ToArray())[^1] == 0xC3
            && MatrixChatLogNativeLayout.BuildChecksumInput(prefix, 0, new byte[] { (byte)'A', 0, (byte)'B' })[^3..]
                .AsSpan().SequenceEqual(new byte[] { (byte)'A', 0, 0 }));
        bool nulRejected = false;
        try { MatrixChatLogNativeLayout.BuildChecksumInput(new byte[] { 1, 0, 2 }, 0, default); }
        catch (ArgumentException) { nulRejected = true; }
        check("Already-formatted checksum prefix cannot silently contain embedded NULs", nulRejected);
    }
}

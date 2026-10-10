using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedModeOneImmediateMoveChecks
{
    public static void Run(Action<string, bool> check)
    {
        bool Matches(uint code, ulong value, byte width, params string[] expected) =>
            NativeGeneratedImmediateMoveTemplates.ForModeOneOrdinaryRegister(code, value, width)
                .Select(candidate => Convert.ToHexString(candidate.Span)).SequenceEqual(expected);
        check("Mode1 body preparatory RCX/RSI MOV from32-bit immediate has only REX.W C7, not compact imm64",
            Matches(54, 4, 32, "48C7C104000000")
            && Matches(59, 1, 32, "48C7C601000000")
            && Matches(54, 0xFFFFFFFF80000000, 32, "48C7C100000080"));
        check("Mode1 ordinary64-bit MOV keeps compact imm64 before signed-fit C7 imm32 and exact signed boundary gates",
            Matches(53, 0x7FFFFFFF, 64, "48B8FFFFFF7F00000000", "48C7C0FFFFFF7F")
            && Matches(53, 0x80000000, 64, "48B80000008000000000")
            && Matches(53, 0xFFFFFFFF80000000, 64, "48B800000080FFFFFFFF", "48C7C000000080")
            && Matches(53, 0xFFFFFFFF7FFFFFFF, 64, "48B8FFFFFF7FFFFFFFFF")
            && Matches(53, ulong.MaxValue, 64, "48B8FFFFFFFFFFFFFFFF", "48C7C0FFFFFFFF"));
        uint address = 0x80001000;
        ulong signedAddressBits = unchecked((ulong)(long)(int)address);
        check("Mode1 table-base MOV retains native signed-DWORD pointer expansion rather than zero extension",
            Matches(60, signedAddressBits, 64, "48BF00100080FFFFFFFF", "48C7C700100080")
            && Matches(60, address, 64, "48BF0010008000000000"));
        bool registers = true;
        for (uint index = 0; index < 8; index++)
        {
            var candidates = NativeGeneratedImmediateMoveTemplates.ForModeOneOrdinaryRegister(53 + index, 0x0123456789ABCDEF, 64);
            var bytes = candidates[0].Span;
            registers &= candidates.Count == 1 && bytes[0] == 0x48 && bytes[1] == 0xB8 + index
                && Convert.ToHexString(bytes[2..]) == "EFCDAB8967452301";
        }
        check("Mode1 ordinary-register compact MOV preserves full64-bit LE immediate and opcode index0..7", registers);
        check("Mode1 ordinary byte MOV reuses unchanged compact/ModRM candidates with byte truncation",
            Matches(2, 0x12345678, 8, "B178", "C6C178"));
        bool extendedRejected = false;
        bool widthRejected = false;
        try { NativeGeneratedImmediateMoveTemplates.ForModeOneOrdinaryRegister(61, 0, 64); }
        catch (ArgumentOutOfRangeException) { extendedRejected = true; }
        try { NativeGeneratedImmediateMoveTemplates.ForModeOneOrdinaryRegister(53, 0, 16); }
        catch (ArgumentOutOfRangeException) { widthRejected = true; }
        check("Mode1 immediate MOV catalog explicitly rejects unsupported extended destinations and source widths",
            extendedRejected && widthRejected);
    }
}

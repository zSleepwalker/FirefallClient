using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedScratchTemplateChecks
{
    public static void Run(Action<string, bool> check)
    {
        var wideInc = NativeGeneratedScratchTemplates.UnaryFromConsumedParitySample(NativeGeneratedOperationCode.Increment, 0);
        var narrowDec = NativeGeneratedScratchTemplates.UnaryFromConsumedParitySample(NativeGeneratedOperationCode.Decrement, 1);
        check("Native scratch unary parity retains one BL candidate or two EBX candidates in compact/FF order",
            wideInc.Count == 2 && Convert.ToHexString(wideInc[0].Span) == "43"
            && Convert.ToHexString(wideInc[1].Span) == "FFC3"
            && narrowDec.Count == 1 && Convert.ToHexString(narrowDec[0].Span) == "FECB");
        bool counts = true;
        for (int count = 0; count < 256; count++)
        {
            var left = NativeGeneratedScratchTemplates.RotateImmediate(NativeGeneratedOperationCode.RotateLeft, (byte)count);
            counts &= left.Count == (count == 1 ? 2 : 1)
                && left[0].Span.SequenceEqual(new byte[] { 0xC0, 0xC3, (byte)count });
        }
        var one = NativeGeneratedScratchTemplates.RotateImmediate(NativeGeneratedOperationCode.RotateRight, 1);
        check("Native byte rotation count1 adds implicit-one candidate after immediate form; other byte counts have one",
            counts && Convert.ToHexString(one[0].Span) == "C0CB01" && Convert.ToHexString(one[1].Span) == "D0CB");
        check("Native byte rotate-via-CL templates are D2 /0 or /1 over BL",
            Convert.ToHexString(NativeGeneratedScratchTemplates.RotateViaCl(NativeGeneratedOperationCode.RotateLeft)[0].Span) == "D2C3"
            && Convert.ToHexString(NativeGeneratedScratchTemplates.RotateViaCl(NativeGeneratedOperationCode.RotateRight)[0].Span) == "D2CB");
        var clMove = NativeGeneratedImmediateMoveTemplates.ForRegister(2, 0xABCDEF01);
        var ediMove = NativeGeneratedImmediateMoveTemplates.ForRegister(44, 0x89ABCDEF);
        check("Native preparatory MOV preserves compact/ModRM order, byte truncation and captured pointer DWORD",
            Convert.ToHexString(clMove[0].Span) == "B101"
            && Convert.ToHexString(clMove[1].Span) == "C6C101"
            && Convert.ToHexString(ediMove[0].Span) == "BFEFCDAB89"
            && Convert.ToHexString(ediMove[1].Span) == "C7C7EFCDAB89");
        bool moveRegisters = true;
        for (uint index = 0; index < 8; index++)
        {
            foreach (uint value in new uint[] { 0, 1, 127, 128, 255, 256, 0x80000000, uint.MaxValue })
            {
                var narrow = NativeGeneratedImmediateMoveTemplates.ForRegister(index + 1, value);
                var wide = NativeGeneratedImmediateMoveTemplates.ForRegister(index + 37, value);
                moveRegisters &= narrow.Count == 2 && wide.Count == 2
                    && narrow[0].Span.SequenceEqual(new byte[] { (byte)(0xB0 + index), unchecked((byte)value) })
                    && narrow[1].Span.SequenceEqual(new byte[] { 0xC6, (byte)(0xC0 | index), unchecked((byte)value) })
                    && wide[0].Length == 5 && wide[0].Span[0] == 0xB8 + index
                    && wide[1].Length == 6 && wide[1].Span[0] == 0xC7 && wide[1].Span[1] == (0xC0 | index)
                    && System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(wide[0].Span[1..]) == value
                    && System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(wide[1].Span[2..]) == value;
            }
        }
        check("Native immediate MOV covers all ordinary8/32-bit register indices and sign-boundary DWORD values", moveRegisters);
    }
}

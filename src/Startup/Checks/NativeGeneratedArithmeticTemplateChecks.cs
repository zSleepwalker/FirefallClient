using System.Buffers.Binary;
using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedArithmeticTemplateChecks
{
    public static void Run(Action<string, bool> check)
    {
        NativeGeneratedOperationCode[] operations =
            [NativeGeneratedOperationCode.Add, NativeGeneratedOperationCode.Subtract, NativeGeneratedOperationCode.Xor];
        byte[] groups = [0x00, 0x28, 0x30];
        bool immediateChecks = true;
        bool registerChecks = true;
        bool tableChecks = true;
        for (int op = 0; op < operations.Length; op++)
        {
            for (uint sample = 0; sample < 256; sample++)
            {
                var narrow = NativeGeneratedArithmeticTemplates.ImmediateFromConsumedParitySample(operations[op], 1, sample);
                var wide = NativeGeneratedArithmeticTemplates.ImmediateFromConsumedParitySample(operations[op], 0, sample);
                immediateChecks &= narrow.Count == 1
                    && narrow[0].Span.SequenceEqual(new byte[] { 0x80, (byte)(0xC3 | groups[op]), (byte)sample })
                    && wide.Count == 1 && wide[0].Length == 6
                    && wide[0].Span[0] == 0x81 && wide[0].Span[1] == (0xC3 | groups[op])
                    && BinaryPrimitives.ReadUInt32LittleEndian(wide[0].Span[2..]) == sample;
            }
            for (uint parity = 0; parity < 2; parity++)
            {
                for (uint parameter = 0; parameter < 2; parameter++)
                {
                    var choice = NativeGeneratedBodyDecisionRules.ArithmeticRegisterSource(parameter, parity);
                    var candidates = NativeGeneratedArithmeticTemplates.RegisterSource(operations[op], choice);
                    byte sourceIndex = parameter == 0 ? (byte)1 : (byte)2;
                    bool wide = parity == 0;
                    registerChecks &= candidates.Count == 2
                        && candidates[0].Span.SequenceEqual(new byte[] { (byte)(groups[op] + (wide ? 1 : 0)), (byte)(0xC3 | (sourceIndex << 3)) })
                        && candidates[1].Span.SequenceEqual(new byte[] { (byte)(groups[op] + (wide ? 3 : 2)), (byte)(0xD8 | sourceIndex) });
                }
            }
            for (uint parameter = 0; parameter < 4096; parameter++)
            {
                var choice = NativeGeneratedBodyDecisionRules.ArithmeticTableSource(parameter, 0xFFFFFFF0);
                var candidates = NativeGeneratedArithmeticTemplates.TableSource(operations[op], choice);
                byte opcode = (byte)(groups[op] + ((parameter & 1) != 0 ? 2 : 3));
                uint scaleBits = (parameter >> 1) & 3;
                byte[] expected = scaleBits == 0
                    ? [opcode, 0x1F]
                    : [opcode, 0x1C, (byte)((scaleBits << 6) | 0x37)];
                tableChecks &= candidates.Count == 1 && candidates[0].Span.SequenceEqual(expected);
            }
        }
        check("Native ADD/SUB/XOR scratch immediates preserve 80/81 groups without an invented sign-shortened83 alternative", immediateChecks);
        check("Native ADD/SUB/XOR register sources retain r/m-first then register-first alternatives", registerChecks);
        check("Native ADD/SUB/XOR bounded table sources retain EDI and scaled-ESI SIB bytes without displacement or host reads", tableChecks);
        check("Native arithmetic immediate truncation is low byte for BL, complete DWORD for EBX",
            Convert.ToHexString(NativeGeneratedArithmeticTemplates.ImmediateFromConsumedParitySample(NativeGeneratedOperationCode.Xor, 1, uint.MaxValue)[0].Span) == "80F3FF"
            && Convert.ToHexString(NativeGeneratedArithmeticTemplates.ImmediateFromConsumedParitySample(NativeGeneratedOperationCode.Subtract, 0, 0x80000000)[0].Span) == "81EB00000080");

        bool rejected = false;
        try
        {
            NativeGeneratedArithmeticTemplates.TableSource(NativeGeneratedOperationCode.Add,
                new(8, 2, 0, 0, 0, 44, 0));
        }
        catch (ArgumentOutOfRangeException) { rejected = true; }
        check("Native arithmetic table projection rejects missing scaled index instead of generating unsupported operands", rejected);
    }
}

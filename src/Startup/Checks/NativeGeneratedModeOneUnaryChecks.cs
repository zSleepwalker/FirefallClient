using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedModeOneUnaryChecks
{
    public static void Run(Action<string, bool> check)
    {
        string Scratch(NativeGeneratedOperationCode operation, uint parity)
        {
            var candidates = NativeGeneratedScratchTemplates.UnaryForModeOneFromConsumedParitySample(operation, parity);
            return candidates.Count == 1 ? Convert.ToHexString(candidates[0].Span) : "unexpected candidate count";
        }
        check("Mode1 scratch INC/DEC has one BL or REX.W RBX candidate, never compact40..4F",
            Scratch(NativeGeneratedOperationCode.Increment, 0) == "48FFC3"
            && Scratch(NativeGeneratedOperationCode.Decrement, 0) == "48FFCB"
            && Scratch(NativeGeneratedOperationCode.Increment, 1) == "FEC3"
            && Scratch(NativeGeneratedOperationCode.Decrement, uint.MaxValue) == "FECB");
        bool registers = true;
        for (uint i = 0; i < 8; i++)
        foreach (var operation in new[] { NativeGeneratedOperationCode.Increment, NativeGeneratedOperationCode.Decrement })
        {
            byte group = operation == NativeGeneratedOperationCode.Increment ? (byte)0xC0 : (byte)0xC8;
            var wide = NativeGeneratedScratchTemplates.UnaryForModeOneOrdinaryRegister(operation, 53 + i);
            var narrow = NativeGeneratedScratchTemplates.UnaryForModeOneOrdinaryRegister(operation, 1 + i);
            registers &= wide.Count == 1 && wide[0].Span.SequenceEqual(new byte[] { 0x48, 0xFF, (byte)(group | i) })
                && narrow.Count == 1 && narrow[0].Span.SequenceEqual(new byte[] { 0xFE, (byte)(group | i) });
        }
        check("Mode1 ordinary unary register catalog preserves INC/DEC ModRM groups for all eight byte/wide registers", registers);
        bool descriptorRejected = false;
        bool operationRejected = false;
        try { NativeGeneratedScratchTemplates.UnaryForModeOneOrdinaryRegister(NativeGeneratedOperationCode.Increment, 37); }
        catch (ArgumentOutOfRangeException) { descriptorRejected = true; }
        try { NativeGeneratedScratchTemplates.UnaryForModeOneFromConsumedParitySample(NativeGeneratedOperationCode.Add, 0); }
        catch (ArgumentOutOfRangeException) { operationRejected = true; }
        check("Mode1 unary catalog explicitly rejects unsupported widths and non-unary operations",
            descriptorRejected && operationRejected);
    }
}

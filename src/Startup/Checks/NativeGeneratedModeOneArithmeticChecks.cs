using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedModeOneArithmeticChecks
{
    public static void Run(Action<string, bool> check)
    {
        bool Matches(IReadOnlyList<ReadOnlyMemory<byte>> candidates, params string[] expected) =>
            candidates.Select(candidate => Convert.ToHexString(candidate.Span)).SequenceEqual(expected);
        check("Mode1 scratch arithmetic uses one REX.W81 imm32 candidate, no83 or invented imm64",
            Matches(NativeGeneratedArithmeticTemplates.ImmediateForModeOneFromConsumedParitySample(NativeGeneratedOperationCode.Add, 0, 0x89ABCDEF), "4881C3EFCDAB89")
            && Matches(NativeGeneratedArithmeticTemplates.ImmediateForModeOneFromConsumedParitySample(NativeGeneratedOperationCode.Subtract, 0, 0x89ABCDEF), "4881EBEFCDAB89")
            && Matches(NativeGeneratedArithmeticTemplates.ImmediateForModeOneFromConsumedParitySample(NativeGeneratedOperationCode.Xor, 0, 0x89ABCDEF), "4881F3EFCDAB89")
            && Matches(NativeGeneratedArithmeticTemplates.ImmediateForModeOneFromConsumedParitySample(NativeGeneratedOperationCode.Add, 1, 0x89ABCDEF), "80C3EF"));
        var rcx = NativeGeneratedBodyDecisionRules.ArithmeticRegisterSourceForModeOne(0, 0);
        var rdx = NativeGeneratedBodyDecisionRules.ArithmeticRegisterSourceForModeOne(1, 0);
        var dl = NativeGeneratedBodyDecisionRules.ArithmeticRegisterSourceForModeOne(1, 1);
        check("Mode1 arithmetic operand decisions retain parameter parity and choose RBX/RCX/RDX or BL/CL/DL",
            rcx == new NativeGeneratedRegisterSourceChoice(new(56, 64), 54, true, 0)
            && rdx == new NativeGeneratedRegisterSourceChoice(new(56, 64), 55, false, 1)
            && dl == new NativeGeneratedRegisterSourceChoice(new(4, 8), 3, false, 1)
            && NativeGeneratedBodyDecisionRules.RotateCountRegisterForModeOneFromConsumedParitySample(0) == new NativeGeneratedScratchChoice(54, 64)
            && NativeGeneratedBodyDecisionRules.RotateCountRegisterForModeOneFromConsumedParitySample(1) == new NativeGeneratedScratchChoice(2, 8));
        check("Mode1 register arithmetic preserves r/m-destination then register-destination alternatives and REX.W",
            Matches(NativeGeneratedArithmeticTemplates.RegisterSourceForModeOne(NativeGeneratedOperationCode.Add, rcx), "4801CB", "4803D9")
            && Matches(NativeGeneratedArithmeticTemplates.RegisterSourceForModeOne(NativeGeneratedOperationCode.Add, rdx), "4801D3", "4803DA")
            && Matches(NativeGeneratedArithmeticTemplates.RegisterSourceForModeOne(NativeGeneratedOperationCode.Subtract, rcx), "4829CB", "482BD9")
            && Matches(NativeGeneratedArithmeticTemplates.RegisterSourceForModeOne(NativeGeneratedOperationCode.Xor, dl), "30D3", "32DA"));
        var direct = NativeGeneratedBodyDecisionRules.ArithmeticTableSourceForModeOne(0, 0x80001000);
        var scaled = NativeGeneratedBodyDecisionRules.ArithmeticTableSourceForModeOne(14, 0x80001000);
        var narrow = NativeGeneratedBodyDecisionRules.ArithmeticTableSourceForModeOne(15, 0x80001000);
        check("Mode1 table arithmetic preserves RDI/RSI addressing, low-byte form and native signed pointer bits",
            direct.BaseDescriptorCode == 60 && direct.IndexDescriptorCode == 0 && direct.Width == 64
            && scaled.IndexDescriptorCode == 59 && scaled.Scale == 8
            && direct.SignedDwordBaseAddressBits == 0xFFFFFFFF80001000
            && Matches(NativeGeneratedArithmeticTemplates.TableSourceForModeOne(NativeGeneratedOperationCode.Add, direct), "48031F")
            && Matches(NativeGeneratedArithmeticTemplates.TableSourceForModeOne(NativeGeneratedOperationCode.Subtract, scaled), "482B1CF7")
            && Matches(NativeGeneratedArithmeticTemplates.TableSourceForModeOne(NativeGeneratedOperationCode.Xor, narrow), "321CF7"));
        bool bounded = true;
        for (uint parameter = 0; parameter < 65536; parameter++)
        {
            var source = NativeGeneratedBodyDecisionRules.ArithmeticTableSourceForModeOne(parameter, 0xFFFFFFF0);
            bounded &= source.EffectiveTableOffset < 256
                && source.EffectiveTableOffset + source.Width / 8 <= NativeGeneratedTransformProviderPrefix.TableLength
                && source.BaseAddress == unchecked(0xFFFFFFF0 + source.BaseOffset);
        }
        check("Mode1 table-source decision projection bounds byte/64-bit reads inside264 bytes over65536 parameter patterns", bounded);
        bool invalidSourceRejected = false;
        try { NativeGeneratedArithmeticTemplates.RegisterSourceForModeOne(NativeGeneratedOperationCode.Add, rcx with { SourceDescriptorCode = 38 }); }
        catch (ArgumentOutOfRangeException) { invalidSourceRejected = true; }
        check("Mode1 arithmetic catalog rejects mode0 source descriptors rather than silently narrowing them", invalidSourceRejected);
    }
}

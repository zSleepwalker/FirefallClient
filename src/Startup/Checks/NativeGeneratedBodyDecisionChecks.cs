using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedBodyDecisionChecks
{
    public static void Run(Action<string, bool> check)
    {
        uint[] forward = [1, 41, 6, 5, 34, 35, 43];
        uint[] reverse = [41, 1, 5, 6, 35, 34, 43];
        bool pairs = true;
        for (uint sample = 0; sample < 21; sample++)
        {
            var pair = NativeGeneratedBodyDecisionRules.PairFromConsumedSample(sample);
            pairs &= (uint)pair.Forward == forward[sample % 7] && (uint)pair.Reverse == reverse[sample % 7];
        }
        check("Native generated seven operation pairs retain modulo order and inverse opcode association", pairs);
        check("Native initial scratch parity selects BL/EBX while rotate count parity selects CL/ECX",
            NativeGeneratedBodyDecisionRules.ScratchFromConsumedParitySample(0) == new NativeGeneratedScratchChoice(40, 32)
            && NativeGeneratedBodyDecisionRules.ScratchFromConsumedParitySample(uint.MaxValue) == new NativeGeneratedScratchChoice(4, 8)
            && NativeGeneratedBodyDecisionRules.RotateCountRegisterFromConsumedParitySample(0) == new NativeGeneratedScratchChoice(38, 32)
            && NativeGeneratedBodyDecisionRules.RotateCountRegisterFromConsumedParitySample(1) == new NativeGeneratedScratchChoice(2, 8)
            && NativeGeneratedBodyDecisionRules.RotateUsesCountRegisterFromConsumedSample(4)
            && !NativeGeneratedBodyDecisionRules.RotateUsesCountRegisterFromConsumedSample(5));
        var counter8 = NativeGeneratedBodyDecisionRules.ArithmeticRegisterSource(1, 1);
        var immediate32 = NativeGeneratedBodyDecisionRules.ArithmeticRegisterSource(0x12345678, 0);
        check("Native register arithmetic uses DL/EDX counter on odd parameter or loads CL/ECX on even parameter",
            counter8.SourceDescriptorCode == 3 && !counter8.RequiresImmediateMove
            && immediate32.SourceDescriptorCode == 38 && immediate32.RequiresImmediateMove
            && immediate32.ImmediateValue == 0x12345678);
        bool tableOffsets = true;
        for (uint parameter = 0; parameter < 65536; parameter++)
        {
            var choice = NativeGeneratedBodyDecisionRules.ArithmeticTableSource(parameter, 0x10000000);
            tableOffsets &= choice.EffectiveTableOffset < 256
                && choice.BaseAddress == 0x10000000 + choice.BaseOffset
                && choice.BaseDescriptorCode == 44
                && choice.IndexDescriptorCode == (choice.Scale == 1 ? 0u : 43u)
                && choice.Width == ((parameter & 1) == 0 ? 32 : 8);
        }
        var edge = NativeGeneratedBodyDecisionRules.ArithmeticTableSource(uint.MaxValue, 0xFFFFFFFC);
        check("Native table arithmetic maps scale/index/window into first256 bytes and retains pointer wrapping",
            tableOffsets && edge is { Scale: 8, IndexValue: 31, BaseOffset: 7, BaseAddress: 3, EffectiveTableOffset: 255 });
    }
}

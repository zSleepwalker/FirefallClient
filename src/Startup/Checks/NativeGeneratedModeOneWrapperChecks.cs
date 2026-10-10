using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedModeOneWrapperChecks
{
    public static void Run(Action<string, bool> check)
    {
        int calls = 0;
        var wrapper = NativeGeneratedByteLoopWrapper.AssembleCapturedBodyForModeOne(new byte[] { 0xAA }, () =>
        {
            calls++;
            return 0;
        });
        check("Mode1 captured-body wrapper consumes21 selections and reproduces exact64-bit register/count loop bytes",
            calls == NativeGeneratedByteLoopWrapper.ModeOneUInt32CallsConsumed
            && wrapper.ConsumedUInt32Samples.Count == 21
            && Convert.ToHexString(wrapper.Bytes.Span)
                == "505152535657488B44E420488B54E4184809D2740FAA881848FFC048FFCA0F85F1FFFFFF5F5E5B5A5958C3");
        calls = 0;
        var scripted = NativeGeneratedByteLoopWrapper.AssembleCapturedBodyForModeOne(new byte[] { 0xAA }, () => (uint)calls++);
        check("Mode1 wrapper preserves one draw per singleton, condition draw8, skip draw13 and closing draws14..20",
            calls == 21
            && scripted.ConsumedUInt32Samples.SequenceEqual(Enumerable.Range(0, 21).Select(i => (uint)i))
            && Convert.ToHexString(scripted.Bytes.Span)
                == "505152535657488B44E420488B54E4184809D20F840F000000AA881848FFC048FFCA0F85F1FFFFFF5F5E5B5A5958C3");
        var alternative = NativeGeneratedByteLoopWrapper.AssembleCapturedBodyForModeOne(new byte[] { 0xAA }, () => 1);
        check("Mode1 wrapper alternate OR form and near positive skip retain original branch-offset rules",
            Convert.ToHexString(alternative.Bytes.Span)
                == "505152535657488B44E420488B54E418480BD20F840F000000AA881848FFC048FFCA0F85F1FFFFFF5F5E5B5A5958C3");
        bool boundaryLengths = true;
        foreach (int length in new[] { 113, 114, 115, 116, 117, 128, 241, 242 })
        {
            byte[] body = Enumerable.Repeat((byte)0xAA, length).ToArray();
            var image = NativeGeneratedByteLoopWrapper.AssembleCapturedBodyForModeOne(body, () => 0);
            int skipOffset = length + 14;
            bool shortSkip = skipOffset <= 255 && skipOffset is not (128 or 129);
            string hex = Convert.ToHexString(image.Bytes.Span);
            string expectedSkip = shortSkip ? $"74{unchecked((byte)skipOffset):X2}"
                : "0F84" + Convert.ToHexString(NativeGeneratedImmediateBytes.Project((uint)skipOffset, 32).Bytes!.Value.Span);
            boundaryLengths &= hex.StartsWith("505152535657488B44E420488B54E4184809D2" + expectedSkip, StringComparison.Ordinal)
                && image.Bytes.Length == 42 + length + (shortSkip ? 0 : 4)
                && image.ConsumedUInt32Samples.Count == 21;
        }
        check("Mode1 wrapper retains skip128/129 exclusion, wrapped130..255 short offsets and256 near boundary", boundaryLengths);
    }
}

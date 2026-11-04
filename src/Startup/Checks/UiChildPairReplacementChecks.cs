using Firefall.Reconstruction.Startup;

internal static class UiChildPairReplacementChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI child pair replacement records the raw scratch and unconditional store anchors",
            UiChildPairReplacementProjectionRules.ScratchDwordCount == 16
            && UiChildPairReplacementProjectionRules.DestinationOffsetStart == 0x24
            && UiChildPairReplacementProjectionRules.DwordByteSize == 4
            && UiChildPairReplacementProjectionRules.LoopIterationCount == 8
            && UiChildPairReplacementProjectionRules.LoopStrideByteCount == 8
            && UiChildPairReplacementProjectionRules.LoopIndexZeroFileOffset == 0xCA6EEA
            && UiChildPairReplacementProjectionRules.DestinationCursorSetupFileOffset == 0xCA6EEC
            && UiChildPairReplacementProjectionRules.LoopStartFileOffset == 0xCA6EF0
            && UiChildPairReplacementProjectionRules.LoopEndFileOffsetInclusive == 0xCA6F2A
            && UiChildPairReplacementProjectionRules.FirstStoreFileOffset == 0xCA6F0A
            && UiChildPairReplacementProjectionRules.SecondStoreFileOffset == 0xCA6F22);

        var capturedInput = Enumerable.Range(0, UiChildPairReplacementProjectionRules.ScratchDwordCount)
            .Select(index => 0x80000000u | unchecked((uint)(0x10203040 + index * 0x01010101)))
            .ToList();
        var expectedValues = capturedInput.ToArray();
        var writes = UiChildPairReplacementProjectionRules.Project(capturedInput);
        bool orderedOffsetsAndExactBits = writes.Count == 16
            && writes.Select((write, index) =>
                write.DestinationOffset == 0x24 + index * 4
                && write.Value == expectedValues[index]).All(matches => matches)
            && writes[15].DestinationOffset == 0x60;
        check("UI child pair replacement emits sixteen sequential DWORD writes with every captured bit preserved",
            orderedOffsetsAndExactBits);

        for (int index = 0; index < capturedInput.Count; index++)
            capturedInput[index] = 0;
        check("UI child pair replacement copies captured input values and does not expose a mutable write list",
            writes.Select(write => write.Value).SequenceEqual(expectedValues)
            && ThrowsNotSupported(() =>
                ((IList<UiChildPairReplacementWrite>)writes)[0] = default));

        check("UI child pair replacement accepts arbitrary captured first-pair values without imposing memset state",
            UiChildPairReplacementProjectionRules.Project(
                new uint[]
                {
                    0xFFFFFFFF, 0x80000000, 2, 3, 4, 5, 6, 7,
                    8, 9, 10, 11, 12, 13, 14, 15
                })[0] == new UiChildPairReplacementWrite(0x24, 0xFFFFFFFF)
            && UiChildPairReplacementProjectionRules.Project(
                new uint[]
                {
                    0xFFFFFFFF, 0x80000000, 2, 3, 4, 5, 6, 7,
                    8, 9, 10, 11, 12, 13, 14, 15
                })[1] == new UiChildPairReplacementWrite(0x28, 0x80000000));

        check("UI child pair replacement requires a non-null input containing exactly sixteen DWORDs",
            ThrowsArgumentException(() => UiChildPairReplacementProjectionRules.Project(null))
            && ThrowsArgumentException(() =>
                UiChildPairReplacementProjectionRules.Project(new uint[15]))
            && ThrowsArgumentException(() =>
                UiChildPairReplacementProjectionRules.Project(new uint[17])));
    }

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }

    private static bool ThrowsNotSupported(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (NotSupportedException)
        {
            return true;
        }
    }
}

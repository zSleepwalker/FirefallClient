namespace Firefall.Reconstruction.Startup;

public readonly record struct UiChildPairReplacementWrite(
    int DestinationOffset,
    uint Value);

public static class UiChildPairReplacementProjectionRules
{
    public const int ScratchDwordCount = 16;
    public const int DestinationOffsetStart = 0x24;
    public const int DwordByteSize = 4;
    public const int LoopIterationCount = 8;
    public const int LoopStrideByteCount = 8;
    // These are raw file offsets, not runtime virtual addresses.
    public const uint LoopIndexZeroFileOffset = 0xCA6EEA;
    public const uint DestinationCursorSetupFileOffset = 0xCA6EEC;
    public const uint LoopStartFileOffset = 0xCA6EF0;
    public const uint LoopEndFileOffsetInclusive = 0xCA6F2A;
    public const uint FirstStoreFileOffset = 0xCA6F0A;
    public const uint SecondStoreFileOffset = 0xCA6F22;

    public static IReadOnlyList<UiChildPairReplacementWrite> Project(
        IReadOnlyList<uint>? capturedScratchDwords)
    {
        ArgumentNullException.ThrowIfNull(capturedScratchDwords);
        if (capturedScratchDwords.Count != ScratchDwordCount)
            throw new ArgumentException(
                $"Exactly {ScratchDwordCount} captured scratch DWORDs are required.",
                nameof(capturedScratchDwords));

        var writes = new UiChildPairReplacementWrite[ScratchDwordCount];
        for (int index = 0; index < writes.Length; index++)
        {
            writes[index] = new UiChildPairReplacementWrite(
                DestinationOffsetStart + DwordByteSize * index,
                capturedScratchDwords[index]);
        }

        return Array.AsReadOnly(writes);
    }
}

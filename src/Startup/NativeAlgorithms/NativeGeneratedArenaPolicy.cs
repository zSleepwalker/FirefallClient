namespace Firefall.Reconstruction.Startup;

public readonly record struct NativeGeneratedArenaRequest(
    uint PayloadLength, uint RequiredBlockSize, uint PreferredListOffset, uint NewRegionSize);

/// <summary>HeaderAddress is a captured numeric token, never a managed/native pointer to dereference.</summary>
public readonly record struct NativeGeneratedArenaFreeBlock(uint HeaderAddress, uint BlockSize);

public readonly record struct NativeGeneratedArenaSelection(
    NativeGeneratedArenaRequest Request, uint? SelectedListOffset, int? SelectedIndex);

public readonly record struct NativeGeneratedArenaReuse(
    uint PayloadAddress, uint RetainedBlockSize, NativeGeneratedArenaFreeBlock? Remainder);

/// <summary>Numerical arena request/search/split projection only. No locks, VirtualAlloc, native writes or releases.</summary>
public static class NativeGeneratedArenaPolicy
{
    public const uint AllocateFileOffset = 0x1250F90;
    public const uint FindBestFitFileOffset = 0x1251050;
    public const uint AllocateRegionFileOffset = 0x12510F0;
    public const uint ReturnBlockFileOffset = 0x1251160;
    public const uint HeaderSize = 16;
    public const uint MinimumSplitRemainder = 32;
    public const uint LargeListOffset = 100;
    public const uint NativeReserveAndCommit = 0x3000;
    public const uint NativePageExecuteReadWrite = 0x40;

    public static NativeGeneratedArenaRequest ProjectRequest(uint payloadLength)
    {
        uint required = unchecked(payloadLength + HeaderSize);
        return new(payloadLength, required, ListOffsetForBlockSize(required),
            unchecked(required + 0xFFFF) & 0xFFFF0000);
    }

    public static uint ListOffsetForBlockSize(uint blockSize) => blockSize switch
    {
        <= 64 => 4,
        <= 128 => 36,
        <= 256 => 68,
        _ => LargeListOffset
    };

    public static int? FindBestFit(ReadOnlySpan<NativeGeneratedArenaFreeBlock> capturedList, uint requiredBlockSize)
    {
        int? selected = null;
        for (int i = 0; i < capturedList.Length; i++)
        {
            uint size = capturedList[i].BlockSize;
            if (size >= requiredBlockSize && (selected is null || size < capturedList[selected.Value].BlockSize))
                selected = i;
        }
        return selected;
    }

    public static NativeGeneratedArenaSelection SelectCapturedFreeBlock(uint payloadLength,
        ReadOnlySpan<NativeGeneratedArenaFreeBlock> capturedPreferredList,
        ReadOnlySpan<NativeGeneratedArenaFreeBlock> capturedLargeList)
    {
        var request = ProjectRequest(payloadLength);
        if (request.PreferredListOffset != LargeListOffset)
        {
            int? small = FindBestFit(capturedPreferredList, request.RequiredBlockSize);
            if (small is not null)
                return new(request, request.PreferredListOffset, small);
        }
        int? large = FindBestFit(capturedLargeList, request.RequiredBlockSize);
        return new(request, large is not null ? LargeListOffset : null, large);
    }

    public static NativeGeneratedArenaReuse ProjectCapturedReuse(
        NativeGeneratedArenaFreeBlock block, uint requiredBlockSize)
    {
        if (block.BlockSize < requiredBlockSize)
            throw new ArgumentOutOfRangeException(nameof(block), "Captured consistent free block must satisfy the native best-fit gate.");
        uint remaining = block.BlockSize - requiredBlockSize;
        NativeGeneratedArenaFreeBlock? remainder = remaining >= MinimumSplitRemainder
            ? new(unchecked(block.HeaderAddress + requiredBlockSize), remaining)
            : null;
        return new(unchecked(block.HeaderAddress + HeaderSize),
            remainder is not null ? requiredBlockSize : block.BlockSize, remainder);
    }
}

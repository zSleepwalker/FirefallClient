using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedArenaPolicyChecks
{
    public static void Run(Action<string, bool> check)
    {
        uint[] payloadLengths = [0, 48, 49, 112, 113, 240, 241, 65520, 65521, 0xFFFFFFEF, 0xFFFFFFF0, uint.MaxValue];
        uint[] required = [16, 64, 65, 128, 129, 256, 257, 65536, 65537, uint.MaxValue, 0, 15];
        uint[] lists = [4, 4, 36, 36, 68, 68, 100, 100, 100, 100, 4, 4];
        uint[] regions = [65536, 65536, 65536, 65536, 65536, 65536, 65536, 65536, 131072, 0, 0, 65536];
        bool requests = true;
        for (int i = 0; i < payloadLengths.Length; i++)
        {
            var request = NativeGeneratedArenaPolicy.ProjectRequest(payloadLengths[i]);
            requests &= request.RequiredBlockSize == required[i] && request.PreferredListOffset == lists[i] && request.NewRegionSize == regions[i];
        }
        check("Native generated arena preserves16-byte headers,64/128/256 buckets,64KiB rounding and DWORD wrap", requests);
        check("Native arena return facts stay path-scoped and distinguish generic VirtualFree from output-region release",
            NativeGeneratedArenaPolicy.FreeListNodeInsertFileOffset == 0x1251BD0
            && NativeGeneratedArenaPolicy.FreeListNodeAllocateFileOffset == 0x1251900
            && NativeGeneratedArenaPolicy.GeneralAllocatorReleaseFileOffset == 0x12C8D60
            && NativeGeneratedArenaPolicy.VirtualFreeImportSlotVirtualAddress == 0x1AC42B4
            && !NativeGeneratedArenaPolicy.ObservedReturnBlockPathCoalescesBlocks
            && !NativeGeneratedArenaPolicy.ObservedReturnBlockPathReleasesWholeRegion);
        NativeGeneratedArenaFreeBlock[] preferred = [new(0x1000, 63), new(0x2000, 64), new(0x3000, 64)];
        NativeGeneratedArenaFreeBlock[] large = [new(0x4000, 400), new(0x5000, 300), new(0x6000, 300)];
        var small = NativeGeneratedArenaPolicy.SelectCapturedFreeBlock(48, preferred, large);
        var fallback = NativeGeneratedArenaPolicy.SelectCapturedFreeBlock(49, preferred, large);
        var onlyLarge = NativeGeneratedArenaPolicy.SelectCapturedFreeBlock(241, preferred, large);
        check("Native generated arena searches preferred before large, chooses smallest fit and retains first equal-size entry",
            small.SelectedListOffset == 4 && small.SelectedIndex == 1
            && fallback.SelectedListOffset == 100 && fallback.SelectedIndex == 1
            && onlyLarge.SelectedListOffset == 100 && onlyLarge.SelectedIndex == 1
            && NativeGeneratedArenaPolicy.FindBestFit(large, 401) is null);
        var whole = NativeGeneratedArenaPolicy.ProjectCapturedReuse(new(0xFFFFFFF0, 95), 64);
        var split = NativeGeneratedArenaPolicy.ProjectCapturedReuse(new(0xFFFFFFF0, 96), 64);
        check("Native generated arena retains31-byte tails and splits exactly32-byte remainders with numerical address wrap",
            whole.PayloadAddress == 0 && whole.RetainedBlockSize == 95 && whole.Remainder is null
            && split.PayloadAddress == 0 && split.RetainedBlockSize == 64
            && split.Remainder == new NativeGeneratedArenaFreeBlock(0x30, 32)
            && NativeGeneratedArenaPolicy.ListOffsetForBlockSize(split.Remainder.Value.BlockSize) == 4);
        var releases = NativeGeneratedProviderLifetimePlan.FromCapturedPointers(0x11, 0x22, 0x33, 0x44);
        var nullReleases = NativeGeneratedProviderLifetimePlan.FromCapturedPointers(0, 0, 0, 0);
        check("Native provider lifetime releases owned RNG/table before both arena outputs and retains unconditional output release calls",
            releases.SequenceEqual(new NativeGeneratedProviderReleaseStep[]
            {
                new(NativeGeneratedProviderReleaseKind.RngDeletingDestructorSlot4WithArgument1, 0x11),
                new(NativeGeneratedProviderReleaseKind.FreeTableAllocation, 0x22),
                new(NativeGeneratedProviderReleaseKind.ReturnFirstOutputToArena, 0x33),
                new(NativeGeneratedProviderReleaseKind.ReturnSecondOutputToArena, 0x44)
            })
            && nullReleases.Count == 2
            && nullReleases.All(step => step.CapturedAddress == 0)
            && NativeGeneratedProviderLifetimePlan.CallbackStackBytesPopped == 8
            && !NativeGeneratedProviderLifetimePlan.ObservedConsumersUseCallbackReturnValue);
    }
}

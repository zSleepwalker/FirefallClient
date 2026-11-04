namespace Firefall.Reconstruction.Startup;

public enum UiShaderSourceAllocationBranch
{
    Pool,
    NonPool
}

public readonly record struct UiShaderSourceAllocationOutputPair(
    uint HeaderPointer,
    uint ControlPointer);

public readonly record struct UiShaderSourceAllocationProjection(
    UiShaderSourceAllocationBranch Branch,
    UiShaderSourceAllocationOutputPair EntryOutputPair);

public static class UiShaderSourceAllocationProjectionRules
{
    public const uint AllocatorEntryRawFileOffset = 0x12CBF30;
    public const uint OutputHeaderZeroStoreRawFileOffset = 0x12CBF65;
    public const uint OutputControlZeroStoreRawFileOffset = 0x12CBF6B;
    public const uint PoolSizeCompareRawFileOffset = 0x12CBF80;
    public const uint AlignmentCompareRawFileOffset = 0x12CBF88;
    public const uint MaximumPooledRequestSize = 0x800;

    public const uint CallerOutputPairAddressSetupRawFileOffset = 0x12CBDA2;
    public const uint CallerOutputHeaderLoadRawFileOffset = 0x12CBDDB;
    public const uint CallerOutputControlLoadRawFileOffset = 0x12CBE03;
    public const uint CallerOutputControlStoreRawFileOffset = 0x12CBE07;
    public const uint CallerOutputHeaderStoreRawFileOffset = 0x12CBE15;

    public const int OutputPairHeaderPointerOffset = 0;
    public const int OutputPairControlPointerOffset = 0x04;
    public const int CallerHeaderFieldOffset = 0x04;
    public const int CallerControlFieldOffset = 0x08;
    public const int HeaderCapacityOffset = 0;
    public const int HeaderDataPointerOffset = 0x04;
    public const int HeaderLengthOffset = 0x0C;

    /// <summary>
    /// Selects the path and records only the pair's entry-time zero initialization.
    /// This does not model later helper writes or imply allocation success.
    /// </summary>
    public static UiShaderSourceAllocationProjection Project(
        uint requestedSize,
        uint alignment)
    {
        UiShaderSourceAllocationBranch branch =
            requestedSize <= MaximumPooledRequestSize && alignment == 0
                ? UiShaderSourceAllocationBranch.Pool
                : UiShaderSourceAllocationBranch.NonPool;

        return new UiShaderSourceAllocationProjection(
            branch,
            default);
    }
}

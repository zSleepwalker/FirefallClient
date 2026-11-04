using Firefall.Reconstruction.Startup;

internal static class UiShaderSourceAllocationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Shader-source allocation records entry zeroing and caller pair mapping",
            UiShaderSourceAllocationProjectionRules.AllocatorEntryRawFileOffset == 0x12CBF30
            && UiShaderSourceAllocationProjectionRules.OutputHeaderZeroStoreRawFileOffset == 0x12CBF65
            && UiShaderSourceAllocationProjectionRules.OutputControlZeroStoreRawFileOffset == 0x12CBF6B
            && UiShaderSourceAllocationProjectionRules.CallerOutputPairAddressSetupRawFileOffset == 0x12CBDA2
            && UiShaderSourceAllocationProjectionRules.CallerOutputHeaderLoadRawFileOffset == 0x12CBDDB
            && UiShaderSourceAllocationProjectionRules.CallerOutputControlLoadRawFileOffset == 0x12CBE03
            && UiShaderSourceAllocationProjectionRules.CallerOutputControlStoreRawFileOffset == 0x12CBE07
            && UiShaderSourceAllocationProjectionRules.CallerOutputHeaderStoreRawFileOffset == 0x12CBE15
            && UiShaderSourceAllocationProjectionRules.OutputPairHeaderPointerOffset == 0
            && UiShaderSourceAllocationProjectionRules.OutputPairControlPointerOffset == 4
            && UiShaderSourceAllocationProjectionRules.CallerHeaderFieldOffset == 4
            && UiShaderSourceAllocationProjectionRules.CallerControlFieldOffset == 8
            && UiShaderSourceAllocationProjectionRules.HeaderCapacityOffset == 0
            && UiShaderSourceAllocationProjectionRules.HeaderDataPointerOffset == 4
            && UiShaderSourceAllocationProjectionRules.HeaderLengthOffset == 0x0C);

        (uint RequestedSize, uint Alignment, UiShaderSourceAllocationBranch Expected)[] cases =
        {
            (0, 0, UiShaderSourceAllocationBranch.Pool),
            (0, 1, UiShaderSourceAllocationBranch.NonPool),
            (0, uint.MaxValue, UiShaderSourceAllocationBranch.NonPool),
            (0x800, 0, UiShaderSourceAllocationBranch.Pool),
            (0x800, 1, UiShaderSourceAllocationBranch.NonPool),
            (0x800, uint.MaxValue, UiShaderSourceAllocationBranch.NonPool),
            (0x801, 0, UiShaderSourceAllocationBranch.NonPool),
            (0x801, 1, UiShaderSourceAllocationBranch.NonPool),
            (0x801, uint.MaxValue, UiShaderSourceAllocationBranch.NonPool),
            (uint.MaxValue, 0, UiShaderSourceAllocationBranch.NonPool),
            (uint.MaxValue, 1, UiShaderSourceAllocationBranch.NonPool),
            (uint.MaxValue, uint.MaxValue, UiShaderSourceAllocationBranch.NonPool)
        };

        bool branchSelectionMatchesUnsignedGate = true;
        bool entryOutputPairIsZero = true;
        foreach (var item in cases)
        {
            UiShaderSourceAllocationProjection projection =
                UiShaderSourceAllocationProjectionRules.Project(
                    item.RequestedSize,
                    item.Alignment);
            branchSelectionMatchesUnsignedGate &=
                projection.Branch == item.Expected;
            entryOutputPairIsZero &=
                projection.EntryOutputPair == default;
        }

        check("Shader-source allocation selects pool only for unsigned size at most 0x800 and zero alignment",
            UiShaderSourceAllocationProjectionRules.MaximumPooledRequestSize == 0x800
            && UiShaderSourceAllocationProjectionRules.PoolSizeCompareRawFileOffset == 0x12CBF80
            && UiShaderSourceAllocationProjectionRules.AlignmentCompareRawFileOffset == 0x12CBF88
            && branchSelectionMatchesUnsignedGate);

        check("Shader-source allocation projection records only the zeroed entry pair",
            entryOutputPairIsZero
            && UiShaderSourceAllocationProjectionRules.Project(0, 0).EntryOutputPair
                is { HeaderPointer: 0, ControlPointer: 0 });
    }
}

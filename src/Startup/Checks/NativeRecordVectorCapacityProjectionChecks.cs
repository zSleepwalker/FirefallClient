using Firefall.Reconstruction.Startup;

internal static class NativeRecordVectorCapacityProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Native record-vector append evidence records the helper, full-capacity path, stride, and maximum count",
            NativeRecordVectorCapacityProjectionRules.AppendMethodFileOffset == 0xC97380
            && NativeRecordVectorCapacityProjectionRules.FullCapacityPathFileOffset == 0xC978F0
            && NativeRecordVectorCapacityProjectionRules.ReallocationMethodFileOffset == 0xC97FA0
            && NativeRecordVectorCapacityProjectionRules.GrowthAllocationFunctionFileOffset == 0x12B7FB0
            && NativeRecordVectorCapacityProjectionRules.AllocationServiceFunctionFileOffset == 0x12C8A20
            && NativeRecordVectorCapacityProjectionRules.RecordStride == 16
            && NativeRecordVectorCapacityProjectionRules.MaximumCount == 0x0FFFFFFF);

        check("Native record-vector failure evidence names both CRT imports, the length message, and their callsites",
            NativeRecordVectorCapacityProjectionRules.LengthErrorCallSiteFileOffset == 0xC9791C
            && NativeRecordVectorCapacityProjectionRules.LengthErrorImportAddressVirtualAddress == 0x1AC45C0
            && NativeRecordVectorCapacityProjectionRules.LengthErrorImportModule == "MSVCP120.dll"
            && NativeRecordVectorCapacityProjectionRules.LengthErrorImportName == "std::_Xlength_error(char const*)"
            && NativeRecordVectorCapacityProjectionRules.LengthErrorImportDecoratedName == "?_Xlength_error@std@@YAXPBD@Z"
            && NativeRecordVectorCapacityProjectionRules.LengthErrorMessageVirtualAddress == 0x1C5EA14
            && NativeRecordVectorCapacityProjectionRules.LengthErrorMessage == "vector<T> too long"
            && NativeRecordVectorCapacityProjectionRules.BadAllocCallSiteFileOffset == 0xC97FFB
            && NativeRecordVectorCapacityProjectionRules.BadAllocImportAddressVirtualAddress == 0x1AC4424
            && NativeRecordVectorCapacityProjectionRules.BadAllocImportModule == "MSVCP120.dll"
            && NativeRecordVectorCapacityProjectionRules.BadAllocImportName == "std::_Xbad_alloc(void)"
            && NativeRecordVectorCapacityProjectionRules.BadAllocImportDecoratedName == "?_Xbad_alloc@std@@YAXXZ");

        bool smallCounts = new (uint Used, uint Capacity, uint Required, uint Projected)[]
        {
            (0, 0, 1, 1),
            (1, 1, 2, 2),
            (3, 3, 4, 4),
            (4, 4, 5, 6)
        }.All(test => NativeRecordVectorCapacityProjectionRules.Project(test.Used, test.Capacity)
            == new NativeRecordVectorCapacityProjection(
                NativeRecordVectorCapacityDecision.CapacityArithmeticProjected,
                test.Required,
                test.Projected,
                test.Projected * 16));
        check("Native record-vector count projection covers zero, one, odd, and even capacities", smallCounts);

        var requiredWins = NativeRecordVectorCapacityProjectionRules.Project(1, 1);
        check("Native record-vector required count can exceed the old-capacity-plus-half candidate",
            requiredWins.RequiredCount == 2 && requiredWins.ProjectedCapacityCount == 2);

        uint thresholdCapacity = (uint)(
            (ulong)NativeRecordVectorCapacityProjectionRules.MaximumCount * 2 / 3);
        var belowThreshold = NativeRecordVectorCapacityProjectionRules.Project(
            thresholdCapacity - 1, thresholdCapacity - 1);
        var atThreshold = NativeRecordVectorCapacityProjectionRules.Project(
            thresholdCapacity, thresholdCapacity);
        var aboveThreshold = NativeRecordVectorCapacityProjectionRules.Project(
            thresholdCapacity + 1, thresholdCapacity + 1);
        check("Native record-vector half-capacity addition respects the exact maximum-count overflow threshold",
            thresholdCapacity == 178956970
            && belowThreshold.ProjectedCapacityCount == thresholdCapacity - 1 + (thresholdCapacity - 1) / 2
            && atThreshold.ProjectedCapacityCount == NativeRecordVectorCapacityProjectionRules.MaximumCount
            && aboveThreshold.ProjectedCapacityCount == thresholdCapacity + 2);

        var maximumUsed = NativeRecordVectorCapacityProjectionRules.Project(
            NativeRecordVectorCapacityProjectionRules.MaximumCount,
            NativeRecordVectorCapacityProjectionRules.MaximumCount);
        check("Native record-vector projection marks maximum used count at the guard without asserting a native exception or return",
            maximumUsed == new NativeRecordVectorCapacityProjection(
                NativeRecordVectorCapacityDecision.MaximumUsedCountGuardInvoked, null, null, null));

        bool maximumBytesFit = NativeRecordVectorCapacityProjectionRules.RequiredBytes(
            NativeRecordVectorCapacityProjectionRules.MaximumCount) == 0xFFFFFFF0;
        check("Native record-vector maximum count multiplied by its 16-byte stride fits in uint",
            maximumBytesFit);

        bool usedRejected = false, capacityRejected = false, bytesRejected = false;
        try
        {
            NativeRecordVectorCapacityProjectionRules.Project(
                NativeRecordVectorCapacityProjectionRules.MaximumCount + 1, 0);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            usedRejected = exception.ParamName == "usedCount";
        }
        try
        {
            NativeRecordVectorCapacityProjectionRules.Project(
                0, NativeRecordVectorCapacityProjectionRules.MaximumCount + 1);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            capacityRejected = exception.ParamName == "oldCapacityCount";
        }
        try
        {
            NativeRecordVectorCapacityProjectionRules.RequiredBytes(
                NativeRecordVectorCapacityProjectionRules.MaximumCount + 1);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            bytesRejected = exception.ParamName == "count";
        }
        check("Native record-vector count and byte projections reject values above the validated maximum",
            usedRejected && capacityRejected && bytesRejected);
    }
}

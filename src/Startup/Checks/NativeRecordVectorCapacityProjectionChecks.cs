using Firefall.Reconstruction.Startup;

internal static class NativeRecordVectorCapacityProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Native record-vector append evidence records the helper, full-capacity path, stride, and maximum count",
            NativeRecordVectorCapacityProjectionRules.AppendMethodFileOffset == 0xC97380
            && NativeRecordVectorCapacityProjectionRules.FullCapacityPathFileOffset == 0xC978F0
            && NativeRecordVectorCapacityProjectionRules.RecordStride == 16
            && NativeRecordVectorCapacityProjectionRules.MaximumCount == 0x0FFFFFFF);

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
        check("Native record-vector maximum used count reports the guard before increment without modeling its effect",
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

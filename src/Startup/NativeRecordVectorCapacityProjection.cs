namespace Firefall.Reconstruction.Startup;

public enum NativeRecordVectorCapacityDecision
{
    CapacityArithmeticProjected,
    MaximumUsedCountGuardInvoked
}

public readonly record struct NativeRecordVectorCapacityProjection(
    NativeRecordVectorCapacityDecision Decision,
    uint? RequiredCount,
    uint? ProjectedCapacityCount,
    uint? ProjectedCapacityBytes);

/// <summary>
/// Projects bounded count arithmetic for the 16-byte-record vector without modeling allocation or pointers.
/// </summary>
public static class NativeRecordVectorCapacityProjectionRules
{
    public const uint AppendMethodFileOffset = 0xC97380;
    public const uint FullCapacityPathFileOffset = 0xC978F0;
    public const uint ReallocationMethodFileOffset = 0xC97FA0;
    public const uint GrowthAllocationFunctionFileOffset = 0x12B7FB0;
    public const uint AllocationServiceFunctionFileOffset = 0x12C8A20;
    public const uint LengthErrorCallSiteFileOffset = 0xC9791C;
    public const uint LengthErrorImportAddressVirtualAddress = 0x1AC45C0;
    public const string LengthErrorImportModule = "MSVCP120.dll";
    public const string LengthErrorImportName = "std::_Xlength_error(char const*)";
    public const string LengthErrorImportDecoratedName = "?_Xlength_error@std@@YAXPBD@Z";
    public const uint LengthErrorMessageVirtualAddress = 0x1C5EA14;
    public const string LengthErrorMessage = "vector<T> too long";
    public const uint BadAllocCallSiteFileOffset = 0xC97FFB;
    public const uint BadAllocImportAddressVirtualAddress = 0x1AC4424;
    public const string BadAllocImportModule = "MSVCP120.dll";
    public const string BadAllocImportName = "std::_Xbad_alloc(void)";
    public const string BadAllocImportDecoratedName = "?_Xbad_alloc@std@@YAXXZ";
    public const uint RecordStride = 16;
    public const uint MaximumCount = 0x0FFFFFFF;

    public static NativeRecordVectorCapacityProjection Project(uint usedCount, uint oldCapacityCount)
    {
        ValidateCount(usedCount, nameof(usedCount));
        ValidateCount(oldCapacityCount, nameof(oldCapacityCount));

        if (usedCount == MaximumCount)
            return new(NativeRecordVectorCapacityDecision.MaximumUsedCountGuardInvoked, null, null, null);

        uint requiredCount = usedCount + 1;
        uint halfCapacity = oldCapacityCount / 2;
        uint projectedCapacityCount = MaximumCount - halfCapacity >= oldCapacityCount
            ? Math.Max(requiredCount, oldCapacityCount + halfCapacity)
            : requiredCount;

        return new(
            NativeRecordVectorCapacityDecision.CapacityArithmeticProjected,
            requiredCount,
            projectedCapacityCount,
            RequiredBytes(projectedCapacityCount));
    }

    public static uint RequiredBytes(uint count)
    {
        ValidateCount(count, nameof(count));
        return count * RecordStride;
    }

    private static void ValidateCount(uint count, string parameterName)
    {
        if (count > MaximumCount)
            throw new ArgumentOutOfRangeException(parameterName, count, "Count exceeds the native vector maximum.");
    }
}

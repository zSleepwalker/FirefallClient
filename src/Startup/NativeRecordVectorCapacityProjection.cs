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

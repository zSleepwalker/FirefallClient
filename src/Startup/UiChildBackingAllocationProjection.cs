namespace Firefall.Reconstruction.Startup;

public readonly record struct UiChildBackingAllocationProjection(
    uint CapturedAllocationResultAtField64,
    uint Field68Value,
    uint Field6CValue,
    bool AllocationNullDiagnosticRequired,
    bool Field74DiagnosticRequired);

/// <summary>
/// Projects the captured stores following the join at raw offset CA6799. For a zero allocation result, this
/// projection is valid only when the null-allocation diagnostic was observed
/// to return; it does not execute or model that diagnostic.
/// </summary>
public static class UiChildBackingAllocationProjectionRules
{
    public const uint AllocationEntryAddress = 0xCA66F0;
    public const uint AllocatorCallAddress = 0xCA674D;
    public const uint AllocationNullTestAddress = 0xCA6754;
    public const uint AllocationNullDiagnosticBranchAddress = 0xCA6756;
    public const uint AllocationContinuationJoinFileOffset = 0xCA6799;
    public const uint Field74BranchAddress = 0xCA67B1;

    public const int CapturedAllocationResultFieldOffset = 0x64;
    public const int FixedField68Offset = 0x68;
    public const int FixedField6COffset = 0x6C;
    public const uint FixedField68Value = 0x20;
    public const uint FixedField6CValue = 0;
    public const int Field74Offset = 0x74;

    /// <summary>
    /// Projects the stores following the join at CA6799. If the allocator result is zero, a
    /// projection is returned only when the captured null-allocation
    /// diagnostic returned. For a nonzero result that diagnostic is skipped.
    /// The separate field-74 flag records whether the later diagnostic is
    /// required; neither diagnostic is executed here.
    /// </summary>
    public static UiChildBackingAllocationProjection? ProjectAtContinuation(
        uint capturedAllocationResult,
        uint capturedField74,
        bool capturedNullAllocationDiagnosticReturned)
    {
        if (capturedAllocationResult == 0 && !capturedNullAllocationDiagnosticReturned)
            return null;

        return new UiChildBackingAllocationProjection(
            capturedAllocationResult,
            FixedField68Value,
            FixedField6CValue,
            AllocationNullDiagnosticRequired: capturedAllocationResult == 0,
            Field74DiagnosticRequired: capturedField74 != 0);
    }
}

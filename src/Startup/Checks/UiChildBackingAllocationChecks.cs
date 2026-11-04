using Firefall.Reconstruction.Startup;

internal static class UiChildBackingAllocationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI child backing allocation records the continuation join, fields, and fixed stores",
            UiChildBackingAllocationProjectionRules.AllocationEntryAddress == 0xCA66F0
            && UiChildBackingAllocationProjectionRules.AllocatorCallAddress == 0xCA674D
            && UiChildBackingAllocationProjectionRules.AllocationNullTestAddress == 0xCA6754
            && UiChildBackingAllocationProjectionRules.AllocationNullDiagnosticBranchAddress == 0xCA6756
            && UiChildBackingAllocationProjectionRules.AllocationContinuationJoinFileOffset == 0xCA6799
            && UiChildBackingAllocationProjectionRules.Field74BranchAddress == 0xCA67B1
            && UiChildBackingAllocationProjectionRules.CapturedAllocationResultFieldOffset == 0x64
            && UiChildBackingAllocationProjectionRules.FixedField68Offset == 0x68
            && UiChildBackingAllocationProjectionRules.FixedField6COffset == 0x6C
            && UiChildBackingAllocationProjectionRules.FixedField68Value == 0x20
            && UiChildBackingAllocationProjectionRules.FixedField6CValue == 0
            && UiChildBackingAllocationProjectionRules.Field74Offset == 0x74);

        check("UI child backing allocation does not project the zero-result path unless its diagnostic returned",
            UiChildBackingAllocationProjectionRules.ProjectAtContinuation(0, 0, false) is null
            && UiChildBackingAllocationProjectionRules.ProjectAtContinuation(0, 1, false) is null);

        var zeroResult = UiChildBackingAllocationProjectionRules.ProjectAtContinuation(0, 0, true);
        check("UI child backing allocation projects a captured zero result after diagnostic return",
            zeroResult == new UiChildBackingAllocationProjection(
                0, 0x20, 0, AllocationNullDiagnosticRequired: true, Field74DiagnosticRequired: false));

        var highBitResult = UiChildBackingAllocationProjectionRules.ProjectAtContinuation(
            0x80000000, 0, capturedNullAllocationDiagnosticReturned: false);
        check("UI child backing allocation preserves a high-bit nonzero result and fixed stores",
            highBitResult == new UiChildBackingAllocationProjection(
                0x80000000, 0x20, 0, AllocationNullDiagnosticRequired: false, Field74DiagnosticRequired: false));

        var nonzeroResult = UiChildBackingAllocationProjectionRules.ProjectAtContinuation(
            0x12345678, 0xFFFFFFFF, capturedNullAllocationDiagnosticReturned: false);
        check("UI child backing allocation keeps the later field-74 diagnostic gate independent of allocation result",
            nonzeroResult == new UiChildBackingAllocationProjection(
                0x12345678, 0x20, 0, AllocationNullDiagnosticRequired: false, Field74DiagnosticRequired: true));

        var zeroResultWithField74 = UiChildBackingAllocationProjectionRules.ProjectAtContinuation(
            0, 0x80000000, capturedNullAllocationDiagnosticReturned: true);
        check("UI child backing allocation records both independent diagnostics when their captured gates require them",
            zeroResultWithField74 == new UiChildBackingAllocationProjection(
                0, 0x20, 0, AllocationNullDiagnosticRequired: true, Field74DiagnosticRequired: true));
    }
}

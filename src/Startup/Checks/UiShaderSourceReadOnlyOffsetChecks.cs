using Firefall.Reconstruction.Startup;

internal static class UiShaderSourceReadOnlyOffsetChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Shader-source read-only getter and contrasting preserve-helper anchors are recorded",
            UiShaderSourceReadOnlyOffsetProjectionRules.WrapperVtableRawFileOffset == 0x187F8C4
            && UiShaderSourceReadOnlyOffsetProjectionRules.WrapperVtableSlot4Offset == 0x04
            && UiShaderSourceReadOnlyOffsetProjectionRules.WrapperVtableSlot8Offset == 0x08
            && UiShaderSourceReadOnlyOffsetProjectionRules.ReadOnlyGetterRawFileOffset == 0x12CB910
            && UiShaderSourceReadOnlyOffsetProjectionRules.WritableGetterRawFileOffset == 0x12CB960
            && UiShaderSourceReadOnlyOffsetProjectionRules.BoundsCompareRawFileOffset == 0x12CB91B
            && UiShaderSourceReadOnlyOffsetProjectionRules.UnsignedBoundsBranchRawFileOffset == 0x12CB91E
            && UiShaderSourceReadOnlyOffsetProjectionRules.BoundsFailureReporterCallRawFileOffset == 0x12CB931
            && UiShaderSourceReadOnlyOffsetProjectionRules.BackingPointerLoadRawFileOffset == 0x12CB93A
            && UiShaderSourceReadOnlyOffsetProjectionRules.BackingDataPointerLoadRawFileOffset == 0x12CB942
            && UiShaderSourceReadOnlyOffsetProjectionRules.DataPlusOffsetRawFileOffset == 0x12CB945
            && UiShaderSourceReadOnlyOffsetProjectionRules.NullBackingReturnRawFileOffset == 0x12CB94C
            && UiShaderSourceReadOnlyOffsetProjectionRules.PreserveHelperCallRawFileOffset == 0x12CB992
            && UiShaderSourceReadOnlyOffsetProjectionRules.PreserveHelperTargetRawFileOffset == 0x12CBCB0);

        check("Read-only getter bytes match the captured body and contain no slot-4 preserve-helper call",
            UiShaderSourceReadOnlyOffsetProjectionRules.ReadOnlyGetterInstructionBytesHex
                == "55 8B EC 56 8B F1 8B 06 FF 50 0C 39 45 08 76 1A "
                    + "6A 00 68 90 00 00 00 68 07 3F B7 01 68 FB 3E B7 01 "
                    + "FF 15 A0 5E C1 01 83 C4 10 8B 46 04 5E 85 C0 74 0A "
                    + "8B 40 04 03 45 08 5D C2 04 00 33 C0 5D C2 04 00"
            && !UiShaderSourceReadOnlyOffsetProjectionRules.ReadOnlyGetterInstructionBytesHex
                .Contains("E8 19 03 00 00", StringComparison.Ordinal));

        uint[] boundaryValues = { 0, 1, 0x7FFFFFFF, 0x80000000, uint.MaxValue };
        bool unsignedBoundsMatch = true;
        bool resultMatchesUncheckedPointerAddition = true;
        for (int lengthIndex = 0; lengthIndex < boundaryValues.Length; lengthIndex++)
        {
            for (int offsetIndex = 0; offsetIndex < boundaryValues.Length; offsetIndex++)
            {
                uint length = boundaryValues[lengthIndex];
                uint offset = boundaryValues[offsetIndex];
                UiShaderSourceReadOnlyOffsetResult result =
                    UiShaderSourceReadOnlyOffsetProjectionRules.ProjectOffsetResult(
                        length,
                        1,
                        0xFFFFFFF0,
                        offset);

                unsignedBoundsMatch &=
                    result.OffsetWithinSizeGetterResult == (offset <= length)
                    && result.WouldReachBoundsFailureReporter == (offset > length);
                resultMatchesUncheckedPointerAddition &=
                    result.ResultPointerIfReporterReturns == unchecked(0xFFFFFFF0u + offset);
            }
        }

        check("Read-only getter uses an unsigned inclusive bound and preserves out-of-range return arithmetic",
            unsignedBoundsMatch
            && resultMatchesUncheckedPointerAddition
            && UiShaderSourceReadOnlyOffsetProjectionRules.ProjectOffsetResult(
                8, 1, 0x1000, 8) is
                {
                    OffsetWithinSizeGetterResult: true,
                    WouldReachBoundsFailureReporter: false,
                    ResultPointerIfReporterReturns: 0x1008
                }
            && UiShaderSourceReadOnlyOffsetProjectionRules.ProjectOffsetResult(
                8, 1, 0x1000, 9) is
                {
                    OffsetWithinSizeGetterResult: false,
                    WouldReachBoundsFailureReporter: true,
                    ResultPointerIfReporterReturns: 0x1009
                });

        UiShaderSourceReadOnlyOffsetResult wrappedPointer =
            UiShaderSourceReadOnlyOffsetProjectionRules.ProjectOffsetResult(
                uint.MaxValue,
                1,
                0xFFFFFFF0,
                0x20);
        check("Read-only getter models data-plus-offset as wrapping DWORD arithmetic",
            wrappedPointer is
                {
                    OffsetWithinSizeGetterResult: true,
                    WouldReachBoundsFailureReporter: false,
                    ResultPointerIfReporterReturns: 0x10
                });

        UiShaderSourceReadOnlyOffsetResult nullBackingAtZero =
            UiShaderSourceReadOnlyOffsetProjectionRules.ProjectOffsetResult(0, 0, null, 0);
        UiShaderSourceReadOnlyOffsetResult nullBackingPastZero =
            UiShaderSourceReadOnlyOffsetProjectionRules.ProjectOffsetResult(0, 0, null, 1);
        check("Null backing returns zero while retaining the zero-length unsigned bounds outcome",
            nullBackingAtZero is
                {
                    OffsetWithinSizeGetterResult: true,
                    WouldReachBoundsFailureReporter: false,
                    ResultPointerIfReporterReturns: 0
                }
            && nullBackingPastZero is
                {
                    OffsetWithinSizeGetterResult: false,
                    WouldReachBoundsFailureReporter: true,
                    ResultPointerIfReporterReturns: 0
                });

        UiShaderSourceReadOnlyOffsetResult sizeAndBackingCapturesCrossed =
            UiShaderSourceReadOnlyOffsetProjectionRules.ProjectOffsetResult(8, 0, null, 4);
        UiShaderSourceReadOnlyOffsetResult backingPresentAfterZeroSize =
            UiShaderSourceReadOnlyOffsetProjectionRules.ProjectOffsetResult(0, 1, 0x1000, 1);
        check("Size and later backing captures remain independent across a changed wrapper state",
            sizeAndBackingCapturesCrossed is
                {
                    OffsetWithinSizeGetterResult: true,
                    WouldReachBoundsFailureReporter: false,
                    ResultPointerIfReporterReturns: 0
                }
            && backingPresentAfterZeroSize is
                {
                    OffsetWithinSizeGetterResult: false,
                    WouldReachBoundsFailureReporter: true,
                    ResultPointerIfReporterReturns: 0x1001
                });

        bool nullBackingDataRejected = ThrowsForParameter(
            () => UiShaderSourceReadOnlyOffsetProjectionRules.ProjectOffsetResult(8, 0, 0, 0),
            "capturedBackingDataPointer");
        bool missingDataRejected = ThrowsForParameter(
            () => UiShaderSourceReadOnlyOffsetProjectionRules.ProjectOffsetResult(8, 1, null, 0),
            "capturedBackingDataPointer");
        check("Read-only getter projection validates only the later backing/data capture pair",
            nullBackingDataRejected && missingDataRejected);
    }

    private static bool ThrowsForParameter(Action action, string parameterName)
    {
        try
        {
            action();
        }
        catch (ArgumentException exception)
        {
            return exception.ParamName == parameterName;
        }

        return false;
    }
}

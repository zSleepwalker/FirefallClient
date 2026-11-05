namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Projects the captured return path of slBlob's bounded, non-detaching pointer getter.
/// The returned pointer is conditional on the bounds-failure reporter returning; no captured
/// pointer is dereferenced and the preserve helper used by slot 4 is not modeled as part of this getter.
/// The size-getter result and later backing-pointer load are independent captures, not an atomic snapshot.
/// </summary>
public static class UiShaderSourceReadOnlyOffsetProjectionRules
{
    public const uint WrapperVtableRawFileOffset = 0x187F8C4;
    public const uint WrapperVtableSlot4Offset = 0x04;
    public const uint WrapperVtableSlot8Offset = 0x08;
    public const uint ReadOnlyGetterRawFileOffset = 0x12CB910;
    public const uint WritableGetterRawFileOffset = 0x12CB960;
    public const uint BoundsCompareRawFileOffset = 0x12CB91B;
    public const uint UnsignedBoundsBranchRawFileOffset = 0x12CB91E;
    public const uint BoundsFailureReporterCallRawFileOffset = 0x12CB931;
    public const uint BackingPointerLoadRawFileOffset = 0x12CB93A;
    public const uint BackingDataPointerLoadRawFileOffset = 0x12CB942;
    public const uint DataPlusOffsetRawFileOffset = 0x12CB945;
    public const uint NullBackingReturnRawFileOffset = 0x12CB94C;
    public const uint PreserveHelperCallRawFileOffset = 0x12CB992;
    public const uint PreserveHelperTargetRawFileOffset = 0x12CBCB0;

    public const string ReadOnlyGetterInstructionBytesHex =
        "55 8B EC 56 8B F1 8B 06 FF 50 0C 39 45 08 76 1A " +
        "6A 00 68 90 00 00 00 68 07 3F B7 01 68 FB 3E B7 01 " +
        "FF 15 A0 5E C1 01 83 C4 10 8B 46 04 5E 85 C0 74 0A " +
        "8B 40 04 03 45 08 5D C2 04 00 33 C0 5D C2 04 00";

    public static UiShaderSourceReadOnlyOffsetResult ProjectOffsetResult(
        uint capturedSizeGetterResult,
        uint laterCapturedBackingPointer,
        uint? capturedBackingDataPointer,
        uint offset)
    {
        bool offsetWithinLength = offset <= capturedSizeGetterResult;
        if (laterCapturedBackingPointer == 0)
        {
            if (capturedBackingDataPointer.HasValue)
                throw new ArgumentException(
                    "A null backing pointer cannot have a captured data pointer.",
                    nameof(capturedBackingDataPointer));

            return new UiShaderSourceReadOnlyOffsetResult(
                OffsetWithinSizeGetterResult: offsetWithinLength,
                WouldReachBoundsFailureReporter: !offsetWithinLength,
                ResultPointerIfReporterReturns: 0);
        }

        uint backingDataPointer = capturedBackingDataPointer
            ?? throw new ArgumentException(
                "A nonnull backing pointer requires a captured data pointer.",
                nameof(capturedBackingDataPointer));

        return new UiShaderSourceReadOnlyOffsetResult(
            OffsetWithinSizeGetterResult: offsetWithinLength,
            WouldReachBoundsFailureReporter: !offsetWithinLength,
            ResultPointerIfReporterReturns: unchecked(backingDataPointer + offset));
    }
}

public readonly record struct UiShaderSourceReadOnlyOffsetResult(
    bool OffsetWithinSizeGetterResult,
    bool WouldReachBoundsFailureReporter,
    uint ResultPointerIfReporterReturns);

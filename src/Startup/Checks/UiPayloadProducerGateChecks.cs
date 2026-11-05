using Firefall.Reconstruction.Startup;

internal static class UiPayloadProducerGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI payload producer raw entry, derived output, and ordered prework gate addresses are recorded",
            UiPayloadProducerGateProjectionRules.ProducerMethodFileOffset == 0xC95400
            && UiPayloadProducerGateProjectionRules.ProducerMethodVirtualAddress == 0x1096000
            && UiPayloadProducerGateProjectionRules.DerivedStateMethodFileOffset == 0xCCA190
            && UiPayloadProducerGateProjectionRules.FirstVirtualCallSiteFileOffset == 0xC9544D
            && UiPayloadProducerGateProjectionRules.FirstVirtualCallVtableOffset == 0x34
            && UiPayloadProducerGateProjectionRules.ZeroByteReturnFileOffset == 0xC95456
            && UiPayloadProducerGateProjectionRules.ThresholdCompareBeginFileOffset == 0xC9545E
            && UiPayloadProducerGateProjectionRules.ThresholdCompareEndFileOffset == 0xC9546D
            && UiPayloadProducerGateProjectionRules.DerivedOutputFirstFloatOffset == 0x2DC
            && UiPayloadProducerGateProjectionRules.DerivedOutputFloatCount == 9
            && UiPayloadProducerGateProjectionRules.CapturedByte300Offset == 0x300
            && UiPayloadProducerGateProjectionRules.ComparedField2ECOffset == 0x2EC
            && UiPayloadProducerGateProjectionRules.SkippedOnPreworkRejectVirtualCall20Offset == 0x20
            && UiPayloadProducerGateProjectionRules.SkippedOnPreworkRejectVirtualCall28Offset == 0x28
            && UiPayloadProducerGateProjectionRules.SkippedOnPreworkRejectConditionalVirtualCall74Offset == 0x74);

        bool allByte300Values = Enumerable.Range(0, 256).All(value =>
        {
            var projection = UiPayloadProducerGateProjectionRules.Project(
                new((byte)value, 1f, 1f, 1, true));
            return value == 0
                ? projection == new UiPayloadProducerGateProjection(
                    UiPayloadProducerPreworkDecision.RejectedByByte300,
                    UiPayloadProducerAppendDecision.NotReached)
                : projection == new UiPayloadProducerGateProjection(
                    UiPayloadProducerPreworkDecision.PassedPreworkGates,
                    UiPayloadProducerAppendDecision.AppendGatesPassed);
        });
        check("UI payload producer byte+300 gate distinguishes zero from every nonzero byte", allByte300Values);

        bool allByte72CValues = Enumerable.Range(0, 256).All(value =>
        {
            var present = UiPayloadProducerGateProjectionRules.Project(
                new(1, 1f, 1f, (byte)value, true));
            var absent = UiPayloadProducerGateProjectionRules.Project(
                new(1, 1f, 1f, (byte)value, false));
            return present.Prework == UiPayloadProducerPreworkDecision.PassedPreworkGates
                && absent.Prework == UiPayloadProducerPreworkDecision.PassedPreworkGates
                && present.Append == (value == 0
                    ? UiPayloadProducerAppendDecision.RejectedByByte72C
                    : UiPayloadProducerAppendDecision.AppendGatesPassed)
                && absent.Append == (value == 0
                    ? UiPayloadProducerAppendDecision.RejectedByByte72C
                    : UiPayloadProducerAppendDecision.RejectedByNullDescriptor);
        });
        check("UI payload append gates require nonzero byte+72C before a nonnull +A08 descriptor",
            allByte72CValues
            && UiManagerNativeLayout.UiPayloadDescriptorConsumerObjectFlagOffset == 0x72C
            && UiManagerNativeLayout.UiManagerPayloadDescriptorOffset == 0xA08
            && UiManagerNativeLayout.UiManagerGlobalPointerVirtualAddress == 0x1E002B8);

        bool finiteComparisons = new (float Threshold, float Field, UiPayloadProducerPreworkDecision Expected)[]
        {
            (2f, 1f, UiPayloadProducerPreworkDecision.RejectedByThreshold),
            (1f, 1f, UiPayloadProducerPreworkDecision.PassedPreworkGates),
            (0f, 1f, UiPayloadProducerPreworkDecision.PassedPreworkGates)
        }.All(test => UiPayloadProducerGateProjectionRules.Project(
                new(1, test.Field, test.Threshold, 1, true)).Prework == test.Expected);
        check("UI payload threshold gate rejects only an ordered global-threshold-greater comparison",
            finiteComparisons);

        bool unorderedComparisonsFallThrough = new (float Threshold, float Field)[]
        {
            (float.NaN, 1f),
            (1f, float.NaN),
            (float.NaN, float.NaN)
        }.All(test => UiPayloadProducerGateProjectionRules.Project(
                new(1, test.Field, test.Threshold, 1, true)).Prework
                    == UiPayloadProducerPreworkDecision.PassedPreworkGates);
        check("UI payload unordered NaN comparison does not take JA when the comparison completes",
            unorderedComparisonsFallThrough);

        bool infinities = new (float Threshold, float Field, UiPayloadProducerPreworkDecision Expected)[]
        {
            (float.PositiveInfinity, 0f, UiPayloadProducerPreworkDecision.RejectedByThreshold),
            (0f, float.PositiveInfinity, UiPayloadProducerPreworkDecision.PassedPreworkGates),
            (float.PositiveInfinity, float.PositiveInfinity, UiPayloadProducerPreworkDecision.PassedPreworkGates),
            (float.NegativeInfinity, float.PositiveInfinity, UiPayloadProducerPreworkDecision.PassedPreworkGates),
            (float.PositiveInfinity, float.NegativeInfinity, UiPayloadProducerPreworkDecision.RejectedByThreshold)
        }.All(test => UiPayloadProducerGateProjectionRules.Project(
                new(1, test.Field, test.Threshold, 1, true)).Prework == test.Expected);
        check("UI payload threshold comparison preserves ordered positive and negative infinity cases", infinities);

        bool rejectedPathsDoNotReachAppend = new (byte Byte300, float Threshold, float Field)[]
        {
            (0, 2f, 1f),
            (1, 2f, 1f)
        }.All(test =>
        {
            var projection = UiPayloadProducerGateProjectionRules.Project(
                new(test.Byte300, test.Field, test.Threshold, 0, false));
            return projection.Append == UiPayloadProducerAppendDecision.NotReached
                && projection.Prework == (test.Byte300 == 0
                    ? UiPayloadProducerPreworkDecision.RejectedByByte300
                    : UiPayloadProducerPreworkDecision.RejectedByThreshold);
        });
        check("UI payload prework rejections skip later virtual calls and do not infer append success from zero",
            rejectedPathsDoNotReachAppend);

        check("UI cursor evidence records one LoadCursorA registration and concrete tree-node layout",
            UiManagerNativeLayout.UiManagerCursorRegistrationMethodFileOffset == 0xBFD6D0
            && UiManagerNativeLayout.UiManagerCursorLoadCursorAIatVirtualAddress == 0x1AC4C2C
            && UiManagerNativeLayout.UiManagerCursorResourceIdentifier == 0x7F8A
            && UiManagerNativeLayout.UiManagerCursorTreeOffset == 0x6C8
            && UiManagerNativeLayout.UiManagerCursorTreeInsertMissingMethodFileOffset == 0xC06D40
            && UiManagerNativeLayout.UiManagerCursorTreeNodePayloadStoreFileOffset == 0xC1675D
            && UiManagerNativeLayout.UiManagerCursorTreeNodeKeyOffset == 0x10
            && UiManagerNativeLayout.UiManagerCursorTreeNodePayloadOffset == 0x14);
    }
}

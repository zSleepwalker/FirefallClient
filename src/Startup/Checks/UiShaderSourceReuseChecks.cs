using Firefall.Reconstruction.Startup;

internal static class UiShaderSourceReuseChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("slBlob reuse records the captured control, capacity, alignment, data, and direct-store anchors",
            UiShaderSourceReuseProjectionRules.BackingCapacityFieldOffset == 0x00
            && UiShaderSourceReuseProjectionRules.BackingDataPointerFieldOffset == 0x04
            && UiShaderSourceReuseProjectionRules.BackingLogicalLengthFieldOffset == 0x0C
            && UiShaderSourceReuseProjectionRules.ControlCountFieldOffset == 0x04
            && UiShaderSourceReuseProjectionRules.ObjectAlignmentFieldOffset == 0x0C
            && UiShaderSourceReuseProjectionRules.ControlTestRawFileOffset == 0x12CBCF5
            && UiShaderSourceReuseProjectionRules.ControlNullBranchRawFileOffset == 0x12CBCF7
            && UiShaderSourceReuseProjectionRules.ControlCountCompareRawFileOffset == 0x12CBCF9
            && UiShaderSourceReuseProjectionRules.ControlCountBranchRawFileOffset == 0x12CBCFD
            && UiShaderSourceReuseProjectionRules.FirstCapacityCompareRawFileOffset == 0x12CBCFF
            && UiShaderSourceReuseProjectionRules.FirstCapacityBranchRawFileOffset == 0x12CBD01
            && UiShaderSourceReuseProjectionRules.AlignmentLoadRawFileOffset == 0x12CBD03
            && UiShaderSourceReuseProjectionRules.AlignmentZeroBranchRawFileOffset == 0x12CBD08
            && UiShaderSourceReuseProjectionRules.DataPointerLoadRawFileOffset == 0x12CBD0A
            && UiShaderSourceReuseProjectionRules.AlignmentDivideRawFileOffset == 0x12CBD0F
            && UiShaderSourceReuseProjectionRules.DataPointerRemainderBranchRawFileOffset == 0x12CBD13
            && UiShaderSourceReuseProjectionRules.SecondCapacityCompareRawFileOffset == 0x12CBD15
            && UiShaderSourceReuseProjectionRules.DirectLengthStoreBranchRawFileOffset == 0x12CBD17
            && UiShaderSourceReuseProjectionRules.DirectLengthStoreRawFileOffset == 0x12CBD33);

        check("Reuse capacity gates read backing field zero, not logical length field C",
            UiShaderSourceReuseProjectionRules.BackingCapacityFieldOffset
                != UiShaderSourceReuseProjectionRules.BackingLogicalLengthFieldOffset);

        UiShaderSourceReuseCapture direct = new(
            RequestedLength: 0x40,
            ControlPointer: 0x1000,
            ControlCount: 1,
            FirstCapacity: 0x40,
            Alignment: 0,
            DataPointer: null,
            SecondCapacity: 0x40);
        check("A zero alignment skips the data-pointer modulo and both unsigned capacity gates admit the direct edge",
            UiShaderSourceReuseProjectionRules.SelectsDirectLengthStoreEdge(direct));

        UiShaderSourceReuseCapture alignedData = direct with
        {
            Alignment = 8,
            DataPointer = 0x1008
        };
        UiShaderSourceReuseCapture misalignedData = alignedData with { DataPointer = 0x1009 };
        check("A nonzero alignment requires an aligned captured data pointer",
            UiShaderSourceReuseProjectionRules.SelectsDirectLengthStoreEdge(alignedData)
            && !UiShaderSourceReuseProjectionRules.SelectsDirectLengthStoreEdge(misalignedData with
            {
                SecondCapacity = null
            }));

        check("Control pointer and count gates reject null, zero-count, and multi-count captures",
            !UiShaderSourceReuseProjectionRules.SelectsDirectLengthStoreEdge(direct with
            {
                ControlPointer = 0,
                ControlCount = null,
                FirstCapacity = null
            })
            && !UiShaderSourceReuseProjectionRules.SelectsDirectLengthStoreEdge(direct with
            {
                ControlCount = 0,
                FirstCapacity = null
            })
            && !UiShaderSourceReuseProjectionRules.SelectsDirectLengthStoreEdge(direct with
            {
                ControlCount = 2,
                FirstCapacity = null
            }));

        UiShaderSourceReuseCapture distinctCapacities = direct with
        {
            RequestedLength = 0x60,
            FirstCapacity = 0x80,
            SecondCapacity = 0x40
        };
        UiShaderSourceReuseCapture firstCapacityRejects = distinctCapacities with
        {
            FirstCapacity = 0x40,
            SecondCapacity = 0x80
        };
        check("The two separately captured capacity reads gate independently",
            !UiShaderSourceReuseProjectionRules.SelectsDirectLengthStoreEdge(distinctCapacities)
            && !UiShaderSourceReuseProjectionRules.SelectsDirectLengthStoreEdge(firstCapacityRejects with
            {
                SecondCapacity = null
            }));

        UiShaderSourceReuseCapture unsignedBoundaries = direct with
        {
            RequestedLength = uint.MaxValue,
            FirstCapacity = uint.MaxValue,
            SecondCapacity = uint.MaxValue
        };
        check("Capacity comparisons preserve unsigned DWORD boundaries including uint.MaxValue",
            UiShaderSourceReuseProjectionRules.SelectsDirectLengthStoreEdge(unsignedBoundaries)
            && !UiShaderSourceReuseProjectionRules.SelectsDirectLengthStoreEdge(
                unsignedBoundaries with { FirstCapacity = 0x7FFFFFFF }));

        UiShaderSourceReuseCapture assertionEdge = direct with
        {
            RequestedLength = 0x41,
            FirstCapacity = 0x80,
            SecondCapacity = 0x40
        };
        check("Exceeding the second capacity does not claim the direct non-assertion edge",
            !UiShaderSourceReuseProjectionRules.SelectsDirectLengthStoreEdge(assertionEdge));

        bool missingControlPointerRejected = RejectsMissing(
            direct with { ControlPointer = null },
            nameof(UiShaderSourceReuseCapture.ControlPointer));
        bool missingCountRejectedAfterNonnullControl = RejectsMissing(
            direct with { ControlCount = null },
            nameof(UiShaderSourceReuseCapture.ControlCount));
        bool missingCountSkippedForNullControl = !RejectsMissing(
            direct with { ControlPointer = 0, ControlCount = null },
            nameof(UiShaderSourceReuseCapture.ControlCount));
        bool missingFirstCapacityRejectedAfterCountOne = RejectsMissing(
            direct with { FirstCapacity = null },
            nameof(UiShaderSourceReuseCapture.FirstCapacity));
        bool missingFirstCapacitySkippedForOtherCount = !RejectsMissing(
            direct with { ControlCount = 0, FirstCapacity = null },
            nameof(UiShaderSourceReuseCapture.FirstCapacity));
        bool missingAlignmentSkippedAfterFirstGateRejects = !RejectsMissing(
            direct with { RequestedLength = 0x41, FirstCapacity = 0x40, Alignment = null },
            nameof(UiShaderSourceReuseCapture.Alignment));
        bool missingAlignmentRejectedAfterFirstGatePasses = RejectsMissing(
            direct with { Alignment = null },
            nameof(UiShaderSourceReuseCapture.Alignment));
        bool missingDataSkippedForZeroAlignment = !RejectsMissing(
            direct with { Alignment = 0, DataPointer = null },
            nameof(UiShaderSourceReuseCapture.DataPointer));
        bool missingDataRejectedForNonzeroAlignment = RejectsMissing(
            direct with { Alignment = 8, DataPointer = null },
            nameof(UiShaderSourceReuseCapture.DataPointer));
        bool missingSecondCapacityRejectedAfterAlignmentGate = RejectsMissing(
            alignedData with { SecondCapacity = null },
            nameof(UiShaderSourceReuseCapture.SecondCapacity));
        check("Missing captured reads are rejected only when their projected branch requires them",
            missingControlPointerRejected
            && missingCountRejectedAfterNonnullControl
            && missingCountSkippedForNullControl
            && missingFirstCapacityRejectedAfterCountOne
            && missingFirstCapacitySkippedForOtherCount
            && missingAlignmentSkippedAfterFirstGateRejects
            && missingAlignmentRejectedAfterFirstGatePasses
            && missingDataSkippedForZeroAlignment
            && missingDataRejectedForNonzeroAlignment
            && missingSecondCapacityRejectedAfterAlignmentGate);
    }

    private static bool RejectsMissing(UiShaderSourceReuseCapture capture, string parameterName)
    {
        try
        {
            UiShaderSourceReuseProjectionRules.SelectsDirectLengthStoreEdge(capture);
            return false;
        }
        catch (ArgumentException exception)
        {
            return exception.ParamName == parameterName;
        }
    }
}

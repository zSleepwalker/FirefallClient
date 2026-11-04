using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePreserveCopyChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Replacement copy anchors and pinned executable bytes are recorded",
            UiShaderSourcePreserveCopyProjectionRules.ImmutableSampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePreserveCopyProjectionRules.CodeVirtualAddressOffsetFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePreserveCopyProjectionRules.ZeroRequestBranchRawFileOffset == 0x12CBD4B
            && UiShaderSourcePreserveCopyProjectionRules.ZeroRequestBranchBytesHex
                == "85F6750E8B5F08897708897704E9C7000000"
            && UiShaderSourcePreserveCopyProjectionRules.SizeAccessorCallRawFileOffset == 0x12CBD61
            && UiShaderSourcePreserveCopyProjectionRules.SizeAccessorCallBytesHex == "FF500C"
            && UiShaderSourcePreserveCopyProjectionRules.SizeAccessorVtableSlotOffset == 0x0C
            && UiShaderSourcePreserveCopyProjectionRules.SizeMinimumRawFileOffset == 0x12CBD68
            && UiShaderSourcePreserveCopyProjectionRules.SizeMinimumBytesHex == "3BC60F47C6"
            && UiShaderSourcePreserveCopyProjectionRules.PreserveByteCompareRawFileOffset == 0x12CBDAE
            && UiShaderSourcePreserveCopyProjectionRules.PreserveByteCompareBytesHex == "807D0C00"
            && UiShaderSourcePreserveCopyProjectionRules.PreserveByteFrameArgumentOffset == 0x0C
            && UiShaderSourcePreserveCopyProjectionRules.ZeroCopyCountTestRawFileOffset == 0x12CBDC0
            && UiShaderSourcePreserveCopyProjectionRules.ZeroCopyCountTestBytesHex == "85D2"
            && UiShaderSourcePreserveCopyProjectionRules.OldDataPointerLoadRawFileOffset == 0x12CBDC8
            && UiShaderSourcePreserveCopyProjectionRules.OldDataPointerLoadBytesHex == "8B4804"
            && UiShaderSourcePreserveCopyProjectionRules.NewDataPointerLoadRawFileOffset == 0x12CBDD0
            && UiShaderSourcePreserveCopyProjectionRules.NewDataPointerLoadBytesHex == "FF7004"
            && UiShaderSourcePreserveCopyProjectionRules.CopyCallRawFileOffset == 0x12CBDD3
            && UiShaderSourcePreserveCopyProjectionRules.CopyCallBytesHex == "E81C1B2D00"
            && UiShaderSourcePreserveCopyProjectionRules.ToCodeVirtualAddress(0x12CBD4B) == 0x16CC94B
            && UiShaderSourcePreserveCopyProjectionRules.ToCodeVirtualAddress(0x12CBDD3) == 0x16CC9D3);

        UiShaderSourcePreserveCopyPlan zeroRequest =
            UiShaderSourcePreserveCopyProjectionRules.Project(
                new UiShaderSourcePreserveCopyCapture(0, null, null));
        check("A zero replacement request bypasses the accessor and preserve-copy gates",
            zeroRequest is
                {
                    SizeAccessorCallReached: false,
                    PreserveByteTestReached: false,
                    CopyByteCount: 0,
                    CopyCallReached: false
                });

        const uint positiveRequest = 0x40;
        bool allPreserveBytesMatchGate = true;
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            byte preserveByte = (byte)value;
            UiShaderSourcePreserveCopyPlan plan =
                UiShaderSourcePreserveCopyProjectionRules.Project(
                    new UiShaderSourcePreserveCopyCapture(
                        positiveRequest,
                        SizeAccessorResult: 0x20,
                        PreserveByte: preserveByte));
            allPreserveBytesMatchGate &=
                plan.SizeAccessorCallReached
                && plan.PreserveByteTestReached
                && plan.CopyByteCount == 0x20
                && plan.CopyCallReached == (preserveByte != 0);
        }
        check("Every preserve byte value gates copy-call reachability by zero versus nonzero",
            allPreserveBytesMatchGate);

        uint[] boundaryValues = { 0, 1, 0x7FFFFFFF, 0x80000000, uint.MaxValue };
        bool unsignedMinimumMatches = true;
        bool copyGateMatches = true;
        foreach (uint requestedLength in boundaryValues)
        foreach (uint accessorResult in boundaryValues)
        foreach (byte preserveByte in new byte[] { 0, 1, byte.MaxValue })
        {
            UiShaderSourcePreserveCopyPlan plan =
                UiShaderSourcePreserveCopyProjectionRules.Project(
                    new UiShaderSourcePreserveCopyCapture(
                        requestedLength,
                        accessorResult,
                        preserveByte));
            uint expectedCount = accessorResult < requestedLength
                ? accessorResult
                : requestedLength;
            unsignedMinimumMatches &= plan.CopyByteCount == expectedCount;
            copyGateMatches &=
                plan.CopyCallReached == (requestedLength != 0
                    && expectedCount != 0
                    && preserveByte != 0);
        }
        check("Copy count uses the unsigned minimum across DWORD boundaries and only positive gated counts copy",
            unsignedMinimumMatches && copyGateMatches);

        UiShaderSourcePreserveCopyPlan zeroAccessorResult =
            UiShaderSourcePreserveCopyProjectionRules.Project(
                new UiShaderSourcePreserveCopyCapture(1, 0, byte.MaxValue));
        UiShaderSourcePreserveCopyPlan preserveDisabled =
            UiShaderSourcePreserveCopyProjectionRules.Project(
                new UiShaderSourcePreserveCopyCapture(1, 1, 0));
        check("A zero selected count or zero preserve byte skips copy without suppressing the accessor",
            zeroAccessorResult is
                {
                    SizeAccessorCallReached: true,
                    PreserveByteTestReached: true,
                    CopyByteCount: 0,
                    CopyCallReached: false
                }
            && preserveDisabled is
                {
                    SizeAccessorCallReached: true,
                    PreserveByteTestReached: true,
                    CopyByteCount: 1,
                    CopyCallReached: false
                });

        bool missingAccessorRejectedEvenWhenPreserveIsZero = RejectsMissing(
            new UiShaderSourcePreserveCopyCapture(1, null, 0),
            nameof(UiShaderSourcePreserveCopyCapture.SizeAccessorResult));
        bool missingPreserveByteRejectedAfterAccessor = RejectsMissing(
            new UiShaderSourcePreserveCopyCapture(1, 1, null),
            nameof(UiShaderSourcePreserveCopyCapture.PreserveByte));
        check("Positive replacement requests require the accessor before evaluating even a disabled preserve byte",
            missingAccessorRejectedEvenWhenPreserveIsZero
            && missingPreserveByteRejectedAfterAccessor);
    }

    private static bool RejectsMissing(
        UiShaderSourcePreserveCopyCapture capture,
        string parameterName)
    {
        try
        {
            UiShaderSourcePreserveCopyProjectionRules.Project(capture);
            return false;
        }
        catch (ArgumentException exception)
        {
            return exception.ParamName == parameterName;
        }
    }
}

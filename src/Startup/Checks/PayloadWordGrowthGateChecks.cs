using Firefall.Reconstruction.Startup;

internal static class PayloadWordGrowthGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE sample hash, independent section mapping, gate bytes, branches, and call target are pinned",
            PayloadWordGrowthGateProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadWordGrowthGateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadWordGrowthGateProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PayloadWordGrowthGateProjection.GateStartRawFileOffset == 0x00BE6306
            && PayloadWordGrowthGateProjection.GateStartVirtualAddress == 0x00FE6F06
            && PayloadWordGrowthGateProjection.GateStartVirtualAddressFromPeSection
                == 0x00FE6F06
            && PayloadWordGrowthGateProjection.GateStartVirtualAddressFromRawOffset
                == 0x00FE6F06
            && PayloadWordGrowthGateProjection.RawFileOffsetToVirtualAddress(0x00BE6306)
                == 0x00FE6F06
            && PayloadWordGrowthGateProjection.LowBitsBranchRawFileOffset == 0x00BE6310
            && PayloadWordGrowthGateProjection.LowBitsBranchTargetRawFileOffset
                == 0x00BE6325
            && PayloadWordGrowthGateProjection.ReceiverBoundBranchRawFileOffset
                == 0x00BE631B
            && PayloadWordGrowthGateProjection.ReceiverBoundBranchTargetRawFileOffset
                == 0x00BE6325
            && PayloadWordGrowthGateProjection.HandoffCallRawFileOffset == 0x00BE6320
            && PayloadWordGrowthGateProjection.HandoffCallTargetRawFileOffset == 0x00BE68A0
            && PayloadWordGrowthGateProjection.HandoffCallTargetVirtualAddress
                == 0x00FE74A0
            && PayloadWordGrowthGateProjection.HandoffCallReturnBoundaryRawFileOffset
                == 0x00BE6325
            && PayloadWordGrowthGateProjection.GateEndRawFileOffsetExclusive
                == 0x00BE6325
            && string.Concat(PayloadWordGrowthGateProjection.CapturedGateBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B460C8B4E1003C1A80775138D4108C1E8033946087708518BCEE87B050000"
            && PayloadWordGrowthGateProjection.Instructions.SequenceEqual(
                ExpectedInstructions()));

        check("The receiver reads are independent DWORD captures and their sum wraps before the AL low-bit test",
            PayloadWordGrowthGateProjection.ReceiverDwordAddress(uint.MaxValue, 0x0C)
                == 0x0B
            && PayloadWordGrowthGateProjection.ReceiverDwordAddress(uint.MaxValue, 0x10)
                == 0x0F
            && VerifyAllLowBitClasses()
            && VerifyDwordWraparound());

        PayloadWordGrowthGatePlan missingFirst = PayloadWordGrowthGateProjection.Project(
            new(0x1000, null, null, null, null));
        PayloadWordGrowthGatePlan laterFirst = PayloadWordGrowthGateProjection.Project(
            new(0x1000, null, 0x1000, null, null));
        PayloadWordGrowthGatePlan missingSecond = PayloadWordGrowthGateProjection.Project(
            new(0x1000, 0x1000, null, null, null));
        PayloadWordGrowthGatePlan laterSecond = PayloadWordGrowthGateProjection.Project(
            new(0x1000, 0x1000, null, 0x1000, null));
        PayloadWordGrowthGatePlan unreachedBound = PayloadWordGrowthGateProjection.Project(
            new(0x1000, 0x1000, 1, 0, null));
        PayloadWordGrowthGatePlan missingBound = PayloadWordGrowthGateProjection.Project(
            new(0x1000, 0x1000, 0, null, null));
        PayloadWordGrowthGatePlan unreachedReturn = PayloadWordGrowthGateProjection.Project(
            new(0x1000, 0x1000, 0, null, true));

        check("Missing and out-of-order nullable captures stop at the first unobserved sequential read",
            missingFirst.Outcome
                == PayloadWordGrowthGateOutcome.MissingReceiverDword0CCapture
            && missingFirst.StopAtRawFileOffsetExclusive
                == PayloadWordGrowthGateProjection.ReceiverDword0CReadRawFileOffset
            && laterFirst.Outcome == PayloadWordGrowthGateOutcome.UnreachedCaptureProvided
            && !laterFirst.ReceiverDword0CRead
            && !laterFirst.ReceiverDword10Read
            && missingSecond.Outcome
                == PayloadWordGrowthGateOutcome.MissingReceiverDword10Capture
            && missingSecond.ReceiverDword0CRead
            && !missingSecond.ReceiverDword10Read
            && missingSecond.ReceiverDword10Address == 0x1010
            && laterSecond.Outcome == PayloadWordGrowthGateOutcome.UnreachedCaptureProvided
            && !laterSecond.ReceiverDword8Read
            && unreachedBound.Outcome == PayloadWordGrowthGateOutcome.UnreachedCaptureProvided
            && !unreachedBound.ReceiverDword8Read
            && missingBound.Outcome
                == PayloadWordGrowthGateOutcome.MissingReceiverDword8Capture
            && missingBound.ReceiverDword8Address == 0x1008
            && !missingBound.ReceiverDword8Read
            && unreachedReturn.Outcome == PayloadWordGrowthGateOutcome.UnreachedCaptureProvided
            && !unreachedReturn.HandoffCallReached);

        check("Unsigned receiver DWORD boundaries select JA only when receiver+8 exceeds the aligned DWORD",
            VerifyReceiverBound(8, 2, branchTaken: false)
            && VerifyReceiverBound(8, 3, branchTaken: true)
            && VerifyReceiverBound(8, 0, branchTaken: false)
            && VerifyReceiverBound(8, 0x80000000, branchTaken: true)
            && VerifyReceiverBound(0xFFFFFFF8, 0, branchTaken: false)
            && VerifyReceiverBound(0xFFFFFFF8, 1, branchTaken: true)
            && VerifyReceiverBound(0xFFFFFFF8, uint.MaxValue, branchTaken: true));

        PayloadWordGrowthGatePlan missingReturn = PayloadWordGrowthGateProjection.Project(
            new(0x2000, 8, 8, 2, null));
        PayloadWordGrowthGatePlan abnormalReturn = PayloadWordGrowthGateProjection.Project(
            new(0x2000, 8, 8, 2, false));
        PayloadWordGrowthGatePlan normalReturn = PayloadWordGrowthGateProjection.Project(
            new(0x2000, 8, 8, 2, true));
        check("Only a captured normal opaque-call return reaches the call-path boundary",
            missingReturn.Outcome
                == PayloadWordGrowthGateOutcome.MissingHandoffCallReturnCapture
            && missingReturn.HandoffCallReached
            && missingReturn.RouteRawFileOffset is null
            && missingReturn.StopAtRawFileOffsetExclusive
                == PayloadWordGrowthGateProjection.HandoffCallReturnBoundaryRawFileOffset
            && abnormalReturn.Outcome
                == PayloadWordGrowthGateOutcome.HandoffCallDidNotReturnNormally
            && abnormalReturn.RouteRawFileOffset is null
            && normalReturn.Outcome
                == PayloadWordGrowthGateOutcome.HandoffCallReturnedNormally
            && normalReturn.HandoffCallPushedArgument == 8
            && normalReturn.HandoffCallReceiverPointer == 0x2000
            && normalReturn.RouteRawFileOffset == 0x00BE6325
            && normalReturn.StopAtRawFileOffsetExclusive == 0x00BE6325
            && normalReturn.AlignedReceiverDword10PlusEight == 2
            && normalReturn.ReceiverBoundBranchTaken == false);
    }

    private static bool VerifyAllLowBitClasses() =>
        Enumerable.Range(0, 8).All(lowBits =>
        {
            PayloadWordGrowthGatePlan plan =
                PayloadWordGrowthGateProjection.Project(
                    new(0x1000, 0x1000, (uint)lowBits, lowBits == 0 ? 1u : null,
                        lowBits == 0 ? true : null));
            return plan.Sum32 == 0x1000u + (uint)lowBits
                && plan.SumLowThreeBits == lowBits
                && (lowBits == 0
                    ? plan.Outcome == PayloadWordGrowthGateOutcome.HandoffCallReturnedNormally
                        && plan.ReceiverDword8Read
                        && plan.AlignedReceiverDword10PlusEight == 1
                    : plan.Outcome == PayloadWordGrowthGateOutcome.LowBitsBranchToBoundary
                        && !plan.ReceiverDword8Read
                        && plan.RouteRawFileOffset == 0x00BE6325);
        });

    private static bool VerifyDwordWraparound()
    {
        PayloadWordGrowthGatePlan plan = PayloadWordGrowthGateProjection.Project(
            new(0x1000, 8, 0xFFFFFFF8, 0, true));
        return plan.Sum32 == 0
            && plan.SumLowThreeBits == 0
            && plan.AlignedReceiverDword10PlusEight == 0
            && plan.ReceiverBoundBranchTaken == false
            && plan.Outcome == PayloadWordGrowthGateOutcome.HandoffCallReturnedNormally;
    }

    private static bool VerifyReceiverBound(
        uint receiverDword10,
        uint receiverDword8,
        bool branchTaken)
    {
        uint aligned = unchecked(receiverDword10 + 8) >> 3;
        PayloadWordGrowthGatePlan plan = PayloadWordGrowthGateProjection.Project(
            new(0x3000, 0x1000, receiverDword10, receiverDword8, branchTaken ? null : true));
        return plan.AlignedReceiverDword10PlusEight == aligned
            && plan.ReceiverBoundBranchTaken == branchTaken
            && plan.ReceiverDword8Read
            && (branchTaken
                ? plan.Outcome == PayloadWordGrowthGateOutcome.ReceiverBoundBranchesToBoundary
                    && !plan.HandoffCallReached
                    && plan.RouteRawFileOffset == 0x00BE6325
                : plan.Outcome == PayloadWordGrowthGateOutcome.HandoffCallReturnedNormally
                    && plan.HandoffCallReached
                    && plan.RouteRawFileOffset == 0x00BE6325);
    }

    private static PayloadWordGrowthGateInstructionEvidence[] ExpectedInstructions() =>
    [
        new("Capture the receiver DWORD at +0x0C", 0x00BE6306, 0x00FE6F06, "8B 46 0C"),
        new("Capture the receiver DWORD at +0x10 into ECX", 0x00BE6309, 0x00FE6F09, "8B 4E 10"),
        new("Add the captured DWORD values with wrapping 32-bit arithmetic", 0x00BE630C, 0x00FE6F0C, "03 C1"),
        new("Test the low three bits of AL", 0x00BE630E, 0x00FE6F0E, "A8 07"),
        new("A nonzero low-bit test branches to raw 0xBE6325", 0x00BE6310, 0x00FE6F10, "75 13", 0x00BE6325, 0x00FE6F25),
        new("Form wrapping receiver+0x10+8 in EAX", 0x00BE6312, 0x00FE6F12, "8D 41 08"),
        new("Shift the 32-bit sum right by three", 0x00BE6315, 0x00FE6F15, "C1 E8 03"),
        new("Compare the captured receiver DWORD at +8 with EAX", 0x00BE6318, 0x00FE6F18, "39 46 08"),
        new("Unsigned JA branches to raw 0xBE6325", 0x00BE631B, 0x00FE6F1B, "77 08", 0x00BE6325, 0x00FE6F25),
        new("Push the captured receiver DWORD from +0x10", 0x00BE631D, 0x00FE6F1D, "51"),
        new("Load the receiver pointer into ECX", 0x00BE631E, 0x00FE6F1E, "8B CE"),
        new("Call the opaque target at raw 0xBE68A0; normal return resumes at 0xBE6325", 0x00BE6320, 0x00FE6F20, "E8 7B 05 00 00", CallTargetRawFileOffset: 0x00BE68A0, CallTargetVirtualAddress: 0x00FE74A0)
    ];
}

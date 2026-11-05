using Firefall.Reconstruction.Startup;

internal static class PayloadWordSlotAllocationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE identity, raw/VA mapping, bounded interval, exact bytes, and call destinations are pinned",
            PayloadWordSlotAllocationProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadWordSlotAllocationProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadWordSlotAllocationProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PayloadWordSlotAllocationProjection.GateStartRawFileOffset == 0x00BE6352
            && PayloadWordSlotAllocationProjection.GateStartVirtualAddress == 0x00FE6F52
            && PayloadWordSlotAllocationProjection.GateStartVirtualAddressFromPeSection
                == 0x00FE6F52
            && PayloadWordSlotAllocationProjection.GateStartVirtualAddressFromRawOffset
                == 0x00FE6F52
            && PayloadWordSlotAllocationProjection.GateEndRawFileOffsetExclusive == 0x00BE636E
            && PayloadWordSlotAllocationProjection.GateEndVirtualAddressExclusive == 0x00FE6F6E
            && PayloadWordSlotAllocationProjection.AllocatorTargetRawFileOffset == 0x012B7FB0
            && PayloadWordSlotAllocationProjection.AllocatorTargetVirtualAddress == 0x016B8BB0
            && PayloadWordSlotAllocationProjection.ZeroPathIndirectCallCellVirtualAddress
                == 0x01AC4424
            && string.Concat(PayloadWordSlotAllocationProjection.CapturedGateBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "6A10E8571C6D008BC883C40485C97506FF152444AC018B4604890C03"
            && PayloadWordSlotAllocationProjection.Instructions.SequenceEqual(
                ExpectedInstructions()));

        PayloadWordSlotAllocationPlan allocatorNonzero = Project(
            allocatorEax: 0x1234,
            receiverDword4: 0xFFFFFFFC);
        check("A normal nonzero allocator return cleans four stack bytes, skips the indirect call, and writes retained ECX",
            allocatorNonzero.Outcome == PayloadWordSlotAllocationOutcome.SlotDwordWritten
            && allocatorNonzero.AllocatorCallReached
            && allocatorNonzero.AllocatorCallArgument == 0x10
            && allocatorNonzero.AllocatorEax == 0x1234
            && allocatorNonzero.AllocatorStackCleanupBytes == 4
            && !allocatorNonzero.ZeroPathIndirectCallReached
            && allocatorNonzero.ZeroPathIndirectCallCellVirtualAddress is null
            && allocatorNonzero.EcxForSlotWrite == 0x1234
            && allocatorNonzero.SlotDwordWritten
            && allocatorNonzero.SlotDwordWriteValue == 0x1234);

        PayloadWordSlotAllocationPlan allocatorZeroWithModifiedEcx = Project(
            allocatorEax: 0,
            indirectCallReturnedNormally: true,
            ecxAfterIndirectCall: 0xAABBCCDD,
            receiverDword4: 0x2000);
        PayloadWordSlotAllocationPlan allocatorZeroWithZeroEcx = Project(
            allocatorEax: 0,
            indirectCallReturnedNormally: true,
            ecxAfterIndirectCall: 0,
            receiverDword4: 0x2000);
        check("The zero-result indirect call uses its independently captured post-call ECX, including a zero value",
            allocatorZeroWithModifiedEcx.Outcome
                == PayloadWordSlotAllocationOutcome.SlotDwordWritten
            && allocatorZeroWithModifiedEcx.ZeroPathIndirectCallReached
            && allocatorZeroWithModifiedEcx.ZeroPathIndirectCallCellVirtualAddress
                == 0x01AC4424
            && allocatorZeroWithModifiedEcx.EcxForSlotWrite == 0xAABBCCDD
            && allocatorZeroWithModifiedEcx.SlotDwordWriteValue == 0xAABBCCDD
            && allocatorZeroWithZeroEcx.Outcome
                == PayloadWordSlotAllocationOutcome.SlotDwordWritten
            && allocatorZeroWithZeroEcx.EcxForSlotWrite == 0
            && allocatorZeroWithZeroEcx.SlotDwordWriteValue == 0);

        PayloadWordSlotAllocationPlan missingPrecondition =
            PayloadWordSlotAllocationProjection.Project(new(
                0x1000, 0x20, null, null, null, null, null, null));
        PayloadWordSlotAllocationPlan unreachedPrecondition =
            PayloadWordSlotAllocationProjection.Project(new(
                0x1000, 0x20, null, true, null, null, null, null));
        PayloadWordSlotAllocationPlan nonzeroPrecondition =
            PayloadWordSlotAllocationProjection.Project(new(
                0x1000, 0x20, 1, null, null, null, null, null));
        PayloadWordSlotAllocationPlan missingAllocatorReturn =
            PayloadWordSlotAllocationProjection.Project(new(
                0x1000, 0x20, 0, null, null, null, null, null));
        PayloadWordSlotAllocationPlan unreachedAllocatorReturn =
            PayloadWordSlotAllocationProjection.Project(new(
                0x1000, 0x20, 0, null, 0, null, null, null));
        PayloadWordSlotAllocationPlan allocatorDidNotReturn =
            PayloadWordSlotAllocationProjection.Project(new(
                0x1000, 0x20, 0, false, null, null, null, null));
        PayloadWordSlotAllocationPlan missingAllocatorEax =
            PayloadWordSlotAllocationProjection.Project(new(
                0x1000, 0x20, 0, true, null, null, null, null));
        PayloadWordSlotAllocationPlan laterAllocatorEax =
            PayloadWordSlotAllocationProjection.Project(new(
                0x1000, 0x20, 0, true, null, null, null, 0x2000));

        check("Missing, nonzero preconditions, nonreturn, and out-of-order captures stop strictly without throwing",
            missingPrecondition.Outcome
                == PayloadWordSlotAllocationOutcome.MissingPrecedingSlotDwordCapture
            && unreachedPrecondition.Outcome
                == PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
            && nonzeroPrecondition.Outcome
                == PayloadWordSlotAllocationOutcome.PrecedingSlotDwordNonzero
            && missingAllocatorReturn.Outcome
                == PayloadWordSlotAllocationOutcome.MissingAllocatorCallReturnCapture
            && unreachedAllocatorReturn.Outcome
                == PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
            && allocatorDidNotReturn.Outcome
                == PayloadWordSlotAllocationOutcome.AllocatorCallDidNotReturnNormally
            && allocatorDidNotReturn.AllocatorStackCleanupBytes is null
            && missingAllocatorEax.Outcome
                == PayloadWordSlotAllocationOutcome.MissingAllocatorEaxCapture
            && missingAllocatorEax.AllocatorStackCleanupBytes is null
            && laterAllocatorEax.Outcome
                == PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided);

        PayloadWordSlotAllocationPlan missingIndirectReturn = Project(
            allocatorEax: 0);
        PayloadWordSlotAllocationPlan laterIndirectReturn = Project(
            allocatorEax: 0,
            receiverDword4: 0x2000);
        PayloadWordSlotAllocationPlan indirectCallDidNotReturn = Project(
            allocatorEax: 0,
            indirectCallReturnedNormally: false);
        PayloadWordSlotAllocationPlan indirectCallDidNotReturnWithLaterCapture = Project(
            allocatorEax: 0,
            indirectCallReturnedNormally: false,
            ecxAfterIndirectCall: 0);
        PayloadWordSlotAllocationPlan missingPostCallEcx = Project(
            allocatorEax: 0,
            indirectCallReturnedNormally: true);
        PayloadWordSlotAllocationPlan laterPostCallEcx = Project(
            allocatorEax: 0,
            indirectCallReturnedNormally: true,
            receiverDword4: 0x2000);
        PayloadWordSlotAllocationPlan nonzeroPathUnreachedCall = Project(
            allocatorEax: 1,
            indirectCallReturnedNormally: true);

        check("The allocator-zero call has separate return and ECX captures; skips reject unreachable zero-path observations",
            missingIndirectReturn.Outcome
                == PayloadWordSlotAllocationOutcome.MissingZeroPathIndirectCallReturnCapture
            && laterIndirectReturn.Outcome
                == PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
            && indirectCallDidNotReturn.Outcome
                == PayloadWordSlotAllocationOutcome.ZeroPathIndirectCallDidNotReturnNormally
            && indirectCallDidNotReturnWithLaterCapture.Outcome
                == PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
            && missingPostCallEcx.Outcome
                == PayloadWordSlotAllocationOutcome.MissingEcxAfterIndirectCallCapture
            && laterPostCallEcx.Outcome
                == PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
            && nonzeroPathUnreachedCall.Outcome
                == PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided);

        PayloadWordSlotAllocationPlan missingReceiverDword4 = Project(
            allocatorEax: 0x1234);
        PayloadWordSlotAllocationPlan wrappedSlot = Project(
            allocatorEax: 0x1234,
            receiverDword4: uint.MaxValue,
            retainedScaledOffset: 2);
        check("The mandatory independent receiver+4 reload precedes a wrapping DWORD slot write",
            missingReceiverDword4.Outcome
                == PayloadWordSlotAllocationOutcome.MissingReceiverDword4Capture
            && missingReceiverDword4.ReceiverDword4Address == 0x1004
            && !missingReceiverDword4.ReceiverDword4Read
            && !missingReceiverDword4.SlotDwordWritten
            && wrappedSlot.Outcome == PayloadWordSlotAllocationOutcome.SlotDwordWritten
            && wrappedSlot.ReceiverDword4Address == 0x1004
            && wrappedSlot.ReceiverDword4Read
            && wrappedSlot.SlotAddress == 1
            && wrappedSlot.SlotDwordWriteValue == 0x1234
            && wrappedSlot.StopAtRawFileOffsetExclusive == 0x00BE636E);
    }

    private static PayloadWordSlotAllocationPlan Project(
        uint allocatorEax,
        bool? indirectCallReturnedNormally = null,
        uint? ecxAfterIndirectCall = null,
        uint? receiverDword4 = null,
        uint retainedScaledOffset = 0x20) =>
        PayloadWordSlotAllocationProjection.Project(new(
            ReceiverPointer: 0x1000,
            RetainedScaledOffset: retainedScaledOffset,
            PrecedingSlotDword: 0,
            AllocatorCallReturnedNormally: true,
            AllocatorEax: allocatorEax,
            ZeroPathIndirectCallReturnedNormally: indirectCallReturnedNormally,
            EcxAfterIndirectCall: ecxAfterIndirectCall,
            ReceiverDword4AtSlotWrite: receiverDword4));

    private static PayloadWordSlotAllocationInstructionEvidence[] ExpectedInstructions() =>
    [
        new("Push allocator argument 0x10", 0x00BE6352, 0x00FE6F52, "6A 10"),
        new("Call allocator target raw 0x12B7FB0; normal return resumes at raw 0xBE6359", 0x00BE6354, 0x00FE6F54, "E8 57 1C 6D 00", 0x012B7FB0, 0x016B8BB0, 0x00BE6359),
        new("Move allocator EAX into ECX", 0x00BE6359, 0x00FE6F59, "8B C8"),
        new("Clean up the pushed allocator argument after normal return", 0x00BE635B, 0x00FE6F5B, "83 C4 04"),
        new("Test the allocator result retained in ECX", 0x00BE635E, 0x00FE6F5E, "85 C9"),
        new("A nonzero allocator result skips the indirect call to raw 0xBE6368", 0x00BE6360, 0x00FE6F60, "75 06", 0x00BE6368, 0x00FE6F68),
        new("The zero-result path calls through absolute cell VA 0x01AC4424; normal return resumes at raw 0xBE6368", 0x00BE6362, 0x00FE6F62, "FF 15 24 44 AC 01", NormalReturnContinuationRawFileOffset: 0x00BE6368, AbsoluteVirtualAddress: 0x01AC4424),
        new("Independently reload receiver DWORD +4 into EAX", 0x00BE6368, 0x00FE6F68, "8B 46 04"),
        new("Write the retained ECX DWORD to [EAX+EBX]", 0x00BE636B, 0x00FE6F6B, "89 0C 03")
    ];
}

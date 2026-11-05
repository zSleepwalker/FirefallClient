using Firefall.Reconstruction.Startup;

internal static class PayloadGrowthBufferAllocationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE identity, raw/VA mapping, exact bounded bytes, and call destinations are pinned",
            PayloadGrowthBufferAllocationProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadGrowthBufferAllocationProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadGrowthBufferAllocationProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PayloadGrowthBufferAllocationProjection.GateStartRawFileOffset == 0x00BE68E1
            && PayloadGrowthBufferAllocationProjection.GateStartVirtualAddress == 0x00FE74E1
            && PayloadGrowthBufferAllocationProjection.GateStartVirtualAddressFromPeSection
                == 0x00FE74E1
            && PayloadGrowthBufferAllocationProjection.GateStartVirtualAddressFromRawOffset
                == 0x00FE74E1
            && PayloadGrowthBufferAllocationProjection.GateEndRawFileOffsetExclusive
                == 0x00BE691A
            && PayloadGrowthBufferAllocationProjection.GateEndVirtualAddressExclusive
                == 0x00FE751A
            && PayloadGrowthBufferAllocationProjection.AllocatorTargetRawFileOffset
                == 0x012B7FB0
            && PayloadGrowthBufferAllocationProjection.AllocatorTargetVirtualAddress
                == 0x016B8BB0
            && PayloadGrowthBufferAllocationProjection.IndirectCallCellVirtualAddress
                == 0x01AC4424
            && string.Concat(PayloadGrowthBufferAllocationProjection.CapturedGateBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B7B0C2BF2C1EF03897DF8C745FC000000008D043285C074203DFF"
                    + "FFFF3F7713C1E00250E8A6166D0083C4048945FC85C07506"
                    + "FF152444AC01"
            && PayloadGrowthBufferAllocationProjection.Instructions.SequenceEqual(
                ExpectedInstructions()));

        PayloadGrowthBufferAllocationPlan eightFromZero = Project(
            initialCount: 0,
            finalCount: 8,
            receiverDword0C: 0x12345678,
            allocatorReturned: true,
            allocatorEax: 1);
        PayloadGrowthBufferAllocationPlan sixteenFromEight = Project(
            initialCount: 8,
            finalCount: 16,
            receiverDword0C: 0xFFFFFFFF,
            allocatorReturned: true,
            allocatorEax: 1);
        check("Valid growth entries reproduce final ESI, write the shifted receiver value, and preserve frame writes",
            eightFromZero.EntryConditionValid
            && eightFromZero.EntryGrowthDelta == 8
            && eightFromZero.EaxBeforeZeroTest == 8
            && eightFromZero.ZeroTestBranchTaken == false
            && eightFromZero.Outcome
                == PayloadGrowthBufferAllocationOutcome.AllocatorReturnedNonzero
            && eightFromZero.FrameMinus8Address == 0x1FF8
            && eightFromZero.FrameMinus8Value == 0x02468ACF
            && eightFromZero.FrameMinus8Written
            && eightFromZero.FrameMinus4Address == 0x1FFC
            && eightFromZero.FrameMinus4ZeroWritten
            && eightFromZero.FrameMinus4EaxWritePerformed
            && eightFromZero.FrameMinus4LastWrittenValue == 1
            && eightFromZero.AllocatorCallArgument == 32
            && eightFromZero.AllocatorStackCleanupBytes == 4
            && eightFromZero.RouteRawFileOffset
                == PayloadGrowthBufferAllocationProjection.GateEndRawFileOffsetExclusive
            && sixteenFromEight.EntryConditionValid
            && sixteenFromEight.EntryGrowthDelta == 8
            && sixteenFromEight.EaxBeforeZeroTest == 16
            && sixteenFromEight.FrameMinus8Value == 0x1FFFFFFF
            && sixteenFromEight.FrameMinus4LastWrittenValue == 1);

        PayloadGrowthBufferAllocationPlan invalidFinalCount = Project(
            initialCount: 4,
            finalCount: 7,
            receiverDword0C: null);
        PayloadGrowthBufferAllocationPlan invalidNoGrowth = Project(
            initialCount: 8,
            finalCount: 8,
            receiverDword0C: null);
        PayloadGrowthBufferAllocationPlan invalidWithLaterCaptures = Project(
            initialCount: 8,
            finalCount: 8,
            receiverDword0C: 0,
            allocatorReturned: true,
            allocatorEax: 1);
        check("Entry validation follows the actual unsigned difference and ESI>=8 JNB guards",
            invalidFinalCount.Outcome
                == PayloadGrowthBufferAllocationOutcome.InvalidEntryCondition
            && !invalidFinalCount.EntryConditionValid
            && invalidNoGrowth.Outcome
                == PayloadGrowthBufferAllocationOutcome.InvalidEntryCondition
            && invalidWithLaterCaptures.Outcome
                == PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided
            && !invalidWithLaterCaptures.ReceiverDword0CRead
            && !invalidWithLaterCaptures.AllocatorCallReached);

        PayloadGrowthBufferAllocationPlan missingFirst = Project(
            receiverDword0C: null);
        PayloadGrowthBufferAllocationPlan laterAfterMissingFirst = Project(
            receiverDword0C: null,
            allocatorReturned: true,
            allocatorEax: 0,
            indirectReturned: true);
        check("A missing receiver capture stops before all stores, and downstream observations are rejected",
            missingFirst.Outcome
                == PayloadGrowthBufferAllocationOutcome.MissingReceiverDword0CCapture
            && missingFirst.ReceiverDword0CAddress == 0x100C
            && !missingFirst.FrameMinus8Written
            && !missingFirst.FrameMinus4ZeroWritten
            && missingFirst.StopAtRawFileOffsetExclusive == 0x00BE68E1
            && laterAfterMissingFirst.Outcome
                == PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided);

        PayloadGrowthBufferAllocationPlan boundarySize = Project(
            finalCount: 0x3FFFFFFF,
            allocatorReturned: true,
            allocatorEax: 7);
        PayloadGrowthBufferAllocationPlan oversized = Project(
            finalCount: 0x40000000,
            indirectReturned: true);
        PayloadGrowthBufferAllocationPlan oversizedWithAllocatorCapture = Project(
            finalCount: 0x40000000,
            allocatorReturned: true);
        check("The unsigned size boundary allocates at 0x3FFFFFFF with wrapped scaling and bypasses allocation above it",
            boundarySize.EntryConditionValid
            && boundarySize.OversizeBranchTaken == false
            && boundarySize.AllocatorCallReached
            && boundarySize.AllocatorCallArgument == 0xFFFFFFFC
            && boundarySize.Outcome
                == PayloadGrowthBufferAllocationOutcome.AllocatorReturnedNonzero
            && oversized.EntryConditionValid
            && oversized.OversizeBranchTaken == true
            && !oversized.AllocatorCallReached
            && oversized.AllocatorCallArgument is null
            && oversized.IndirectCallReached
            && oversized.IndirectCallCellVirtualAddress == 0x01AC4424
            && oversized.Outcome
                == PayloadGrowthBufferAllocationOutcome.IndirectCallReturnedNormally
            && oversizedWithAllocatorCapture.Outcome
                == PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided);

        PayloadGrowthBufferAllocationPlan allocatorZeroMissingIndirect =
            Project(allocatorReturned: true, allocatorEax: 0);
        PayloadGrowthBufferAllocationPlan allocatorZeroIndirectNonreturn =
            Project(allocatorReturned: true, allocatorEax: 0, indirectReturned: false);
        PayloadGrowthBufferAllocationPlan allocatorZeroIndirectReturn =
            Project(allocatorReturned: true, allocatorEax: 0, indirectReturned: true);
        PayloadGrowthBufferAllocationPlan allocatorNonzeroUnreachedIndirect =
            Project(allocatorReturned: true, allocatorEax: 3, indirectReturned: true);
        check("Allocator zero reaches the independent indirect call, while nonzero skips its unreachable capture",
            allocatorZeroMissingIndirect.Outcome
                == PayloadGrowthBufferAllocationOutcome.MissingIndirectCallReturnCapture
            && allocatorZeroMissingIndirect.FrameMinus4ZeroWritten
            && allocatorZeroMissingIndirect.FrameMinus4EaxWritePerformed
            && allocatorZeroMissingIndirect.FrameMinus4LastWrittenValue == 0
            && allocatorZeroIndirectNonreturn.Outcome
                == PayloadGrowthBufferAllocationOutcome.IndirectCallDidNotReturnNormally
            && allocatorZeroIndirectReturn.Outcome
                == PayloadGrowthBufferAllocationOutcome.IndirectCallReturnedNormally
            && allocatorZeroIndirectReturn.AllocatorEax == 0
            && allocatorNonzeroUnreachedIndirect.Outcome
                == PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided);

        PayloadGrowthBufferAllocationPlan missingAllocatorReturn =
            Project(allocatorReturned: null);
        PayloadGrowthBufferAllocationPlan nonreturningAllocator =
            Project(allocatorReturned: false);
        PayloadGrowthBufferAllocationPlan nonreturningAllocatorWithLaterEax =
            Project(allocatorReturned: false, allocatorEax: 0);
        PayloadGrowthBufferAllocationPlan missingAllocatorEax =
            Project(allocatorReturned: true);
        PayloadGrowthBufferAllocationPlan missingAllocatorEaxWithLaterCall =
            Project(allocatorReturned: true, indirectReturned: true);
        check("Allocator return and EAX captures are separate; cleanup precedes the missing-EAX stop",
            missingAllocatorReturn.Outcome
                == PayloadGrowthBufferAllocationOutcome.MissingAllocatorCallReturnCapture
            && missingAllocatorReturn.AllocatorStackCleanupBytes is null
            && missingAllocatorReturn.StopAtRawFileOffsetExclusive == 0x00BE690A
            && nonreturningAllocator.Outcome
                == PayloadGrowthBufferAllocationOutcome.AllocatorCallDidNotReturnNormally
            && nonreturningAllocator.AllocatorStackCleanupBytes is null
            && nonreturningAllocatorWithLaterEax.Outcome
                == PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided
            && missingAllocatorEax.Outcome
                == PayloadGrowthBufferAllocationOutcome.MissingAllocatorEaxCapture
            && missingAllocatorEax.AllocatorStackCleanupBytes == 4
            && missingAllocatorEax.FrameMinus4ZeroWritten
            && !missingAllocatorEax.FrameMinus4EaxWritePerformed
            && missingAllocatorEax.FrameMinus4LastWrittenValue == 0
            && missingAllocatorEax.StopAtRawFileOffsetExclusive == 0x00BE690D
            && missingAllocatorEaxWithLaterCall.Outcome
                == PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided);

        PayloadGrowthBufferAllocationPlan wrappedFrame = Project(
            receiverPointer: uint.MaxValue,
            framePointer: 3,
            receiverDword0C: 0x80000007,
            allocatorReturned: true,
            allocatorEax: 5);
        check("Receiver and frame pointer arithmetic wraps as DWORD, and indirect-call effects are not synthesized",
            wrappedFrame.ReceiverDword0CAddress == 0x0B
            && wrappedFrame.FrameMinus8Address == 0xFFFFFFFB
            && wrappedFrame.FrameMinus4Address == 0xFFFFFFFF
            && wrappedFrame.FrameMinus8Value == 0x10000000
            && wrappedFrame.FrameMinus4LastWrittenValue == 5
            && wrappedFrame.IndirectCallCellVirtualAddress is null
            && wrappedFrame.Outcome
                == PayloadGrowthBufferAllocationOutcome.AllocatorReturnedNonzero);
    }

    private static PayloadGrowthBufferAllocationPlan Project(
        uint receiverPointer = 0x1000,
        uint framePointer = 0x2000,
        uint initialCount = 0,
        uint finalCount = 8,
        uint? receiverDword0C = 0x12345678,
        bool? allocatorReturned = null,
        uint? allocatorEax = null,
        bool? indirectReturned = null) =>
        PayloadGrowthBufferAllocationProjection.Project(new(
            ReceiverPointer: receiverPointer,
            FramePointer: framePointer,
            RetainedInitialCount: initialCount,
            RetainedFinalDoublingResult: finalCount,
            ReceiverDword0C: receiverDword0C,
            AllocatorCallReturnedNormally: allocatorReturned,
            AllocatorEax: allocatorEax,
            IndirectCallReturnedNormally: indirectReturned));

    private static PayloadGrowthBufferAllocationInstructionEvidence[] ExpectedInstructions() =>
    [
        new("Capture receiver DWORD +0x0C into EDI", 0x00BE68E1, 0x00FE74E1, "8B 7B 0C"),
        new("Subtract retained initial EDX from final ESI with DWORD wraparound", 0x00BE68E4, 0x00FE74E4, "2B F2"),
        new("Shift captured receiver DWORD right by three", 0x00BE68E6, 0x00FE74E6, "C1 EF 03"),
        new("Write the shifted value to frame-8", 0x00BE68E9, 0x00FE74E9, "89 7D F8"),
        new("Initialize frame-4 to zero", 0x00BE68EC, 0x00FE74EC, "C7 45 FC 00 00 00 00"),
        new("Form EAX as EDX plus the wrapped ESI difference, reproducing entry ESI", 0x00BE68F3, 0x00FE74F3, "8D 04 32"),
        new("Test EAX; the zero branch is unreachable after a valid entry guard", 0x00BE68F6, 0x00FE74F6, "85 C0"),
        new("JZ to raw 0xBE691A; valid entry requires the tested EAX to be at least eight", 0x00BE68F8, 0x00FE74F8, "74 20", 0x00BE691A, 0x00FE751A),
        new("Compare EAX unsigned against 0x3FFFFFFF", 0x00BE68FA, 0x00FE74FA, "3D FF FF FF 3F"),
        new("Unsigned JA sends oversized counts directly to the indirect call", 0x00BE68FF, 0x00FE74FF, "77 13", 0x00BE6914, 0x00FE7514),
        new("Scale the allocator size by four with DWORD wraparound", 0x00BE6901, 0x00FE7501, "C1 E0 02"),
        new("Push the scaled allocator argument", 0x00BE6904, 0x00FE7504, "50"),
        new("Call allocator raw 0x12B7FB0; normal return resumes at raw 0xBE690A", 0x00BE6905, 0x00FE7505, "E8 A6 16 6D 00", CallTargetRawFileOffset: 0x012B7FB0, CallTargetVirtualAddress: 0x016B8BB0, NormalReturnContinuationRawFileOffset: 0x00BE690A),
        new("Clean up the allocator argument after normal return", 0x00BE690A, 0x00FE750A, "83 C4 04"),
        new("After cleanup, store captured allocator EAX into frame-4", 0x00BE690D, 0x00FE750D, "89 45 FC"),
        new("Test captured allocator EAX", 0x00BE6910, 0x00FE7510, "85 C0"),
        new("A nonzero allocator result skips the indirect call", 0x00BE6912, 0x00FE7512, "75 06", 0x00BE691A, 0x00FE751A),
        new("Call through absolute cell VA 0x01AC4424; normal return resumes at raw 0xBE691A", 0x00BE6914, 0x00FE7514, "FF 15 24 44 AC 01", NormalReturnContinuationRawFileOffset: 0x00BE691A, AbsoluteVirtualAddress: 0x01AC4424)
    ];
}

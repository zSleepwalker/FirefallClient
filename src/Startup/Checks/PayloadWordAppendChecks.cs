using Firefall.Reconstruction.Startup;

internal static class PayloadWordAppendChecks
{
    public static void Run(Action<string, bool> check)
    {
        var lowBitsBranch = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(
                receiverDword0C: 1,
                receiverDword10: 0,
                receiverDword8: null,
                callReturnedNormally: null),
            SlotIndex: Index(slotDword: 0xABC),
            SlotAllocation: null,
            CopyTail: Tail()));
        var receiverBoundBranch = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(
                receiverDword0C: 0,
                receiverDword10: 0,
                receiverDword8: 2,
                callReturnedNormally: null),
            SlotIndex: Index(slotDword: 0xABC),
            SlotAllocation: null,
            CopyTail: Tail()));
        var missingIndexAfterLowBitsBranch = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(
                receiverDword0C: 1,
                receiverDword10: 0,
                receiverDword8: null,
                callReturnedNormally: null),
            SlotIndex: null,
            SlotAllocation: null,
            CopyTail: null));
        check("Both gate branches continue into slot indexing and the existing-slot copy tail",
            lowBitsBranch.Outcome
                == PayloadWordAppendOutcome.CompletedWithExistingSlot
            && lowBitsBranch.GrowthGatePlan?.Outcome
                == PayloadWordGrowthGateOutcome.LowBitsBranchToBoundary
            && receiverBoundBranch.Outcome
                == PayloadWordAppendOutcome.CompletedWithExistingSlot
            && receiverBoundBranch.GrowthGatePlan?.Outcome
                == PayloadWordGrowthGateOutcome.ReceiverBoundBranchesToBoundary
            && missingIndexAfterLowBitsBranch.Outcome
                == PayloadWordAppendOutcome.MissingSlotIndexStage);

        var gateUnreachedRoute = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(
                receiverDword0C: 1,
                receiverDword10: 0,
                receiverDword8: 0,
                callReturnedNormally: null),
            SlotIndex: Index(slotDword: 0xABC),
            SlotAllocation: null,
            CopyTail: Tail()));
        check("A boundary route with UnreachedCaptureProvided is rejected rather than accepted by route alone",
            gateUnreachedRoute.Outcome == PayloadWordAppendOutcome.GrowthGateInvalid
            && gateUnreachedRoute.GrowthGatePlan?.Outcome
                == PayloadWordGrowthGateOutcome.UnreachedCaptureProvided
            && gateUnreachedRoute.GrowthGatePlan?.RouteRawFileOffset
                == PayloadWordGrowthGateProjection.LowBitsBranchTargetRawFileOffset
            && gateUnreachedRoute.SlotIndexPlan is null);

        var missingGate = PayloadWordAppendProjection.Project(new(
            GrowthGate: null,
            SlotIndex: Index(receiverPointer: 0),
            SlotAllocation: null,
            CopyTail: null));
        var missingIndex = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(),
            SlotIndex: null,
            SlotAllocation: null,
            CopyTail: null));
        check("Missing earlier stages remain invalid and supplied partial plans are retained",
            missingGate.Outcome == PayloadWordAppendOutcome.MissingGrowthGateStage
            && missingGate.Capture.SlotIndex is not null
            && missingGate.SlotIndexPlan is null
            && missingIndex.Outcome == PayloadWordAppendOutcome.MissingSlotIndexStage
            && missingIndex.GrowthGatePlan?.Outcome
                == PayloadWordGrowthGateOutcome.HandoffCallReturnedNormally);

        var normalNonzero = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(),
            SlotIndex: Index(slotDword: 0xABC),
            SlotAllocation: null,
            CopyTail: Tail(
                retainedEdi: 0x1234,
                receiverDword4: 0xFFFF0000,
                incrementOldDword: uint.MaxValue)));
        var normalZero = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(),
            SlotIndex: Index(slotDword: 0),
            SlotAllocation: Allocation(receiverDword4AtSlotWrite: 0xDEADBEEF),
            CopyTail: Tail(
                retainedEdi: 0x1234,
                receiverDword4: 0xFFFFFFFF,
                slotPointer: 0x1000,
                stackArgumentPointer: 0x5000,
                word: 0x6677,
                incrementOldDword: 4)));
        check("Normal return composes nonzero-slot bypass and zero-slot allocation before the completed tail",
            normalNonzero.Outcome
                == PayloadWordAppendOutcome.CompletedWithExistingSlot
            && normalNonzero.SlotAllocationPlan is null
            && normalNonzero.CopyTailPlan?.Outcome
                == PayloadWordCopyTailOutcome.CandidateZeroSkippedWordCopy
            && normalZero.Outcome
                == PayloadWordAppendOutcome.CompletedAfterSlotAllocation
            && normalZero.SlotAllocationPlan?.Outcome
                == PayloadWordSlotAllocationOutcome.SlotDwordWritten
            && normalZero.CopyTailPlan?.Outcome
                == PayloadWordCopyTailOutcome.WordCopiedAndCounterIncremented);

        var forbiddenAllocation = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(),
            SlotIndex: Index(slotDword: 3),
            SlotAllocation: Allocation(),
            CopyTail: null));
        var incompleteAllocation = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(),
            SlotIndex: Index(slotDword: 0),
            SlotAllocation: Allocation(
                allocatorReturned: false,
                allocatorEax: null,
                indirectCallReturned: null,
                receiverDword4AtSlotWrite: null),
            CopyTail: Tail()));
        var missingAllocation = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(),
            SlotIndex: Index(slotDword: 0),
            SlotAllocation: null,
            CopyTail: null));
        check("A nonzero slot rejects allocation and a zero slot requires a completed write",
            forbiddenAllocation.Outcome
                == PayloadWordAppendOutcome.AllocationCaptureForNonzeroSlot
            && forbiddenAllocation.Capture.SlotAllocation is not null
            && forbiddenAllocation.SlotAllocationPlan is null
            && incompleteAllocation.Outcome
                == PayloadWordAppendOutcome.SlotAllocationInvalid
            && incompleteAllocation.SlotAllocationPlan?.Outcome
                == PayloadWordSlotAllocationOutcome.AllocatorCallDidNotReturnNormally
            && incompleteAllocation.CopyTailPlan is null
            && missingAllocation.Outcome
                == PayloadWordAppendOutcome.MissingSlotAllocationStage);

        var pointerMismatch = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(receiverPointer: 0),
            SlotIndex: Index(receiverPointer: 1),
            SlotAllocation: null,
            CopyTail: null));
        var ediMismatch = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(),
            SlotIndex: Index(slotDword: 1),
            SlotAllocation: null,
            CopyTail: Tail(retainedEdi: 0x1235, retainedEbx: 0x5678)));
        var ebxMismatch = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(),
            SlotIndex: Index(slotDword: 1),
            SlotAllocation: null,
            CopyTail: Tail(retainedEdi: 0x1234, retainedEbx: 1)));
        var allocationOffsetMismatch = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(),
            SlotIndex: Index(slotDword: 0),
            SlotAllocation: Allocation(retainedScaledOffset: 4),
            CopyTail: null));
        check("Receiver pointers and retained EDI/EBX must match their composed inputs",
            pointerMismatch.Outcome == PayloadWordAppendOutcome.ReceiverPointerMismatch
            && ediMismatch.Outcome == PayloadWordAppendOutcome.RetainedEdiMismatch
            && ebxMismatch.Outcome == PayloadWordAppendOutcome.RetainedEbxMismatch
            && allocationOffsetMismatch.Outcome
                == PayloadWordAppendOutcome.RetainedEbxMismatch);

        var independentReloads = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(),
            SlotIndex: Index(slotDword: 0),
            SlotAllocation: Allocation(receiverDword4AtSlotWrite: 0x11111111),
            CopyTail: Tail(
                receiverDword4: 0x22222222,
                slotPointer: 0xFFFFFFF8,
                framePointer: 0x33333333,
                incrementOldDword: 0)));
        check("Independent receiver reloads and the tail frame pointer remain independent observations",
            independentReloads.SlotAllocationPlan?.SlotDwordWritten == true
            && independentReloads.SlotAllocationPlan?.ReceiverDword4Read == true
            && independentReloads.CopyTailPlan?.ReceiverDword4Read == true
            && independentReloads.CopyTailPlan?.Capture.FramePointer == 0x33333333
            && independentReloads.CopyTailPlan?.StackArgumentPointerReadAddress is null);

        var completedCopy = PayloadWordAppendProjection.Project(new(
            GrowthGate: Gate(),
            SlotIndex: Index(slotDword: 0x10),
            SlotAllocation: null,
            CopyTail: Tail(
                retainedEdi: 0x1234,
                slotPointer: 0x1000,
                framePointer: 0x2000,
                stackArgumentPointer: 0x3000,
                word: 0xABCD,
                incrementOldDword: uint.MaxValue)));
        PayloadWordCopyTailPlan? completedTail = completedCopy.CopyTailPlan;
        check("The copy tail is accepted only at its RET 4 boundary with wrapping counter increment",
            completedCopy.Outcome
                == PayloadWordAppendOutcome.CompletedWithExistingSlot
            && completedTail is { } tailPlan
            && tailPlan.Outcome
                == PayloadWordCopyTailOutcome.WordCopiedAndCounterIncremented
            && tailPlan.WordCopyPerformed
            && tailPlan.WordCopyValue == 0xABCD
            && tailPlan.IncrementedDwordValue == 0
            && tailPlan.Ret4BoundaryRawFileOffsetExclusive
                == PayloadWordCopyTailProjection.Ret4BoundaryRawFileOffsetExclusive
            && PayloadWordAppendProjection.Project(new(
                GrowthGate: Gate(),
                SlotIndex: Index(slotDword: 1),
                SlotAllocation: null,
                CopyTail: null)).Outcome
                == PayloadWordAppendOutcome.MissingCopyTailStage);
    }

    private static PayloadWordGrowthGateCapture Gate(
        uint receiverPointer = 0,
        uint receiverDword0C = 0,
        uint receiverDword10 = 0,
        uint? receiverDword8 = 0,
        bool? callReturnedNormally = true) =>
        new(
            receiverPointer,
            receiverDword0C,
            receiverDword10,
            receiverDword8,
            callReturnedNormally);

    private static PayloadWordSlotIndexCapture Index(
        uint receiverPointer = 0,
        uint slotDword = 0,
        uint receiverDword8 = 1,
        uint receiverDword10 = 0x1234,
        uint receiverDword0CBeforeAnd = 0,
        uint receiverDword0CAfterAnd = 0,
        uint receiverDword4 = 0x1000) =>
        new(
            receiverPointer,
            receiverDword8,
            receiverDword10,
            receiverDword0CBeforeAnd,
            receiverDword0CAfterAnd,
            receiverDword4,
            slotDword);

    private static PayloadWordSlotAllocationCapture Allocation(
        uint receiverPointer = 0,
        uint retainedScaledOffset = 0,
        uint? precedingSlotDword = 0,
        bool? allocatorReturned = true,
        uint? allocatorEax = 0x99,
        bool? indirectCallReturned = null,
        uint? ecxAfterIndirectCall = null,
        uint? receiverDword4AtSlotWrite = 0x1000) =>
        new(
            receiverPointer,
            retainedScaledOffset,
            precedingSlotDword,
            allocatorReturned,
            allocatorEax,
            indirectCallReturned,
            ecxAfterIndirectCall,
            receiverDword4AtSlotWrite);

    private static PayloadWordCopyTailCapture Tail(
        uint receiverPointer = 0,
        uint retainedEdi = 0x1234,
        uint retainedEbx = 0,
        uint framePointer = 0x4000,
        uint receiverDword4 = 0x1000,
        uint slotPointer = 0xFFFFFFF8,
        uint? stackArgumentPointer = null,
        ushort? word = null,
        uint? incrementOldDword = 0) =>
        new(
            receiverPointer,
            retainedEdi,
            retainedEbx,
            framePointer,
            receiverDword4,
            slotPointer,
            stackArgumentPointer,
            word,
            incrementOldDword);
}

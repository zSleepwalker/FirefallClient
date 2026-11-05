using Firefall.Reconstruction.Startup;

internal static class FrameHandoffHelperEntrySlotChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffHelperEntrySlotPlan lowTwoRoute = Project(
            new(0x1111, 0x1000, 1, 0, null, null),
            new(0x1000, 0, 0, 0xFFFFFFFF, 0x55, 0x8000, 0));
        FrameHandoffHelperEntrySlotPlan greaterRoute = Project(
            new(0x2222, 0x2000, uint.MaxValue, 1, 2, null),
            new(0x2000, 2, 3, 0xFFFFFFFF, 0x77, 0x9000, 0));
        FrameHandoffHelperEntrySlotPlan helperReturnedRoute = Project(
            new(0x3333, 0x3000, 0, 0, 0, true),
            new(0x3000, 2, 1, 0xA5, 3, 0x8000, 0));

        check("Only the three exact BE64C5 routes admit the slot projection",
            lowTwoRoute.Outcome == FrameHandoffHelperEntrySlotOutcome.SlotProjected
            && lowTwoRoute.EntryPlan.Outcome
                == FrameHandoffHelperEntryOutcome.LowTwoSumBitsNonzero
            && lowTwoRoute.EntryPlan.StopAtRawFileOffsetExclusive
                == FrameHandoffHelperSlotProjection.EntryRawFileOffset
            && lowTwoRoute.SlotPlan?.Outcome
                == FrameHandoffHelperSlotOutcome.FirstSlotZeroFallsThrough
            && greaterRoute.Outcome == FrameHandoffHelperEntrySlotOutcome.SlotProjected
            && greaterRoute.EntryPlan.Outcome
                == FrameHandoffHelperEntryOutcome.ReceiverPlus08GreaterThanRoundedLength
            && greaterRoute.SlotPlan?.Outcome
                == FrameHandoffHelperSlotOutcome.FirstSlotZeroFallsThrough
            && greaterRoute.EntryPlan.Capture.ReceiverPlus10 == 1
            && greaterRoute.Capture.Slot.ReceiverPlus10 == 3
            && greaterRoute.SlotPlan?.ReceiverPlus10ReadValue == 3
            && greaterRoute.EntryPlan.Capture.ReceiverPlus0C == uint.MaxValue
            && greaterRoute.Capture.Slot.ReceiverPlus0CFresh == 0x77
            && greaterRoute.SlotPlan?.ReceiverPlus0CFreshReadValue == 0x77
            && helperReturnedRoute.Outcome
                == FrameHandoffHelperEntrySlotOutcome.SlotProjected
            && helperReturnedRoute.EntryPlan.Outcome
                == FrameHandoffHelperEntryOutcome.HelperReturnedNormally
            && helperReturnedRoute.SlotPlan?.Outcome
                == FrameHandoffHelperSlotOutcome.FirstSlotZeroFallsThrough);

        FrameHandoffHelperEntrySlotPlan missingCallReturn = Project(
            new(0x4444, 0x4000, 0, 0, 0, null),
            CompleteSlot(0x4000));
        FrameHandoffHelperEntrySlotPlan nonreturningCall = Project(
            new(0x5555, 0x5000, 0, 0, 0, false),
            CompleteSlot(0x5000));
        check("Missing and nonreturning helper calls preserve entry state and mark supplied slot reads unreached",
            missingCallReturn.EntryPlan.Outcome
                == FrameHandoffHelperEntryOutcome.MissingHelperReturnCapture
            && missingCallReturn.Outcome
                == FrameHandoffHelperEntrySlotOutcome.EntryDidNotAdmitContinuation
            && missingCallReturn.SlotPlan is null
            && AllSlotCapturesUnreached(missingCallReturn)
            && missingCallReturn.EntryPlan.HelperCallReached
            && missingCallReturn.EntryPlan.HelperReturnedNormally is null
            && nonreturningCall.EntryPlan.Outcome
                == FrameHandoffHelperEntryOutcome.HelperDidNotReturnNormally
            && nonreturningCall.Outcome
                == FrameHandoffHelperEntrySlotOutcome.EntryDidNotAdmitContinuation
            && nonreturningCall.SlotPlan is null
            && AllSlotCapturesUnreached(nonreturningCall)
            && nonreturningCall.EntryPlan.HelperReturnedNormally == false);

        FrameHandoffHelperEntrySlotPlan entryFlagsBlockRoute = Project(
            new(0x6666, 0x6000, 1, 0, 0, true),
            CompleteSlot(0x6000));
        check("An exact route is still rejected when it carries any entry unreached-capture flag",
            entryFlagsBlockRoute.EntryPlan.Outcome
                == FrameHandoffHelperEntryOutcome.LowTwoSumBitsNonzero
            && entryFlagsBlockRoute.EntryPlan.UnreachedReceiverPlus08CaptureProvided
            && entryFlagsBlockRoute.EntryPlan.UnreachedHelperReturnedNormallyCaptureProvided
            && entryFlagsBlockRoute.EntryPlan.SelectedDestinationRawFileOffset
                == FrameHandoffHelperEntryProjection.FirstBranchDestinationRawFileOffset
            && entryFlagsBlockRoute.Outcome
                == FrameHandoffHelperEntrySlotOutcome.EntryUnreachedCaptureWasProvided
            && entryFlagsBlockRoute.EntryRouteAdmitted
            && !entryFlagsBlockRoute.EntryUnreachedCapturesClear
            && entryFlagsBlockRoute.SlotPlan is null
            && entryFlagsBlockRoute.UnreachedReceiverBaseEsiCapture
            && AllSlotCapturesUnreached(entryFlagsBlockRoute));

        FrameHandoffHelperEntrySlotPlan identityMismatch = Project(
            new(0x7777, 0x7000, 0, 0, 0, true),
            CompleteSlot(0x7001));
        check("Receiver identity mismatch is rejected before slot writes",
            identityMismatch.Outcome
                == FrameHandoffHelperEntrySlotOutcome.ReceiverIdentityMismatch
            && identityMismatch.ReceiverIdentityChecked
            && !identityMismatch.ReceiverIdentityMatches
            && !identityMismatch.UnreachedReceiverBaseEsiCapture
            && identityMismatch.SlotPlan is null
            && identityMismatch.UnreachedReceiverPlus08CaptureProvided
            && identityMismatch.UnreachedReceiverPlus10CaptureProvided
            && identityMismatch.UnreachedReceiverPlus0COldCaptureProvided
            && identityMismatch.UnreachedReceiverPlus0CFreshCaptureProvided
            && identityMismatch.UnreachedReceiverPlus04CaptureProvided
            && identityMismatch.UnreachedFirstSlotDwordCaptureProvided);

        FrameHandoffHelperEntrySlotPlan partialSlotWrite = Project(
            new(0x8888, 0x8000, 0, 0, 0, true),
            new(0x8000, 2, 3, 0xFFFFFFFF, null, 0x8000, 0));
        check("A reached slot write remains projected even when the independent fresh reload is missing",
            partialSlotWrite.Outcome == FrameHandoffHelperEntrySlotOutcome.SlotProjected
            && partialSlotWrite.SlotPlan is FrameHandoffHelperSlotPlan partialSlot
            && partialSlot.Outcome
                == FrameHandoffHelperSlotOutcome.MissingReceiverPlus0CFreshCapture
            && partialSlot.ReceiverPlus0CWriteReached
            && partialSlot.ReceiverPlus0CWriteValue == 7
            && partialSlot.ReceiverPlus0CFreshReadReached
            && !partialSlot.ReceiverPlus0CFreshCaptured
            && !partialSlotWrite.UnreachedReceiverPlus0CFreshCaptureProvided
            && partialSlotWrite.UnreachedReceiverPlus04CaptureProvided
            && partialSlotWrite.UnreachedFirstSlotDwordCaptureProvided);

        FrameHandoffHelperEntrySlotPlan receiverZero = Project(
            new(0x9999, 0, 0, 0, 0, true),
            new(0, 0, 0, 0, 0, 0, 0));
        FrameHandoffHelperEntrySlotPlan receiverWrap = Project(
            new(0xAAAA, uint.MaxValue - 0x0B, 0, 0, 0, true),
            new(uint.MaxValue - 0x0B, 0x40000001, uint.MaxValue, uint.MaxValue, 2,
                0xFFFFFFFE, 0));
        FrameHandoffHelperEntrySlotPlan differentEntryFrame = Project(
            new(0, 0, 0, 0, 0, true),
            new(0, 0, 0, 0, 0, 0, 0));
        check("Receiver zero and wrapping receiver/slot arithmetic remain valid and independent of the entry frame",
            receiverZero.Outcome == FrameHandoffHelperEntrySlotOutcome.SlotProjected
            && receiverZero.SlotPlan?.ReceiverPlus08ReadAddress == 0x08
            && receiverZero.SlotPlan?.ReceiverPlus0COldReadAddress == 0x0C
            && receiverZero.SlotPlan?.ReceiverPlus0CFreshReadAddress == 0x0C
            && receiverWrap.Outcome == FrameHandoffHelperEntrySlotOutcome.SlotProjected
            && receiverWrap.SlotPlan?.ReceiverPlus08ReadAddress == 0xFFFFFFFC
            && receiverWrap.SlotPlan?.ReceiverPlus0COldReadAddress == 0
            && receiverWrap.SlotPlan?.ReceiverPlus0CWriteValue == 3
            && receiverWrap.SlotPlan?.FirstSlotAddress == 0xFFFFFFFE
            && receiverZero.SlotPlan == differentEntryFrame.SlotPlan);

        FrameHandoffHelperEntrySlotPlan nonzeroSlot = Project(
            new(0xBBBB, 0xB000, 0, 0, 0, true),
            new(0xB000, 2, 1, 0xA5, 3, 0x8000, 0x80000000));
        check("The first-slot zero and nonzero outcomes follow the slot plan exactly",
            helperReturnedRoute.SlotPlan?.Outcome
                == FrameHandoffHelperSlotOutcome.FirstSlotZeroFallsThrough
            && nonzeroSlot.SlotPlan is FrameHandoffHelperSlotPlan nonzeroSlotPlan
            && nonzeroSlotPlan.Outcome
                == FrameHandoffHelperSlotOutcome.FirstSlotNonzeroBranches
            && nonzeroSlotPlan.FirstSlotNonzero == true
            && nonzeroSlotPlan.SelectedDestinationRawFileOffset
                == FrameHandoffHelperSlotProjection.FirstSlotNonzeroBranchDestinationRawFileOffset);
    }

    private static FrameHandoffHelperEntrySlotPlan Project(
        FrameHandoffHelperEntryCapture entry,
        FrameHandoffHelperSlotCapture slot) =>
        FrameHandoffHelperEntrySlotProjection.Project(new(entry, slot));

    private static FrameHandoffHelperSlotCapture CompleteSlot(uint receiver) =>
        new(receiver, 2, 1, 0xA5, 3, 0x8000, 0);

    private static bool AllSlotCapturesUnreached(FrameHandoffHelperEntrySlotPlan plan) =>
        plan.SlotPlan is null
        && plan.UnreachedReceiverBaseEsiCapture
        && plan.UnreachedReceiverPlus08CaptureProvided
        && plan.UnreachedReceiverPlus10CaptureProvided
        && plan.UnreachedReceiverPlus0COldCaptureProvided
        && plan.UnreachedReceiverPlus0CFreshCaptureProvided
        && plan.UnreachedReceiverPlus04CaptureProvided
        && plan.UnreachedFirstSlotDwordCaptureProvided;
}

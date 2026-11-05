using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveOccupiedSlotChecks
{
    private const uint ReceiverBase = 0x10203040;
    private const uint FrameBase = 0x50607080;
    private const uint SlotRecordPointer = 0xA0B0C0D0;
    private const uint PayloadPointer = 0xD0C0B0A0;
    private const uint ReloadedReceiverPointer = 0x76543210;

    public static void Run(Action<string, bool> check)
    {
        ReliableReceiveOccupiedSlotPlan equalWords = ProjectValid(
            recordWord: 0x1234,
            payloadWord: 0x1234);
        ReliableReceiveOccupiedSlotPlan unequalWords = ProjectValid(
            recordWord: 0x1234,
            payloadWord: 0x5678,
            diagnosticReturnedNormally: true);
        check("Equal words skip diagnostics and unequal words advance only after normal diagnostic return",
            equalWords.Outcome == ReliableReceiveOccupiedSlotOutcome.OccupiedBranchCompleted
            && equalWords.RecordWordPlan?.Outcome
                == ReliableReceiveRecordWordOutcome.DiagnosticSkippedForEqualWords
            && equalWords.RecordWordPlan?.SelectedDestinationRawFileOffset
                == ReliableReceiveRecordWordProjection.EqualityBranchDestinationRawFileOffset
            && unequalWords.Outcome == ReliableReceiveOccupiedSlotOutcome.OccupiedBranchCompleted
            && unequalWords.RecordWordPlan?.Outcome
                == ReliableReceiveRecordWordOutcome.DiagnosticReturnedNormally
            && unequalWords.RecordWordPlan?.CleanupReached == true
            && equalWords.ForwardingReturnPlan?.StopReason
                == ReliableReceiveForwardingReturnStopReason.ReturnCompleted
            && unequalWords.ForwardingReturnPlan?.StopReason
                == ReliableReceiveForwardingReturnStopReason.ReturnCompleted);

        ReliableReceiveOccupiedSlotPlan emptySlot = Project(
            slotDword: 0,
            recordCapture: ValidRecordCapture(),
            callerCapture: ValidCallerCapture(),
            tailCapture: ValidTailCapture());
        ReliableReceiveOccupiedSlotPlan missingSlot = Project(
            slotDword: null,
            recordCapture: ValidRecordCapture(),
            callerCapture: ValidCallerCapture(),
            tailCapture: ValidTailCapture());
        ReliableReceiveOccupiedSlotPlan missingReceiver = ReliableReceiveOccupiedSlotProjection.Project(
            CreateInput(),
            new ReliableReceiveSlotGateCapture(null, SlotRecordPointer),
            ValidRecordCapture(),
            ValidCallerCapture(),
            ValidTailCapture());
        ReliableReceiveOccupiedSlotPlan invalidInput = ReliableReceiveOccupiedSlotProjection.Project(
            default,
            new ReliableReceiveSlotGateCapture(0, SlotRecordPointer),
            ValidRecordCapture(),
            ValidCallerCapture(),
            ValidTailCapture());
        check("Empty, missing, or invalid slot gates never project later captures",
            emptySlot.Outcome == ReliableReceiveOccupiedSlotOutcome.EmptySlotBranchSelected
            && emptySlot.SlotGatePlan.Outcome == ReliableReceiveSlotGateOutcome.ZeroSlotBranchSelected
            && emptySlot.RecordWordPlan is null
            && emptySlot.ForwardingReturnPlan is null
            && emptySlot.UnreachedRecordCaptureProvided
            && emptySlot.UnreachedCallerCaptureProvided
            && emptySlot.UnreachedRecycleReturnCaptureProvided
            && missingSlot.SlotGatePlan.Outcome == ReliableReceiveSlotGateOutcome.MissingSlotDwordCapture
            && missingSlot.RecordWordPlan is null
            && missingSlot.ForwardingReturnPlan is null
            && missingSlot.UnreachedRecordCaptureProvided
            && missingReceiver.SlotGatePlan.Outcome
                == ReliableReceiveSlotGateOutcome.UnreachedSlotDwordCapture
            && missingReceiver.RecordWordPlan is null
            && missingReceiver.ForwardingReturnPlan is null
            && missingReceiver.UnreachedRecordCaptureProvided
            && missingReceiver.UnreachedCallerCaptureProvided
            && missingReceiver.UnreachedRecycleReturnCaptureProvided
            && invalidInput.SlotGatePlan.Outcome
                == ReliableReceiveSlotGateOutcome.InvalidOrUnvalidatedDelta
            && invalidInput.RecordWordPlan is null
            && invalidInput.ForwardingReturnPlan is null
            && invalidInput.UnreachedRecordCaptureProvided
            && invalidInput.UnreachedCallerCaptureProvided
            && invalidInput.UnreachedRecycleReturnCaptureProvided);

        ReliableReceiveOccupiedSlotPlan pointerMismatch = Project(
            recordCapture: ValidRecordCapture() with
            {
                CapturedSlotRecordPointer = SlotRecordPointer + 1
            });
        check("The record read is rejected unless its pointer matches the captured slot DWORD",
            pointerMismatch.Outcome == ReliableReceiveOccupiedSlotOutcome.RecordPointerMismatch
            && pointerMismatch.RecordWordPlan is null
            && pointerMismatch.ForwardingReturnPlan is null
            && pointerMismatch.StopAtRawFileOffsetExclusive
                == ReliableReceiveRecordWordProjection.RecordWordReadRawFileOffset);

        ReliableReceiveOccupiedSlotPlan forbiddenEqualReturn = Project(
            recordCapture: ValidRecordCapture(diagnosticReturnedNormally: false));
        ReliableReceiveOccupiedSlotPlan diagnosticDidNotReturn = Project(
            recordCapture: ValidRecordCapture(
                recordWord: 1,
                payloadWord: 2,
                diagnosticReturnedNormally: false));
        check("An unreached equal-word diagnostic capture and a non-returning diagnostic cannot advance",
            forbiddenEqualReturn.Outcome
                == ReliableReceiveOccupiedSlotOutcome.RecordWordDidNotPermitForwarding
            && forbiddenEqualReturn.RecordWordPlan?.Outcome
                == ReliableReceiveRecordWordOutcome.UnreachedDiagnosticReturnCapture
            && forbiddenEqualReturn.RecordWordPlan?.SelectedDestinationRawFileOffset
                == ReliableReceiveRecordWordProjection.EqualityBranchDestinationRawFileOffset
            && forbiddenEqualReturn.RecordWordPlan?.UnreachedDiagnosticReturnCaptureProvided == true
            && forbiddenEqualReturn.ForwardingReturnPlan is null
            && forbiddenEqualReturn.UnreachedCallerCaptureProvided
            && forbiddenEqualReturn.UnreachedRecycleReturnCaptureProvided
            && diagnosticDidNotReturn.Outcome
                == ReliableReceiveOccupiedSlotOutcome.RecordWordDidNotPermitForwarding
            && diagnosticDidNotReturn.RecordWordPlan?.Outcome
                == ReliableReceiveRecordWordOutcome.DiagnosticDidNotReturnNormally
            && diagnosticDidNotReturn.ForwardingReturnPlan is null);

        ReliableReceiveOccupiedSlotPlan missingRecord = Project(
            recordCapture: null,
            useDefaultRecordCapture: false);
        ReliableReceiveOccupiedSlotPlan missingRecordWord = Project(
            recordCapture: ValidRecordCapture() with { RecordWordAtPointerPlusTwo = null });
        check("Missing record evidence stops before caller or recycle projection",
            missingRecord.Outcome == ReliableReceiveOccupiedSlotOutcome.MissingRecordWordCapture
            && missingRecord.RecordWordPlan is null
            && missingRecord.ForwardingReturnPlan is null
            && missingRecord.UnreachedCallerCaptureProvided
            && missingRecord.UnreachedRecycleReturnCaptureProvided
            && missingRecordWord.Outcome
                == ReliableReceiveOccupiedSlotOutcome.RecordWordDidNotPermitForwarding
            && missingRecordWord.RecordWordPlan?.Outcome
                == ReliableReceiveRecordWordOutcome.UnreachedPayloadWordCapture
            && missingRecordWord.ForwardingReturnPlan is null);

        ReliableReceiveOccupiedSlotPlan wrongCallerRoute = Project(
            callerCapture: ValidCallerCapture() with
            {
                Route = ReliableReceiveForwardingCallerRoute.HighDelta
            });
        ReliableReceiveOccupiedSlotPlan wrongCallerReceiver = Project(
            callerCapture: ValidCallerCapture() with
            {
                ReceiverBaseAddress = ReceiverBase + 1
            });
        ReliableReceiveOccupiedSlotPlan wrongTailPayload = Project(
            tailCapture: ValidTailCapture() with
            {
                PayloadPointerInEdi = PayloadPointer + 1
            });
        check("Caller route/base and independent record-to-tail payload pointer mismatches are rejected",
            wrongCallerRoute.Outcome
                == ReliableReceiveOccupiedSlotOutcome.ForwardingCallerRouteMismatch
            && wrongCallerRoute.ForwardingReturnPlan is null
            && wrongCallerReceiver.Outcome
                == ReliableReceiveOccupiedSlotOutcome.ForwardingCallerReceiverBaseMismatch
            && wrongCallerReceiver.ForwardingReturnPlan is null
            && wrongTailPayload.Outcome
                == ReliableReceiveOccupiedSlotOutcome.TailPayloadPointerMismatch
            && wrongTailPayload.ForwardingReturnPlan is null
            && wrongTailPayload.UnreachedRecycleReturnCaptureProvided);

        ReliableReceiveOccupiedSlotPlan wrongTailRoute = Project(
            tailCapture: ValidTailCapture() with
            {
                Route = ReliableReceiveRecycleReturnRoute.HighDelta
            });
        check("Tail route validation is delegated to the existing forwarding-return composition",
            wrongTailRoute.Outcome
                == ReliableReceiveOccupiedSlotOutcome.ForwardingReturnDidNotComplete
            && wrongTailRoute.ForwardingReturnPlan?.StopReason
                == ReliableReceiveForwardingReturnStopReason.TailRouteMismatch
            && wrongTailRoute.ForwardingReturnPlan?.CompletedStages
                == ReliableReceiveForwardingReturnCompletedStages.CallerCleanup
            && wrongTailRoute.ForwardingReturnPlan?.RecycleReturnPlan is null);

        ReliableReceiveOccupiedSlotPlan missingCaller = Project(
            callerCapture: null,
            useDefaultCallerCapture: false);
        ReliableReceiveOccupiedSlotPlan incompleteCaller = Project(
            callerCapture: ValidCallerCapture() with { ReceiverDwordAtOffset8 = null });
        check("Forwarding requires an ExistingRecord caller and a completed caller gates the tail",
            missingCaller.Outcome == ReliableReceiveOccupiedSlotOutcome.MissingForwardingCallerCapture
            && missingCaller.ForwardingReturnPlan is null
            && missingCaller.UnreachedRecycleReturnCaptureProvided
            && incompleteCaller.Outcome
                == ReliableReceiveOccupiedSlotOutcome.ForwardingReturnDidNotComplete
            && incompleteCaller.ForwardingReturnPlan?.StopReason
                == ReliableReceiveForwardingReturnStopReason.TailCaptureProvidedBeforeCallerCleanup
            && incompleteCaller.ForwardingReturnPlan?.RecycleReturnPlan is null);

        ReliableReceiveOccupiedSlotPlan partialEax = Project(
            tailCapture: ValidTailCapture() with { HelperReturnEax = null });
        check("Only completed caller cleanup and recycle RET 4 complete the branch; AL-only return suffices",
            partialEax.OccupiedBranchCompleted
            && partialEax.ForwardingReturnPlan?.StopReason
                == ReliableReceiveForwardingReturnStopReason.ReturnCompleted
            && partialEax.ForwardingReturnPlan?.CompletedStages
                == (ReliableReceiveForwardingReturnCompletedStages.CallerCleanup
                    | ReliableReceiveForwardingReturnCompletedStages.RecycleReturn)
            && partialEax.ForwardingReturnPlan?.RecycleReturnPlan?.AlValue == 1
            && partialEax.ForwardingReturnPlan?.RecycleReturnPlan?.EaxAfterAlWrite is null
            && partialEax.ForwardingReturnPlan?.RecycleReturnPlan?.Ret4Reached == true
            && partialEax.ForwardingReturnPlan?.CallerPlan.ReceiverOffset8DwordLoadedIntoEax
                == ReloadedReceiverPointer
            && partialEax.ForwardingReturnPlan?.CallerPlan.ReceiverOffset8DwordLoadedIntoEax
                != ReceiverBase
            && partialEax.ForwardingReturnPlan?.CallerPlan.Capture.ReceiverDwordAtOffset4
                == 0x11112222
            && partialEax.ForwardingReturnPlan?.CallerPlan.Capture.ReceiverDwordAtOffset24
                == 0x33334444
            && partialEax.SlotGatePlan.Input.FrameBaseAddress != ReceiverBase);
    }

    private static ReliableReceiveOccupiedSlotPlan Project(
        uint? slotDword = SlotRecordPointer,
        ReliableReceiveRecordWordCapture? recordCapture = null,
        ReliableReceiveForwardingCallerCapture? callerCapture = null,
        ReliableReceiveRecycleReturnCapture? tailCapture = null,
        bool useDefaultRecordCapture = true,
        bool useDefaultCallerCapture = true,
        bool useDefaultTailCapture = true) =>
        ReliableReceiveOccupiedSlotProjection.Project(
            CreateInput(),
            new ReliableReceiveSlotGateCapture(0, slotDword),
            useDefaultRecordCapture ? recordCapture ?? ValidRecordCapture() : recordCapture,
            useDefaultCallerCapture ? callerCapture ?? ValidCallerCapture() : callerCapture,
            useDefaultTailCapture ? tailCapture ?? ValidTailCapture() : tailCapture);

    private static ReliableReceiveOccupiedSlotPlan ProjectValid(
        ushort recordWord,
        ushort payloadWord,
        bool? diagnosticReturnedNormally = null) =>
        ReliableReceiveOccupiedSlotProjection.Project(
            CreateInput(),
            new ReliableReceiveSlotGateCapture(0, SlotRecordPointer),
            ValidRecordCapture(recordWord, payloadWord, diagnosticReturnedNormally),
            ValidCallerCapture(),
            ValidTailCapture());

    private static ReliableReceiveSlotGateValidatedInput CreateInput()
    {
        if (!ReliableReceiveSlotGateValidatedInput.TryCreate(
                ReceiverBase,
                FrameBase,
                0,
                out ReliableReceiveSlotGateValidatedInput input))
        {
            throw new InvalidOperationException("The test input must satisfy the slot-gate precondition.");
        }

        return input;
    }

    private static ReliableReceiveRecordWordCapture ValidRecordCapture(
        ushort recordWord = 0x1234,
        ushort payloadWord = 0x1234,
        bool? diagnosticReturnedNormally = null) =>
        new(
            SlotRecordPointer,
            PayloadPointer,
            recordWord,
            payloadWord,
            diagnosticReturnedNormally);

    private static ReliableReceiveForwardingCallerCapture ValidCallerCapture() =>
        new(
            ReliableReceiveForwardingCallerRoute.ExistingRecord,
            ReceiverBase,
            ReceiverDwordAtOffset4: 0x11112222,
            ReceiverDwordAtOffset24: 0x33334444,
            HelperReturnedNormally: true,
            ReceiverDwordAtOffset8: ReloadedReceiverPointer);

    private static ReliableReceiveRecycleReturnCapture ValidTailCapture() =>
        new(
            ReliableReceiveRecycleReturnRoute.ExistingRecord,
            ReloadedReceiverPointer,
            PayloadPointer,
            OldCounterDword: 7,
            GlobalDword: 0xABCDEF01,
            HelperReturnedNormally: true,
            HelperReturnEax: 0x10293847);
}

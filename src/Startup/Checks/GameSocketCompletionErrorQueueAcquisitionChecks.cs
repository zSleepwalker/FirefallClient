using Firefall.Reconstruction.Startup;

internal static class GameSocketCompletionErrorQueueAcquisitionChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("Composition anchors retain the parent call target, token, child acquisition target, and first tag-read boundary",
            GameSocketCompletionErrorQueueAcquisitionProjection.ParentQueueAppendCallRawFileOffset
                == 0xBD2E14
            && GameSocketCompletionErrorQueueAcquisitionProjection.ParentQueueAppendTargetRawFileOffset
                == 0x12CE310
            && GameSocketCompletionErrorQueueAcquisitionProjection.ParentQueueAppendTargetVirtualAddress
                == 0x16CEF10
            && GameSocketCompletionErrorQueueAcquisitionProjection.ChildCounterXaddRawFileOffset
                == 0x12CE32A
            && GameSocketCompletionErrorQueueAcquisitionProjection.ChildAcquisitionCallRawFileOffset
                == 0x12CE334
            && GameSocketCompletionErrorQueueAcquisitionProjection.ChildAcquisitionTargetRawFileOffset
                == 0x12CDE60
            && GameSocketCompletionErrorQueueAcquisitionProjection.ChildAcquisitionTargetVirtualAddress
                == 0x16CEA60
            && GameSocketCompletionErrorQueueAcquisitionProjection.ChildStopBeforeFirstTailTagReadRawFileOffset
                == 0x12CE364);

        GameSocketCompletionErrorQueueAcquisitionPlan zeroOld = Project(
            Parent(0x1000, 0, 0xDEADBEEF),
            QueueAppendAcquisitionCapture<uint>.Captured(uint.MaxValue),
            QueueAppendAcquisitionCapture<uint>.Captured(0x12345678),
            QueueAppendAcquisitionCapture<uint>.Captured(0));
        QueueAppendAcquisitionPlan zeroChild = zeroOld.ChildAcquisitionPlan!.Value;
        check("Zero parent state commits ten and copies the fresh field before deriving queue this and projecting token two",
            zeroOld.CleanParentPreCallHandoffValidated
            && zeroOld.ParentPlan.Outcome
                == GameSocketCompletionErrorStateOutcome.MissingQueueAppendReturnObservation
            && zeroOld.ParentPlan.UnusedCaptures.IsEmpty
            && zeroOld.ParentPlan.MemoryEffects.Length == 4
            && zeroOld.ParentPlan.Writes.Length == 2
            && zeroOld.ParentPlan.Writes[0].Value == 10
            && zeroOld.ParentPlan.Writes[1].Value == 0xDEADBEEF
            && zeroOld.DerivedQueueReceiverPointer == 0x1260
            && zeroOld.QueueToken == 2
            && zeroOld.ParentQueueAppendTargetRawFileOffset == 0x12CE310
            && zeroOld.ParentQueueAppendTargetVirtualAddress == 0x16CEF10
            && zeroChild.Captures.ReceiverPointer == 0x1260
            && zeroChild.CounterAddress == 0x1270
            && zeroChild.CounterOldValue == uint.MaxValue
            && zeroChild.CounterNewValue == 0
            && zeroChild.AcquisitionResult == 0
            && zeroOld.ChildCaptureStates.CounterOldValue
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured
            && zeroOld.ChildCaptureStates.GlobalPoolPointer
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured
            && zeroOld.ChildCaptureStates.AcquisitionResult
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured
            && zeroOld.ParentQueueAppendReturnObserved == false
            && zeroOld.ParentQueueAppendReturnedNormally is null
            && zeroOld.ParentPostCallContinuationEntered == false
            && zeroOld.LastReachedRawFileOffset == 0x12CE360
            && zeroOld.StopBeforeRawFileOffset == 0x12CE364
            && zeroOld.ChildAcquisitionReachedBoundedBoundary
            && zeroOld.ReachedBoundedProjectionBoundary);

        check("Parent CMP and TEST flags survive to the CALL handoff but are not propagated through the child prologue",
            zeroOld.ParentCmpFlags == zeroOld.ParentPlan.FlagsAfterCompareExchange
            && zeroOld.ParentCmpFlags is
                { Carry: false, Parity: true, AuxiliaryCarry: false, Zero: true, Sign: false, Overflow: false }
            && zeroOld.ParentEflagsAtCallHandoff == zeroOld.ParentPlan.FlagsAfterTestEax
            && zeroOld.ParentEflagsAtCallHandoff is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: true, Sign: false, Overflow: false }
            && zeroOld.ChildFlagsAfterPrologue is null
            && zeroOld.ChildNativeInstructionsExecuted == false
            && zeroOld.ChildQueueAppendEntryObserved == false
            && zeroOld.ChildAcquisitionNativeCallExecuted == false
            && zeroOld.ChildHelperInstructionsProjected
            && zeroOld.FirstTailTagReadNotReached
            && zeroOld.FlagsAtStop is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: true, Sign: false, Overflow: false }
            && zeroChild.Actions.Any(action =>
                action.Kind == QueueAppendAcquisitionActionKind.CallAcquisitionFunction)
            && zeroOld.ChildNodeFieldsRead == false);

        check("The ordered trace reuses parent effects and generic child actions without entering CAS or publication",
            zeroOld.OrderedTrace.Take(4).Select(step => step.ParentMemoryEffect?.Kind).SequenceEqual(
            [
                GameSocketCompletionErrorStateMemoryEffectKind.AtomicStateRead,
                GameSocketCompletionErrorStateMemoryEffectKind.AtomicConditionalStateWrite,
                GameSocketCompletionErrorStateMemoryEffectKind.FreshFieldRead,
                GameSocketCompletionErrorStateMemoryEffectKind.CopyDestinationWrite
            ])
            && zeroOld.OrderedTrace.Take(4).All(step => step.ChildAction is null)
            && zeroOld.OrderedTrace[4].ChildAction is
                { Kind: QueueAppendAcquisitionActionKind.IncrementReceiverCounterAndCaptureOldValue,
                  InstructionRawFileOffset: 0x12CE32A,
                  Address: 0x1270,
                  Value: uint.MaxValue,
                  RelatedValue: 0 }
            && zeroOld.OrderedTrace[^1].ChildAction is
                { Kind: QueueAppendAcquisitionActionKind.StopBeforeReceiverWordCRead,
                  InstructionRawFileOffset: 0x12CE364 }
            && zeroChild.Actions.Single(action =>
                action.Kind == QueueAppendAcquisitionActionKind.AttemptUnconditionalFirstDwordZero)
                .InstructionRawFileOffset == 0x12CE353
            && zeroChild.Actions.Single(action =>
                action.Kind == QueueAppendAcquisitionActionKind.AttemptUnconditionalSecondDwordZero)
                .InstructionRawFileOffset == 0x12CE359
            && !zeroChild.Actions.Any(action =>
                action.InstructionRawFileOffset == 0x12CE3A5
                || action.InstructionRawFileOffset == 0x12CE3ED));

        check("A captured acquisition result projects its target and branch without claiming an outer queue return",
            zeroOld.Outcome
                == GameSocketCompletionErrorQueueAcquisitionOutcome.ChildAcquisitionBranchProjected
            && zeroOld.ChildAcquisitionPlan?.Outcome
                == QueueAppendAcquisitionOutcome.NullResultBranchProjected
            && zeroChild.Actions.Single(action =>
                action.Kind == QueueAppendAcquisitionActionKind.CallAcquisitionFunction)
                is
                {
                    TargetRawFileOffset: 0x12CDE60,
                    TargetVirtualAddress: 0x16CEA60
                }
            && zeroOld.ParentQueueAppendReturnObserved == false
            && zeroOld.ParentPostCallContinuationEntered == false
            && zeroOld.HasActualBoundedCompletion == false
            && !zeroOld.ParentPlan.ReachedBoundedPostCallContinuation
            && !zeroOld.ParentPlan.HasActualBoundedCompletion);

        GameSocketCompletionErrorQueueAcquisitionPlan nonzeroOld = Project(
            Parent(0x2000, 7, null),
            QueueAppendAcquisitionCapture<uint>.Captured(5),
            QueueAppendAcquisitionCapture<uint>.Captured(0xABC),
            QueueAppendAcquisitionCapture<uint>.Captured(0x98765432));
        check("Nonzero parent state skips the fresh read and keeps its CAS and TEST flags at the child call handoff",
            nonzeroOld.CleanParentPreCallHandoffValidated
            && nonzeroOld.ParentPlan.StateCompareExchangeWriteCommitted == false
            && nonzeroOld.ParentPlan.ZeroTestBranchTaken == true
            && nonzeroOld.ParentPlan.Writes.IsEmpty
            && nonzeroOld.ParentPlan.MemoryEffects.Length == 2
            && nonzeroOld.ParentPlan.CaptureStates.FreshField108
                == GameSocketCompletionErrorStateCaptureState.NotReached
            && nonzeroOld.ParentPlan.UnusedCaptures.IsEmpty
            && nonzeroOld.ParentCmpFlags == nonzeroOld.ParentPlan.FlagsAfterCompareExchange
            && nonzeroOld.ParentEflagsAtCallHandoff == nonzeroOld.ParentPlan.FlagsAfterTestEax
            && nonzeroOld.DerivedQueueReceiverPointer == 0x2260
            && nonzeroOld.ChildAcquisitionPlan?.Outcome
                == QueueAppendAcquisitionOutcome.NonzeroResultBranchProjected
            && nonzeroOld.FirstTailTagReadNotReached
            && nonzeroOld.FlagsAtStop is
                { Carry: false, Parity: false, AuxiliaryCarry: null, Zero: false, Sign: true, Overflow: false });

        GameSocketCompletionErrorQueueAcquisitionPlan missingChildCounter = Project(
            Parent(0x3000, 0, 0x55AA),
            QueueAppendAcquisitionCapture<uint>.Missing(),
            QueueAppendAcquisitionCapture<uint>.Captured(0x1234),
            QueueAppendAcquisitionCapture<uint>.Captured(0x5678));
        check("A missing child XADD capture leaves later supplied inputs unused and stops at the child counter boundary",
            missingChildCounter.CleanParentPreCallHandoffValidated
            && missingChildCounter.ChildAcquisitionPlan?.Outcome
                == QueueAppendAcquisitionOutcome.MissingCounterCapture
            && missingChildCounter.ChildAcquisitionPlan!.Value.Actions.Single().InstructionRawFileOffset
                == 0x12CE32A
            && missingChildCounter.ParentPlan.Writes.Length == 2
            && missingChildCounter.ChildCaptureStates.CounterOldValue
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.Missing
            && missingChildCounter.ChildCaptureStates.GlobalPoolPointer
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.SuppliedButUnused
            && missingChildCounter.ChildCaptureStates.AcquisitionResult
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.SuppliedButUnused
            && missingChildCounter.ChildNativeInstructionsExecuted == false
            && missingChildCounter.ParentQueueAppendReturnObserved == false
            && missingChildCounter.LastReachedRawFileOffset == 0x12CE32A
            && missingChildCounter.StopBeforeRawFileOffset == 0x12CE32A
            && missingChildCounter.FlagsAtStop is null);

        GameSocketCompletionErrorQueueAcquisitionPlan missingChildCounterAndLaterCaptures = Project(
            Parent(0x3800, 0x11, null),
            QueueAppendAcquisitionCapture<uint>.Missing(),
            QueueAppendAcquisitionCapture<uint>.Missing(),
            QueueAppendAcquisitionCapture<uint>.Missing());
        check("Missing later child captures remain not-reached when the counter gate prevents their loads",
            missingChildCounterAndLaterCaptures.ChildAcquisitionPlan?.Outcome
                == QueueAppendAcquisitionOutcome.MissingCounterCapture
            && missingChildCounterAndLaterCaptures.ChildCaptureStates.GlobalPoolPointer
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.NotReached
            && missingChildCounterAndLaterCaptures.ChildCaptureStates.AcquisitionResult
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.NotReached);

        GameSocketCompletionErrorQueueAcquisitionPlan missingChildResult = Project(
            Parent(0x4000, 0xA, null),
            QueueAppendAcquisitionCapture<uint>.Captured(8),
            QueueAppendAcquisitionCapture<uint>.Captured(0xBEEF),
            QueueAppendAcquisitionCapture<uint>.Missing());
        check("A missing opaque acquisition result stops at its result store while leaving the parent CALL return unobserved",
            missingChildResult.ChildAcquisitionPlan?.Outcome
                == QueueAppendAcquisitionOutcome.MissingAcquisitionResult
            && missingChildResult.ChildAcquisitionPlan!.Value.Actions[^1].InstructionRawFileOffset
                == 0x12CE339
            && missingChildResult.ChildAcquisitionPlan.Value.Actions.Any(action =>
                action.Kind == QueueAppendAcquisitionActionKind.CallAcquisitionFunction
                && action.TargetRawFileOffset == 0x12CDE60)
            && missingChildResult.ChildCaptureStates.AcquisitionResult
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.Missing
            && missingChildResult.ParentPlan.Outcome
                == GameSocketCompletionErrorStateOutcome.MissingQueueAppendReturnObservation
            && !missingChildResult.ParentPostCallContinuationEntered
            && missingChildResult.ParentQueueAppendReturnObserved == false
            && missingChildResult.LastReachedRawFileOffset == 0x12CE339
            && missingChildResult.StopBeforeRawFileOffset == 0x12CE339
            && missingChildResult.FlagsAtStop is null);

        GameSocketCompletionErrorQueueAcquisitionPlan missingGlobal = Project(
            Parent(0x4800, 9, null),
            QueueAppendAcquisitionCapture<uint>.Captured(7),
            QueueAppendAcquisitionCapture<uint>.Missing(),
            QueueAppendAcquisitionCapture<uint>.Captured(0x99));
        check("A missing global-pool load preserves XADD flags and leaves a supplied result unused",
            missingGlobal.ChildAcquisitionPlan?.Outcome
                == QueueAppendAcquisitionOutcome.MissingGlobalPoolCapture
            && missingGlobal.ChildCaptureStates.CounterOldValue
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured
            && missingGlobal.ChildCaptureStates.GlobalPoolPointer
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.Missing
            && missingGlobal.ChildCaptureStates.AcquisitionResult
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.SuppliedButUnused
            && missingGlobal.ChildFlagsAfterCounterXadd is
                { Carry: false, Parity: false, AuxiliaryCarry: false, Zero: false, Sign: false, Overflow: false }
            && missingGlobal.FlagsAtStop == missingGlobal.ChildFlagsAfterCounterXadd
            && missingGlobal.LastReachedRawFileOffset == 0x12CE32E
            && missingGlobal.StopBeforeRawFileOffset == 0x12CE32E
            && missingGlobal.ChildAcquisitionReachedBoundedBoundary == false);

        GameSocketCompletionErrorQueueAcquisitionPlan missingParentAtomic = Project(
            new GameSocketCompletionErrorStateCapture(
                true, 0x5000, null, null, null),
            QueueAppendAcquisitionCapture<uint>.Captured(1),
            QueueAppendAcquisitionCapture<uint>.Captured(2),
            QueueAppendAcquisitionCapture<uint>.Captured(3));
        check("A dirty parent missing its CAS observation does not enter the child and marks supplied child captures unused",
            missingParentAtomic.Outcome
                == GameSocketCompletionErrorQueueAcquisitionOutcome.ParentPreCallHandoffRejected
            && missingParentAtomic.ParentPlan.Outcome
                == GameSocketCompletionErrorStateOutcome.MissingAtomicObservation
            && !missingParentAtomic.CleanParentPreCallHandoffValidated
            && missingParentAtomic.ChildAcquisitionPlan is null
            && missingParentAtomic.ChildCaptureStates.CounterOldValue
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.SuppliedButUnused
            && missingParentAtomic.ChildCaptureStates.GlobalPoolPointer
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.SuppliedButUnused
            && missingParentAtomic.ChildCaptureStates.AcquisitionResult
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.SuppliedButUnused
            && !missingParentAtomic.ChildHelperInstructionsProjected
            && missingParentAtomic.ParentQueueAppendTargetRawFileOffset is null
            && missingParentAtomic.QueueToken is null);

        GameSocketCompletionErrorQueueAcquisitionPlan missingParentFresh = Project(
            Parent(0x6000, 0, null, queueReturnedNormally: true),
            QueueAppendAcquisitionCapture<uint>.Captured(1),
            QueueAppendAcquisitionCapture<uint>.Captured(2),
            QueueAppendAcquisitionCapture<uint>.Captured(3));
        check("A missing parent fresh-field read preserves the parent's bounded stop without entering the queue helper",
            missingParentFresh.ParentPlan.Outcome
                == GameSocketCompletionErrorStateOutcome.MissingFreshField108
            && missingParentFresh.ParentPlan.LastReachedRawFileOffset
                == GameSocketCompletionErrorStateProjection.FreshFieldReadRawFileOffset
            && missingParentFresh.ParentPlan.StopBeforeRawFileOffset
                == GameSocketCompletionErrorStateProjection.FreshFieldReadRawFileOffset
            && missingParentFresh.ParentCapture.QueueAppendReturnedNormally == true
            && missingParentFresh.ParentPlan.QueueAppendReturnedNormally is null
            && missingParentFresh.ParentPlan.CaptureStates.QueueAppendReturnedNormally
                == GameSocketCompletionErrorStateCaptureState.SuppliedButUnused
            && missingParentFresh.ParentPlan.UnusedCaptures.Contains(
                GameSocketCompletionErrorStateCaptureKind.QueueAppendReturnedNormally)
            && missingParentFresh.ChildAcquisitionPlan is null
            && missingParentFresh.ParentQueueAppendReturnObserved == false
            && missingParentFresh.LastReachedRawFileOffset
                == GameSocketCompletionErrorStateProjection.FreshFieldReadRawFileOffset
            && missingParentFresh.StopBeforeRawFileOffset
                == GameSocketCompletionErrorStateProjection.FreshFieldReadRawFileOffset
            && missingParentFresh.FlagsAtStop == missingParentFresh.ParentPlan.FlagsAtStop
            && !missingParentFresh.CleanParentPreCallHandoffValidated);

        GameSocketCompletionErrorQueueAcquisitionPlan unusedParentFresh = Project(
            Parent(0x6800, 7, 0x12345678),
            QueueAppendAcquisitionCapture<uint>.Captured(1),
            QueueAppendAcquisitionCapture<uint>.Captured(2),
            QueueAppendAcquisitionCapture<uint>.Captured(3));
        check("A nonzero parent CAS rejects an unused fresh-field capture instead of admitting the child",
            unusedParentFresh.ParentPlan.Outcome
                == GameSocketCompletionErrorStateOutcome.MissingQueueAppendReturnObservation
            && unusedParentFresh.ParentPlan.CaptureStates.FreshField108
                == GameSocketCompletionErrorStateCaptureState.SuppliedButUnused
            && unusedParentFresh.ParentPlan.UnusedCaptures.Contains(
                GameSocketCompletionErrorStateCaptureKind.FreshField108)
            && unusedParentFresh.ChildAcquisitionPlan is null
            && unusedParentFresh.ChildCaptureStates.CounterOldValue
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.SuppliedButUnused
            && unusedParentFresh.LastReachedRawFileOffset == 0xBD2E14
            && unusedParentFresh.StopBeforeRawFileOffset == 0xBD2E19);

        GameSocketCompletionErrorQueueAcquisitionPlan globalCounterAlias = Project(
            Parent(0x01E0014C, 0, 0xCAFEBABE),
            QueueAppendAcquisitionCapture<uint>.Captured(0x7FFFFFFF),
            QueueAppendAcquisitionCapture<uint>.Captured(0x80000000),
            QueueAppendAcquisitionCapture<uint>.Captured(0x80000000));
        QueueAppendAcquisitionPlan aliasChild =
            globalCounterAlias.ChildAcquisitionPlan!.Value;
        check("The queue counter aliases the later pool-pointer load after XADD while parent state and copy slots stay distinct",
            globalCounterAlias.CleanParentPreCallHandoffValidated
            && globalCounterAlias.DerivedQueueReceiverPointer == 0x01E003AC
            && aliasChild.CounterAddress
                == QueueAppendAcquisitionProjection.GlobalPoolPointerVirtualAddress
            && aliasChild.CounterAddress == 0x01E003BC
            && globalCounterAlias.ParentPlan.EsiStateSlotAddress == 0x01E00270
            && globalCounterAlias.ParentPlan.EsiCopyDestinationAddress == 0x01E00274
            && aliasChild.CounterAddress != globalCounterAlias.ParentPlan.EsiStateSlotAddress
            && aliasChild.CounterAddress != globalCounterAlias.ParentPlan.EsiCopyDestinationAddress
            && aliasChild.CounterOldValue == 0x7FFFFFFF
            && aliasChild.CounterNewValue == 0x80000000
            && aliasChild.Actions.Single(action =>
                action.Kind == QueueAppendAcquisitionActionKind.LoadIndependentGlobalPoolPointerIntoEcx)
                is { Address: 0x01E003BC, Value: 0x80000000 }
            && globalCounterAlias.ChildFlagsAfterCounterXadd is
                { Carry: false, Parity: true, AuxiliaryCarry: true, Zero: false, Sign: true, Overflow: true }
            && globalCounterAlias.FlagsAtStop is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: false, Sign: true, Overflow: false });

        GameSocketCompletionErrorQueueAcquisitionPlan queueAlreadyReturned = Project(
            Parent(0x7000, 0, 0, queueReturnedNormally: true),
            QueueAppendAcquisitionCapture<uint>.Captured(4),
            QueueAppendAcquisitionCapture<uint>.Captured(5),
            QueueAppendAcquisitionCapture<uint>.Captured(6));
        GameSocketCompletionErrorQueueAcquisitionPlan queueDidNotReturn = Project(
            Parent(0x8000, 0, 0, queueReturnedNormally: false),
            QueueAppendAcquisitionCapture<uint>.Captured(4),
            QueueAppendAcquisitionCapture<uint>.Captured(5),
            QueueAppendAcquisitionCapture<uint>.Captured(6));
        check("Either observed outer queue-return value is rejected rather than projected as a child-entry handoff",
            queueAlreadyReturned.ParentPlan.Outcome
                == GameSocketCompletionErrorStateOutcome.BoundedAtQueueAppendReturn
            && queueAlreadyReturned.ParentCapture.QueueAppendReturnedNormally == true
            && queueAlreadyReturned.ChildAcquisitionPlan is null
            && queueAlreadyReturned.ParentQueueAppendReturnObserved
            && queueAlreadyReturned.ParentQueueAppendReturnedNormally == true
            && queueAlreadyReturned.ParentQueueAppendTargetRawFileOffset == 0x12CE310
            && queueAlreadyReturned.QueueToken == 2
            && queueAlreadyReturned.ChildQueueAppendEntryImpliedByNormalParentReturn
            && queueAlreadyReturned.ParentPostCallContinuationEntered
            && queueAlreadyReturned.HasActualBoundedCompletion
            && queueAlreadyReturned.ReachedBoundedProjectionBoundary
            && queueDidNotReturn.ParentPlan.Outcome
                == GameSocketCompletionErrorStateOutcome.QueueAppendDidNotReturnNormally
            && queueDidNotReturn.ParentCapture.QueueAppendReturnedNormally == false
            && queueDidNotReturn.ChildAcquisitionPlan is null
            && queueDidNotReturn.ParentQueueAppendReturnObserved
            && queueDidNotReturn.ParentQueueAppendReturnedNormally == false
            && !queueDidNotReturn.ParentPostCallContinuationEntered
            && !queueDidNotReturn.HasActualBoundedCompletion
            && !queueDidNotReturn.ReachedBoundedProjectionBoundary
            && !queueAlreadyReturned.CleanParentPreCallHandoffValidated
            && !queueDidNotReturn.CleanParentPreCallHandoffValidated);

        GameSocketCompletionErrorQueueAcquisitionPlan missingParentEntry = Project(
            new GameSocketCompletionErrorStateCapture(
                null, 0x9000, 0, 0, null),
            QueueAppendAcquisitionCapture<uint>.Captured(1),
            QueueAppendAcquisitionCapture<uint>.Captured(2),
            QueueAppendAcquisitionCapture<uint>.Captured(3));
        check("A parent that never validates its continuation retains its exact bounded stop and leaves child inputs unused",
            missingParentEntry.ParentPlan.Outcome
                == GameSocketCompletionErrorStateOutcome.MissingContinuationValidation
            && missingParentEntry.ParentPlan.StopBeforeRawFileOffset
                == GameSocketCompletionErrorStateProjection.ContinuationRawFileOffset
            && missingParentEntry.ChildAcquisitionPlan is null
            && missingParentEntry.ChildCaptureStates.CounterOldValue
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.SuppliedButUnused);
    }

    private static GameSocketCompletionErrorQueueAcquisitionPlan Project(
        GameSocketCompletionErrorStateCapture parent,
        QueueAppendAcquisitionCapture<uint> counterOldValue,
        QueueAppendAcquisitionCapture<uint> globalPoolPointer,
        QueueAppendAcquisitionCapture<uint> acquisitionResult) =>
        GameSocketCompletionErrorQueueAcquisitionProjection.Project(
            parent,
            counterOldValue,
            globalPoolPointer,
            acquisitionResult);

    private static GameSocketCompletionErrorStateCapture Parent(
        uint esi,
        uint oldState,
        uint? freshField,
        bool? queueReturnedNormally = null) =>
        new(true, esi, oldState, freshField, queueReturnedNormally);
}

using Firefall.Reconstruction.Startup;

internal static class GameSocketCompletionErrorQueueAttemptLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("The lifecycle starts at the acquisition-to-tag boundary and ends before publication",
            GameSocketCompletionErrorQueueAttemptLifecycleProjection.FirstTagCaptureRawFileOffset
                == 0x12CE364
            && GameSocketCompletionErrorQueueAttemptLifecycleProjection.FirstTailCaptureRawFileOffset
                == 0x12CE367
            && GameSocketCompletionErrorQueueAttemptLifecycleProjection.AttemptStopBeforePublicationRawFileOffset
                == 0x12CE3C3
            && GameSocketCompletionErrorQueueAttemptLifecycleProjection.QueueReceiverOffsetFromParentEsi
                == 0x260);

        GameSocketCompletionErrorQueueAttemptLifecyclePlan success = Project(
            Acquisition(esi: 0x1000, fresh: 0xDEADBEEF, result: 0x1260),
            tag: 0xAABBCCDD,
            tail: 0x12345678,
            eax: 0x12345678,
            edx: 0xAABBCCDD);
        QueueAppendAttemptPlan genericSuccess = success.ChildAttemptPlan!.Value;
        check("Only a clean nonzero acquisition supplies receiver and new-node values to one generic CAS attempt",
            success.CleanNonzeroAcquisitionAdmitted
            && success.DerivedQueueReceiverPointer == 0x1260
            && success.CapturedNewNode == 0x1260
            && genericSuccess.Captures.ReceiverAddress == 0x1260
            && genericSuccess.Captures.CapturedNewNode == 0x1260
            && unchecked(success.ParentPlan.ParentCapture.InheritedEntryEsi!.Value
                + (uint)GameSocketCompletionErrorQueueAttemptLifecycleProjection.QueueReceiverOffsetFromParentEsi)
                == success.DerivedQueueReceiverPointer
            && genericSuccess.ReceiverPairPointer == 0x1268
            && genericSuccess.ExpectedPair == new QueueAppendAttemptPair(0x12345678, 0xAABBCCDD)
            && genericSuccess.ReplacementPair == new QueueAppendAttemptPair(0x1260, 0xAABBCCDE)
            && success.ParentPlan.ParentPlan.Writes[1].Value == 0xDEADBEEF
            && success.ParentPlan.DerivedQueueReceiverPointer == 0x1260
            && success.ParentPlan.ChildAcquisitionPlan?.AcquisitionResult == 0x1260
            && genericSuccess.Actions.Count(action =>
                action.Kind == QueueAppendAttemptActionKind.AttemptLockedCompareExchange) == 1
            && success.QueueToken == 2
            && success.Outcome == GameSocketCompletionErrorQueueAttemptLifecycleOutcome.AttemptProjected
            && success.Disposition
                == GameSocketCompletionErrorQueueAttemptLifecycleDisposition.BoundedInGenericAttemptProjection
            && success.LastReachedRawFileOffset == 0x12CE3C3
            && success.StopBeforeRawFileOffset == 0x12CE3C3
            && success.QueuePublicationReached == false
            && success.QueuePublicationObserved == false
            && success.NativeInstructionsExecuted == false
            && success.IsCleanAttemptProjection);

        check("The parent trace is an immutable ordered prefix and child actions preserve capture/CAS/compare order",
            success.OrderedTrace.Take(success.ParentPlan.OrderedTrace.Length)
                .Select(step => step.ParentStep)
                .SequenceEqual(success.ParentPlan.OrderedTrace.Select(step =>
                    (GameSocketCompletionErrorQueueAcquisitionTraceStep?)step))
            && success.OrderedTrace.Take(success.ParentPlan.OrderedTrace.Length)
                .All(step => step.AttemptAction is null && step.LifecycleAction is null)
            && success.OrderedTrace.Skip(success.ParentPlan.OrderedTrace.Length)
                .Take(2).Select(step => step.LifecycleAction?.Kind)
                .SequenceEqual(new GameSocketCompletionErrorQueueAttemptLifecycleActionKind?[]
                {
                    GameSocketCompletionErrorQueueAttemptLifecycleActionKind.ReadReceiverTag,
                    GameSocketCompletionErrorQueueAttemptLifecycleActionKind.ReadReceiverTail
                })
            && success.OrderedTrace.Skip(success.ParentPlan.OrderedTrace.Length + 2)
                .Select(step => step.AttemptAction?.Kind)
                .SequenceEqual(genericSuccess.Actions.Skip(2)
                    .Select(action => (QueueAppendAttemptActionKind?)action.Kind))
            && success.OrderedTrace[success.ParentPlan.OrderedTrace.Length].LifecycleAction is
                {
                    Kind: GameSocketCompletionErrorQueueAttemptLifecycleActionKind.ReadReceiverTag,
                    InstructionRawFileOffset: 0x12CE364,
                    Address: 0x126C,
                    Value: 0xAABBCCDD,
                    Attempted: true,
                    Captured: true,
                    Committed: false
                }
            && success.OrderedTrace[success.ParentPlan.OrderedTrace.Length + 1].LifecycleAction is
                {
                    Kind: GameSocketCompletionErrorQueueAttemptLifecycleActionKind.ReadReceiverTail,
                    InstructionRawFileOffset: 0x12CE367,
                    Address: 0x1268,
                    Value: 0x12345678,
                    Attempted: true,
                    Captured: true,
                    Committed: false
                }
            && genericSuccess.Actions[0].Kind == QueueAppendAttemptActionKind.CaptureReceiverTag
            && genericSuccess.Actions[1].Kind == QueueAppendAttemptActionKind.CaptureReceiverTail
            && genericSuccess.Actions[^1].Kind == QueueAppendAttemptActionKind.StopAtSuccessBoundary);

        GameSocketCompletionErrorQueueAttemptLifecyclePlan eaxRetry = Project(
            Acquisition(result: 0x9000),
            tag: 7,
            tail: 3,
            eax: 4,
            edx: null);
        GameSocketCompletionErrorQueueAttemptLifecyclePlan edxRetry = Project(
            Acquisition(result: 0x9000),
            tag: 5,
            tail: 3,
            eax: 3,
            edx: 6);
        check("Returned EAX is compared before EDX and either mismatch stops at its retry edge",
            eaxRetry.ChildAttemptPlan?.Outcome == QueueAppendAttemptOutcome.RetryBoundary
            && eaxRetry.ChildAttemptPlan.Value.Actions[^1]
                == new QueueAppendAttemptAction(
                    QueueAppendAttemptActionKind.StopAtRetryBoundary,
                    0x12CE3BB,
                    BranchTargetRawFileOffset: 0x12CE364)
            && !eaxRetry.ChildAttemptPlan.Value.Actions.Any(action =>
                action.Kind == QueueAppendAttemptActionKind.CompareReturnedEdx)
            && eaxRetry.CaptureStates.CompareExchangeReturnedEdx
                == GameSocketCompletionErrorQueueAttemptCaptureState.NotReached
            && eaxRetry.CaptureStates.CompareExchangeReturnedEax
                == GameSocketCompletionErrorQueueAttemptCaptureState.Captured
            && eaxRetry.LastReachedRawFileOffset == 0x12CE3BB
            && eaxRetry.StopBeforeRawFileOffset == 0x12CE364
            && eaxRetry.FlagsAtStop is
                { Carry: false, Parity: false, AuxiliaryCarry: false, Zero: false, Sign: false, Overflow: false }
            && edxRetry.ChildAttemptPlan?.Outcome == QueueAppendAttemptOutcome.RetryBoundary
            && edxRetry.ChildAttemptPlan.Value.Actions[^1]
                == new QueueAppendAttemptAction(
                    QueueAppendAttemptActionKind.StopAtRetryBoundary,
                    0x12CE3C1,
                    BranchTargetRawFileOffset: 0x12CE364)
            && edxRetry.LastReachedRawFileOffset == 0x12CE3C1
            && edxRetry.StopBeforeRawFileOffset == 0x12CE364
            && edxRetry.FlagsAtStop is
                { Carry: false, Parity: false, AuxiliaryCarry: false, Zero: false, Sign: false, Overflow: false });

        GameSocketCompletionErrorQueueAttemptLifecyclePlan nullTailMissingDiagnostic = Project(
            Acquisition(result: 0x9000),
            tag: uint.MaxValue,
            tail: 0,
            diagnosticCapture: QueueAppendAttemptCapture<bool>.Missing(),
            eax: 0x9000,
            edx: uint.MaxValue);
        GameSocketCompletionErrorQueueAttemptLifecyclePlan nullTailNoReturn = Project(
            Acquisition(result: 0x9000),
            tag: uint.MaxValue,
            tail: 0,
            diagnostic: false,
            eax: 0x9000,
            edx: uint.MaxValue);
        GameSocketCompletionErrorQueueAttemptLifecyclePlan nullTailReturned = Project(
            Acquisition(result: 0x9000),
            tag: uint.MaxValue,
            tail: 0,
            diagnostic: true,
            eax: 0,
            edx: uint.MaxValue);
        check("Null-tail diagnostics gate the CAS; missing and nonreturning calls leave supplied CAS outputs unused",
            nullTailMissingDiagnostic.ChildAttemptPlan?.Outcome
                == QueueAppendAttemptOutcome.MissingDiagnosticReturnBoundary
            && nullTailMissingDiagnostic.CaptureStates.DiagnosticReturned
                == GameSocketCompletionErrorQueueAttemptCaptureState.Missing
            && nullTailMissingDiagnostic.CaptureStates.CompareExchangeReturnedEax
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && nullTailMissingDiagnostic.CaptureStates.CompareExchangeReturnedEdx
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && !nullTailMissingDiagnostic.ChildAttemptPlan.Value.Actions.Any(action =>
                action.Kind == QueueAppendAttemptActionKind.AttemptLockedCompareExchange)
            && nullTailMissingDiagnostic.LastReachedRawFileOffset == 0x12CE381
            && nullTailMissingDiagnostic.StopBeforeRawFileOffset == 0x12CE381
            && nullTailMissingDiagnostic.FlagsAtStop is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: true, Sign: false, Overflow: false }
            && !nullTailMissingDiagnostic.ReachedBoundedProjectionBoundary
            && nullTailMissingDiagnostic.OrderedTrace[^1].LifecycleAction is
                {
                    Kind: GameSocketCompletionErrorQueueAttemptLifecycleActionKind.StopAtNullTailDiagnosticCall,
                    InstructionRawFileOffset: 0x12CE381,
                    Attempted: true
                }
            && nullTailNoReturn.ChildAttemptPlan?.Outcome
                == QueueAppendAttemptOutcome.DiagnosticDidNotReturnBoundary
            && nullTailNoReturn.CaptureStates.CompareExchangeReturnedEax
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && nullTailNoReturn.LastReachedRawFileOffset == 0x12CE381
            && nullTailNoReturn.StopBeforeRawFileOffset == 0x12CE381
            && nullTailNoReturn.FlagsAtStop is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: true, Sign: false, Overflow: false }
            && !nullTailNoReturn.ReachedBoundedProjectionBoundary
            && nullTailNoReturn.OrderedTrace[^1].LifecycleAction is
                {
                    Kind: GameSocketCompletionErrorQueueAttemptLifecycleActionKind.StopAtNullTailDiagnosticCall,
                    InstructionRawFileOffset: 0x12CE381,
                    Attempted: true
                }
            && nullTailReturned.ChildAttemptPlan?.Outcome == QueueAppendAttemptOutcome.SuccessBoundary
            && nullTailReturned.ChildAttemptPlan.Value.ExpectedPair
                == new QueueAppendAttemptPair(0, uint.MaxValue)
            && nullTailReturned.ChildAttemptPlan.Value.ReplacementPair
                == new QueueAppendAttemptPair(0x9000, 0)
            && nullTailReturned.FlagsAfterDiagnosticReturn is
                { Carry: null, Parity: null, AuxiliaryCarry: null, Zero: null, Sign: null, Overflow: null }
            && nullTailReturned.FlagsBeforeCompareExchange is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: false, Sign: false, Overflow: false }
            && nullTailReturned.FlagsAfterCompareExchange is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: true, Sign: false, Overflow: false }
            && nullTailReturned.FlagsAtStop is
                { Carry: false, Parity: true, AuxiliaryCarry: false, Zero: true, Sign: false, Overflow: false }
            && nullTailReturned.ReachedBoundedProjectionBoundary);

        GameSocketCompletionErrorQueueAttemptLifecyclePlan missingTag = Project(
            Acquisition(result: 0x9000),
            tagCapture: QueueAppendAttemptCapture<uint>.Missing(),
            tailCapture: QueueAppendAttemptCapture<uint>.Captured(9),
            diagnosticCapture: QueueAppendAttemptCapture<bool>.Captured(true),
            eaxCapture: QueueAppendAttemptCapture<uint>.Captured(9),
            edxCapture: QueueAppendAttemptCapture<uint>.Captured(1));
        GameSocketCompletionErrorQueueAttemptLifecyclePlan missingTail = Project(
            Acquisition(result: 0x9000),
            tag: 1,
            tailCapture: QueueAppendAttemptCapture<uint>.Missing(),
            diagnosticCapture: QueueAppendAttemptCapture<bool>.Captured(true),
            eaxCapture: QueueAppendAttemptCapture<uint>.Captured(9),
            edxCapture: QueueAppendAttemptCapture<uint>.Captured(1));
        check("Missing early tag or tail captures stop in order and mark every supplied later value unused",
            missingTag.Outcome
                == GameSocketCompletionErrorQueueAttemptLifecycleOutcome.MissingReceiverTagBoundary
            && missingTag.ChildAttemptPlan is null
            && missingTag.LastReachedRawFileOffset == 0x12CE364
            && missingTag.StopBeforeRawFileOffset == 0x12CE364
            && missingTag.CaptureStates.ReceiverTag
                == GameSocketCompletionErrorQueueAttemptCaptureState.Missing
            && missingTag.CaptureStates.ReceiverTail
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && missingTag.CaptureStates.DiagnosticReturned
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && missingTag.CaptureStates.CompareExchangeReturnedEdx
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && missingTag.FlagsAtStop == missingTag.ParentPlan.FlagsAtStop
            && !missingTag.ReachedBoundedProjectionBoundary
            && missingTag.OrderedTrace.Single(step =>
                step.LifecycleAction?.Kind
                    == GameSocketCompletionErrorQueueAttemptLifecycleActionKind.ReadReceiverTag)
                .LifecycleAction is
                {
                    Address: 0x126C,
                    Attempted: true,
                    Captured: false,
                    Committed: false
                }
            && missingTail.Outcome
                == GameSocketCompletionErrorQueueAttemptLifecycleOutcome.MissingReceiverTailBoundary
            && missingTail.CaptureStates.ReceiverTag
                == GameSocketCompletionErrorQueueAttemptCaptureState.Captured
            && missingTail.CaptureStates.ReceiverTail
                == GameSocketCompletionErrorQueueAttemptCaptureState.Missing
            && missingTail.CaptureStates.DiagnosticReturned
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && missingTail.CaptureStates.CompareExchangeReturnedEax
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && missingTail.LastReachedRawFileOffset == 0x12CE367
            && missingTail.StopBeforeRawFileOffset == 0x12CE367
            && missingTail.FlagsAtStop == missingTail.ParentPlan.FlagsAtStop
            && !missingTail.ReachedBoundedProjectionBoundary
            && missingTail.OrderedTrace.Skip(missingTail.ParentPlan.OrderedTrace.Length)
                .Take(2).Select(step => step.LifecycleAction?.Kind)
                .SequenceEqual(new GameSocketCompletionErrorQueueAttemptLifecycleActionKind?[]
                {
                    GameSocketCompletionErrorQueueAttemptLifecycleActionKind.ReadReceiverTag,
                    GameSocketCompletionErrorQueueAttemptLifecycleActionKind.ReadReceiverTail
                })
            && missingTail.OrderedTrace.Single(step =>
                step.LifecycleAction?.Kind
                    == GameSocketCompletionErrorQueueAttemptLifecycleActionKind.ReadReceiverTail)
                .LifecycleAction is
                {
                    Address: 0x1268,
                    Attempted: true,
                    Captured: false,
                    Committed: false
                });

        GameSocketCompletionErrorQueueAttemptLifecyclePlan parentZero = Project(
            Acquisition(result: 0),
            tag: 1,
            tail: 2,
            eax: 2,
            edx: 1);
        AcquisitionInputs missingParentResult =
            Acquisition(resultCapture: QueueAppendAcquisitionCapture<uint>.Missing());
        GameSocketCompletionErrorQueueAttemptLifecyclePlan childResultMissing = Project(
            missingParentResult,
            tag: 1,
            tail: 2,
            eax: 2,
            edx: 1);
        AcquisitionInputs dirtyParent = new(
            new GameSocketCompletionErrorStateCapture(true, 0x5000, null, null, null),
            QueueAppendAcquisitionCapture<uint>.Captured(1),
            QueueAppendAcquisitionCapture<uint>.Captured(2),
            QueueAppendAcquisitionCapture<uint>.Captured(3));
        GameSocketCompletionErrorQueueAttemptLifecyclePlan parentDirty = Project(
            dirtyParent,
            tag: 1,
            tail: 2,
            eax: 2,
            edx: 1);
        check("Zero or missing acquisition and dirty parent boundaries never enter the append attempt",
            parentZero.Outcome
                == GameSocketCompletionErrorQueueAttemptLifecycleOutcome.NullAcquisitionResultBoundary
            && parentZero.ChildAttemptPlan is null
            && parentZero.CapturedNewNode is null
            && parentZero.CaptureStates.ReceiverTag
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && parentZero.LastReachedRawFileOffset == parentZero.ParentPlan.LastReachedRawFileOffset
            && parentZero.FlagsAtStop == parentZero.ParentPlan.FlagsAtStop
            && childResultMissing.Outcome
                == GameSocketCompletionErrorQueueAttemptLifecycleOutcome.ParentAcquisitionBoundary
            && childResultMissing.ChildAttemptPlan is null
            && childResultMissing.ParentPlan.ChildAcquisitionPlan?.Outcome
                == QueueAppendAcquisitionOutcome.MissingAcquisitionResult
            && childResultMissing.ParentPlan.ChildCaptureStates.AcquisitionResult
                == GameSocketCompletionErrorQueueAcquisitionCaptureState.Missing
            && childResultMissing.ParentPlan.ParentPlan.MemoryEffects.Any(effect =>
                effect.Kind == GameSocketCompletionErrorStateMemoryEffectKind.FreshFieldRead
                && effect.Attempted && effect.Captured && effect.Committed == false)
            && parentZero.ReachedBoundedProjectionBoundary
                == parentZero.ParentPlan.ReachedBoundedProjectionBoundary
            && childResultMissing.ReachedBoundedProjectionBoundary
                == childResultMissing.ParentPlan.ReachedBoundedProjectionBoundary
            && parentDirty.ChildAttemptPlan is null
            && parentDirty.CaptureStates.ReceiverTag
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && parentDirty.LastReachedRawFileOffset == parentDirty.ParentPlan.LastReachedRawFileOffset
            && parentDirty.StopBeforeRawFileOffset == parentDirty.ParentPlan.StopBeforeRawFileOffset
            && parentDirty.FlagsAtStop == parentDirty.ParentPlan.FlagsAtStop
            && parentDirty.ReachedBoundedProjectionBoundary
                == parentDirty.ParentPlan.ReachedBoundedProjectionBoundary);

        GameSocketCompletionErrorQueueAttemptLifecyclePlan diagnosticThenMissingEax = Project(
            Acquisition(result: 0x9000),
            tag: 5,
            tail: 0,
            diagnostic: true,
            eaxCapture: QueueAppendAttemptCapture<uint>.Missing(),
            edx: 6);
        GameSocketCompletionErrorQueueAttemptLifecyclePlan missingEdx = Project(
            Acquisition(result: 0x9000),
            tag: 5,
            tail: 3,
            eax: 3,
            edxCapture: QueueAppendAttemptCapture<uint>.Missing());
        GameSocketCompletionErrorQueueAttemptLifecyclePlan eaxMismatchWithSuppliedEdx = Project(
            Acquisition(result: 0x9000),
            tag: 5,
            tail: 3,
            eax: 4,
            edx: 6);
        GameSocketCompletionErrorQueueAttemptLifecyclePlan retryWithSuppliedDiagnostic = Project(
            Acquisition(result: 0x9000),
            tag: 5,
            tail: 3,
            eax: 4,
            diagnosticCapture: QueueAppendAttemptCapture<bool>.Captured(true));
        check("Later CAS-result captures remain unused after a missing EAX result or the first EAX mismatch",
            diagnosticThenMissingEax.ChildAttemptPlan?.Outcome
                == QueueAppendAttemptOutcome.MissingCompareExchangeEaxBoundary
            && diagnosticThenMissingEax.CaptureStates.CompareExchangeReturnedEax
                == GameSocketCompletionErrorQueueAttemptCaptureState.Missing
            && diagnosticThenMissingEax.CaptureStates.CompareExchangeReturnedEdx
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && diagnosticThenMissingEax.FlagsBeforeCompareExchange is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: false, Sign: false, Overflow: false }
            && diagnosticThenMissingEax.FlagsAfterCompareExchange is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: null, Sign: false, Overflow: false }
            && diagnosticThenMissingEax.FlagsAtStop == diagnosticThenMissingEax.FlagsAfterCompareExchange
            && !diagnosticThenMissingEax.ReachedBoundedProjectionBoundary
            && missingEdx.ChildAttemptPlan?.Outcome
                == QueueAppendAttemptOutcome.MissingCompareExchangeEdxBoundary
            && missingEdx.CaptureStates.CompareExchangeReturnedEdx
                == GameSocketCompletionErrorQueueAttemptCaptureState.Missing
            && !missingEdx.ReachedBoundedProjectionBoundary
            && eaxMismatchWithSuppliedEdx.ChildAttemptPlan?.Outcome
                == QueueAppendAttemptOutcome.RetryBoundary
            && eaxMismatchWithSuppliedEdx.CaptureStates.CompareExchangeReturnedEdx
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && eaxMismatchWithSuppliedEdx.ChildAttemptPlan.Value.Actions[^1].InstructionRawFileOffset
                == 0x12CE3BB
            && eaxMismatchWithSuppliedEdx.ReachedBoundedProjectionBoundary
            && !eaxMismatchWithSuppliedEdx.IsCleanAttemptProjection
            && retryWithSuppliedDiagnostic.ChildAttemptPlan?.Outcome
                == QueueAppendAttemptOutcome.RetryBoundary
            && retryWithSuppliedDiagnostic.CaptureStates.DiagnosticReturned
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && retryWithSuppliedDiagnostic.ReachedBoundedProjectionBoundary
            && !retryWithSuppliedDiagnostic.IsCleanAttemptProjection);

        GameSocketCompletionErrorQueueAttemptLifecyclePlan parentWithMissingFresh = Project(
            Acquisition(esi: 0x6000, oldState: 0, fresh: null, queueReturnedNormally: true),
            tag: 1,
            tail: 2,
            eax: 2,
            edx: 1);
        GameSocketCompletionErrorQueueAttemptLifecyclePlan parentReturned = Project(
            Acquisition(esi: 0x7000, oldState: 0, fresh: 0, queueReturnedNormally: true),
            tag: 1,
            tail: 2,
            eax: 2,
            edx: 1);
        check("Missing fresh reads and already-returned parent calls preserve their original bounds and flags",
            parentWithMissingFresh.ParentPlan.ParentPlan.Outcome
                == GameSocketCompletionErrorStateOutcome.MissingFreshField108
            && parentWithMissingFresh.ParentPlan.ParentPlan.MemoryEffects.Any(effect =>
                effect.Kind == GameSocketCompletionErrorStateMemoryEffectKind.FreshFieldRead
                && effect.Attempted && !effect.Captured && effect.Committed == false)
            && parentWithMissingFresh.ChildAttemptPlan is null
            && parentWithMissingFresh.LastReachedRawFileOffset
                == parentWithMissingFresh.ParentPlan.LastReachedRawFileOffset
            && parentWithMissingFresh.FlagsAtStop == parentWithMissingFresh.ParentPlan.FlagsAtStop
            && parentReturned.ParentPlan.ParentPlan.Outcome
                == GameSocketCompletionErrorStateOutcome.BoundedAtQueueAppendReturn
            && parentReturned.ChildAttemptPlan is null
            && parentReturned.LastReachedRawFileOffset == parentReturned.ParentPlan.LastReachedRawFileOffset
            && parentReturned.FlagsAtStop == parentReturned.ParentPlan.FlagsAtStop
            && parentReturned.ReachedBoundedProjectionBoundary
                == parentReturned.ParentPlan.ReachedBoundedProjectionBoundary);

        GameSocketCompletionErrorQueueAttemptLifecyclePlan tailTestAndCasFlags = Project(
            Acquisition(result: 0x9000),
            tag: 0,
            tail: 0x80000000,
            eax: 0x80000000,
            edx: 0);
        check("Tail TEST flags lead into OR-derived pre-CAS flags, CMPXCHG8B changes only ZF, and CMP flags are full",
            tailTestAndCasFlags.FlagsAfterTailTest is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: false, Sign: true, Overflow: false }
            && tailTestAndCasFlags.FlagsBeforeCompareExchange is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: false, Sign: false, Overflow: false }
            && tailTestAndCasFlags.FlagsAfterCompareExchange is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: true, Sign: false, Overflow: false }
            && tailTestAndCasFlags.FlagsAtStop is
                { Carry: false, Parity: true, AuxiliaryCarry: false, Zero: true, Sign: false, Overflow: false }
            && tailTestAndCasFlags.ChildAttemptPlan?.Outcome == QueueAppendAttemptOutcome.SuccessBoundary
            && tailTestAndCasFlags.ReachedBoundedProjectionBoundary
            && tailTestAndCasFlags.IsCleanAttemptProjection);

        bool rejectsUnreachedReachedCapture = ThrowsArgumentException(() => Project(
            Acquisition(result: 0x9000),
            tagCapture: QueueAppendAttemptCapture<uint>.Unreached(),
            tail: 1));
        check("Reached receiver captures cannot be marked unreached", rejectsUnreachedReachedCapture);
    }

    private readonly record struct AcquisitionInputs(
        GameSocketCompletionErrorStateCapture ParentCapture,
        QueueAppendAcquisitionCapture<uint> CounterOldValue,
        QueueAppendAcquisitionCapture<uint> GlobalPoolPointer,
        QueueAppendAcquisitionCapture<uint> AcquisitionResult);

    private static GameSocketCompletionErrorQueueAttemptLifecyclePlan Project(
        AcquisitionInputs parent,
        uint? tag = 7,
        uint? tail = 3,
        bool? diagnostic = null,
        uint? eax = 3,
        uint? edx = 7,
        QueueAppendAttemptCapture<uint>? tagCapture = null,
        QueueAppendAttemptCapture<uint>? tailCapture = null,
        QueueAppendAttemptCapture<bool>? diagnosticCapture = null,
        QueueAppendAttemptCapture<uint>? eaxCapture = null,
        QueueAppendAttemptCapture<uint>? edxCapture = null) =>
        GameSocketCompletionErrorQueueAttemptLifecycleProjection.Project(
            parent.ParentCapture,
            parent.CounterOldValue,
            parent.GlobalPoolPointer,
            parent.AcquisitionResult,
            tagCapture ?? Captured(tag),
            tailCapture ?? Captured(tail),
            diagnosticCapture ?? Captured(diagnostic),
            eaxCapture ?? Captured(eax),
            edxCapture ?? Captured(edx));

    private static AcquisitionInputs Acquisition(
        uint esi = 0x1000,
        uint oldState = 0,
        uint? fresh = 0xDEADBEEF,
        uint result = 0x9000,
        QueueAppendAcquisitionCapture<uint>? resultCapture = null,
        bool? queueReturnedNormally = null) =>
        new(
            new GameSocketCompletionErrorStateCapture(
                true, esi, oldState, fresh, queueReturnedNormally),
            QueueAppendAcquisitionCapture<uint>.Captured(4),
            QueueAppendAcquisitionCapture<uint>.Captured(0x1E003BC),
            resultCapture ?? QueueAppendAcquisitionCapture<uint>.Captured(result));

    private static QueueAppendAttemptCapture<T> Captured<T>(T? value) where T : struct =>
        value.HasValue
            ? QueueAppendAttemptCapture<T>.Captured(value.Value)
            : QueueAppendAttemptCapture<T>.Unreached();

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}

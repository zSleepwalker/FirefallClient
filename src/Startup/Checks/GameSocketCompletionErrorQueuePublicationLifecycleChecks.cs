using Firefall.Reconstruction.Startup;

internal static class GameSocketCompletionErrorQueuePublicationLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("Publication composes the existing success boundary and leaves the outer continuation distinct",
            GameSocketCompletionErrorQueuePublicationLifecycleProjection.PublicationStartRawFileOffset
                == 0x12CE3C3
            && GameSocketCompletionErrorQueuePublicationLifecycleProjection.PublicationReturnRawFileOffset
                == 0x12CE3F4
            && GameSocketCompletionErrorQueuePublicationLifecycleProjection.PostDiagnosticStackCleanupRawFileOffset
                == 0x12CE3DF
            && GameSocketCompletionErrorQueuePublicationLifecycleProjection.FreshPayloadArgumentRawFileOffset
                == 0x12CE3E2
            && GameSocketCompletionErrorQueuePublicationLifecycleProjection.FreshNewNodeReloadRawFileOffset
                == 0x12CE3E8
            && GameSocketCompletionErrorQueuePublicationLifecycleProjection.OuterQueueAppendContinuationRawFileOffset
                == 0xBD2E19);

        GameSocketCompletionErrorQueuePublicationLifecyclePlan success = Project(
            acquisitionResult: 0x1260,
            tag: 0xAABBCCDD,
            tail: 0,
            attemptDiagnostic: true,
            eax: 0,
            edx: 0xAABBCCDD,
            oldTailNext: 0,
            payloadArgument: 2,
            newNodePointer: 0x1260);
        QueueAppendPublicationPlan publication = success.ChildPublicationPlan!.Value;
        check("A clean CAS success admits publication using fresh stack reloads and the retained token metadata",
            success.AttemptPlan.ChildAttemptPlan?.Outcome == QueueAppendAttemptOutcome.SuccessBoundary
            && success.PublicationProjectionEntered
            && success.QueueToken == 2
            && success.DerivedQueueReceiverPointer == 0x1260
            && success.CapturedOldTailPointer == 0
            && success.CapturedNewNode == 0x1260
            && publication.Capture.CapturedOldTailPointer == 0
            && publication.Capture.CapturedPayloadArgument == 2
            && publication.Capture.CapturedNewNodePointer == 0x1260
            && publication.Outcome == QueueAppendPublicationOutcome.ReturnedCapturedNewNode
            && publication.ReturnValue == 0x1260
            && success.IsCleanPublicationProjection
            && success.OuterQueueAppendReturnedNormally is null
            && !success.OuterQueueAppendReturnObserved
            && !success.NativeInstructionsExecuted
            && !success.QueuePublicationObserved);

        check("Parent and attempt traces remain an ordered prefix of fresh child memory observations",
            success.OrderedTrace.Take(success.AttemptPlan.OrderedTrace.Length)
                .Select(step => step.AttemptStep)
                .SequenceEqual(success.AttemptPlan.OrderedTrace.Select(step =>
                    (GameSocketCompletionErrorQueueAttemptLifecycleTraceStep?)step))
            && success.OrderedTrace[success.AttemptPlan.OrderedTrace.Length]
                is
                {
                    PublicationAction: { Kind: QueueAppendPublicationActionKind.CaptureOldTailNextPointer },
                    PublicationMemoryEffect:
                    {
                        Kind: GameSocketCompletionErrorQueuePublicationMemoryEffectKind.Read,
                        Address: 0,
                        Value: 0,
                        Attempted: true,
                        Captured: true,
                        ProjectedCommitted: false
                    }
                }
            && publication.Actions.Any(action =>
                action.Kind == QueueAppendPublicationActionKind.SkipDiagnosticOnNullNextPointer)
            && !publication.Actions.Any(action =>
                action.Kind == QueueAppendPublicationActionKind.CallDiagnosticThroughIndirectCell)
            && success.OrderedTrace.Any(step =>
                step.PublicationMemoryEffect is
                {
                    Kind: GameSocketCompletionErrorQueuePublicationMemoryEffectKind.Write,
                    Address: 4,
                    Value: 2,
                    Attempted: true,
                    Captured: true,
                    ProjectedCommitted: true
                })
            && success.OrderedTrace.Any(step =>
                step.PublicationMemoryEffect is
                {
                    Kind: GameSocketCompletionErrorQueuePublicationMemoryEffectKind.Write,
                    Address: 0,
                    Value: 0x1260,
                    ProjectedCommitted: true
                })
            && success.LastReachedRawFileOffset == 0x12CE3F4
            && success.StopBeforeRawFileOffset == 0x12CE3F4
            && success.FlagsAtStop is
                { Carry: false, Parity: true, AuxiliaryCarry: false, Zero: true, Sign: false, Overflow: false });

        GameSocketCompletionErrorQueuePublicationLifecyclePlan returnedDiagnostic = Project(
            acquisitionResult: 0x9000,
            tag: 7,
            tail: 3,
            eax: 3,
            edx: 7,
            oldTailNext: 0x44,
            publicationDiagnosticCapture: QueueAppendAttemptCapture<bool>.Captured(true),
            postDiagnosticEsp: 0xFFFFFFF8,
            payloadArgument: 0xCAFEBABE,
            newNodePointer: 0xABCDEF01);
        check("The diagnostic return's fresh ESP drives ADD flags and fresh payload/node reloads may differ from inherited values",
            returnedDiagnostic.ChildPublicationPlan?.Outcome
                == QueueAppendPublicationOutcome.ReturnedCapturedNewNode
            && returnedDiagnostic.QueueToken == 2
            && returnedDiagnostic.FreshPayloadArgument == 0xCAFEBABE
            && returnedDiagnostic.FreshReloadedNewNode == 0xABCDEF01
            && returnedDiagnostic.CapturedNewNode == 0x9000
            && returnedDiagnostic.ChildPublicationPlan.Value.ReturnValue == 0xABCDEF01
            && returnedDiagnostic.ChildPublicationPlan.Value.Actions.Any(action =>
                action.Kind == QueueAppendPublicationActionKind.AttemptPayloadStoreToOldTail
                && action.Pointer == 7
                && action.CapturedValue == 0xCAFEBABE)
            && Array.FindIndex(
                returnedDiagnostic.ChildPublicationPlan.Value.Actions.ToArray(),
                action => action.Kind == QueueAppendPublicationActionKind.AttemptPayloadStoreToOldTail)
                < Array.FindIndex(
                    returnedDiagnostic.ChildPublicationPlan.Value.Actions.ToArray(),
                    action => action.Kind == QueueAppendPublicationActionKind.CaptureNewNodePointer)
            && returnedDiagnostic.FlagsAtStop is
                { Carry: true, Parity: false, AuxiliaryCarry: false, Zero: false, Sign: false, Overflow: false });

        GameSocketCompletionErrorQueuePublicationLifecyclePlan signedStackCleanup = Project(
            acquisitionResult: 0x9000,
            oldTailNext: 0x44,
            publicationDiagnosticCapture: QueueAppendAttemptCapture<bool>.Captured(true),
            postDiagnosticEsp: 0x7FFFFFF0);
        check("The cleanup ADD computes signed overflow, sign, and parity flags from its fresh ESP operand",
            signedStackCleanup.FlagsAtStop is
                { Carry: false, Parity: true, AuxiliaryCarry: false, Zero: false, Sign: true, Overflow: true });

        GameSocketCompletionErrorQueuePublicationLifecyclePlan retry = Project(
            acquisitionResult: 0x9000,
            tag: 5,
            tail: 3,
            eax: 4,
            edxCapture: QueueAppendAttemptCapture<uint>.Unreached(),
            oldTailNext: 0x44,
            publicationDiagnosticCapture: QueueAppendAttemptCapture<bool>.Captured(true));
        check("A clean retry does not admit publication and retains its retry target",
            retry.AttemptPlan.ChildAttemptPlan?.Outcome == QueueAppendAttemptOutcome.RetryBoundary
            && retry.AttemptPlan.IsCleanAttemptProjection
            && !retry.PublicationProjectionEntered
            && retry.Outcome == GameSocketCompletionErrorQueuePublicationLifecycleOutcome.RetryBoundary
            && retry.LastReachedRawFileOffset == 0x12CE3BB
            && retry.StopBeforeRawFileOffset == 0x12CE364
            && retry.PublicationCaptureStates.OldTailNextPointer
                == GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused
            && retry.PublicationCaptureStates.DiagnosticReturned
                == GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused);

        GameSocketCompletionErrorQueuePublicationLifecyclePlan dirtyRetry = Project(
            acquisitionResult: 0x9000,
            tag: 5,
            tail: 3,
            attemptDiagnostic: true,
            eax: 4,
            edx: 6,
            oldTailNext: 0x44);
        check("Unused EDX and attempt-diagnostic captures stay dirty without erasing the retry boundary",
            dirtyRetry.AttemptPlan.CaptureStates.CompareExchangeReturnedEdx
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && dirtyRetry.AttemptPlan.CaptureStates.DiagnosticReturned
                == GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            && !dirtyRetry.AttemptPlan.IsCleanAttemptProjection
            && !dirtyRetry.PublicationProjectionEntered
            && dirtyRetry.StopBeforeRawFileOffset == 0x12CE364
            && dirtyRetry.PublicationCaptureStates.OldTailNextPointer
                == GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused);

        GameSocketCompletionErrorQueuePublicationLifecyclePlan missingNext = Project(
            acquisitionResult: 0x9000,
            tag: 1,
            tail: 0x9000,
            eax: 0x9000,
            edx: 1,
            nextCapture: QueueAppendAcquisitionCapture<uint>.Missing());
        GameSocketCompletionErrorQueuePublicationLifecyclePlan missingDiagnostic = Project(
            acquisitionResult: 0x9000,
            tag: 1,
            tail: 0x9000,
            eax: 0x9000,
            edx: 1,
            oldTailNext: 0x1234,
            publicationDiagnosticCapture: QueueAppendAttemptCapture<bool>.Missing());
        check("Missing next and diagnostic captures retain their exact boundary and flag behavior",
            missingNext.Outcome
                == GameSocketCompletionErrorQueuePublicationLifecycleOutcome.AwaitingNextPointerCapture
            && missingNext.ChildPublicationPlan?.Outcome
                == QueueAppendPublicationOutcome.AwaitingNextPointerCapture
            && missingNext.PublicationCaptureStates.OldTailNextPointer
                == GameSocketCompletionErrorQueuePublicationCaptureState.Missing
            && missingNext.FlagsAtStop == missingNext.AttemptPlan.FlagsAtStop
            && missingNext.OrderedTrace[^1].PublicationMemoryEffect is
                {
                    Kind: GameSocketCompletionErrorQueuePublicationMemoryEffectKind.Read,
                    Attempted: false,
                    Captured: false,
                    ProjectedCommitted: false
                }
            && missingDiagnostic.Outcome
                == GameSocketCompletionErrorQueuePublicationLifecycleOutcome.AwaitingDiagnosticReturnCapture
            && missingDiagnostic.ChildPublicationPlan?.Outcome
                == QueueAppendPublicationOutcome.AwaitingDiagnosticReturnCapture
            && missingDiagnostic.PublicationCaptureStates.DiagnosticReturned
                == GameSocketCompletionErrorQueuePublicationCaptureState.Missing
            && missingDiagnostic.FlagsAtStop is
                { Carry: false, Parity: false, AuxiliaryCarry: false, Zero: false, Sign: false, Overflow: false });

        GameSocketCompletionErrorQueuePublicationLifecyclePlan diagnosticDidNotReturn = Project(
            acquisitionResult: 0x9000,
            tag: 1,
            tail: 0x9000,
            eax: 0x9000,
            edx: 1,
            oldTailNext: 0x1234,
            publicationDiagnosticCapture: QueueAppendAttemptCapture<bool>.Captured(false));
        check("A nonreturning opaque diagnostic preserves the CMP flags and stops at its call boundary",
            diagnosticDidNotReturn.Outcome
                == GameSocketCompletionErrorQueuePublicationLifecycleOutcome.DiagnosticContinuationBoundary
            && diagnosticDidNotReturn.FlagsAtStop is
                { Carry: false, Parity: false, AuxiliaryCarry: false, Zero: false, Sign: false, Overflow: false }
            && diagnosticDidNotReturn.LastReachedRawFileOffset == 0x12CE3D9);

        GameSocketCompletionErrorQueuePublicationLifecyclePlan missingEsp = Project(
            acquisitionResult: 0x9000,
            tag: 1,
            tail: 0x9000,
            eax: 0x9000,
            edx: 1,
            oldTailNext: 0x44,
            publicationDiagnosticCapture: QueueAppendAttemptCapture<bool>.Captured(true),
            postDiagnosticEspCapture: QueueAppendAcquisitionCapture<uint>.Missing());
        check("A normal opaque return stops at ADD ESP when its fresh stack-pointer value is missing",
            missingEsp.Outcome
                == GameSocketCompletionErrorQueuePublicationLifecycleOutcome.AwaitingPostDiagnosticEspCapture
            && missingEsp.ChildPublicationPlan?.Outcome
                == QueueAppendPublicationOutcome.AwaitingPayloadCapture
            && missingEsp.PublicationCaptureStates.PostDiagnosticEspBeforeCleanup
                == GameSocketCompletionErrorQueuePublicationCaptureState.Missing
            && missingEsp.PublicationCaptureStates.FreshPayloadArgument
                == GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused
            && missingEsp.PublicationCaptureStates.FreshReloadedNewNode
                == GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused
            && missingEsp.LastReachedRawFileOffset == 0x12CE3D9
            && missingEsp.StopBeforeRawFileOffset == 0x12CE3DF
            && missingEsp.OrderedTrace[^1].LifecycleAction is
                {
                    Kind: GameSocketCompletionErrorQueuePublicationLifecycleActionKind
                        .StopAtMissingPostDiagnosticEspCapture,
                    InstructionRawFileOffset: 0x12CE3DF
                }
            && missingEsp.FlagsAtStop is
                { Carry: null, Parity: null, AuxiliaryCarry: null, Zero: null, Sign: null, Overflow: null }
            && !missingEsp.ReachedBoundedProjectionBoundary);

        GameSocketCompletionErrorQueuePublicationLifecyclePlan missingPayload = Project(
            acquisitionResult: 0x9000,
            tag: 1,
            tail: 0x9000,
            eax: 0x9000,
            edx: 1,
            oldTailNext: 0,
            payloadCapture: QueueAppendAcquisitionCapture<uint>.Missing());
        GameSocketCompletionErrorQueuePublicationLifecyclePlan missingNode = Project(
            acquisitionResult: 0x9000,
            tag: 1,
            tail: 0x9000,
            eax: 0x9000,
            edx: 1,
            oldTailNext: 0,
            payloadArgument: 0x12345678,
            newNodeCapture: QueueAppendAcquisitionCapture<uint>.Missing());
        check("Fresh payload and node reloads stop separately, with only the reached payload store projected",
            missingPayload.Outcome
                == GameSocketCompletionErrorQueuePublicationLifecycleOutcome.AwaitingFreshPayloadCapture
            && missingPayload.PublicationCaptureStates.FreshPayloadArgument
                == GameSocketCompletionErrorQueuePublicationCaptureState.Missing
            && missingPayload.PublicationCaptureStates.FreshReloadedNewNode
                == GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused
            && missingPayload.StopBeforeRawFileOffset == 0x12CE3E2
            && !missingPayload.ReachedBoundedProjectionBoundary
            && missingNode.Outcome
                == GameSocketCompletionErrorQueuePublicationLifecycleOutcome.AwaitingFreshReloadedNewNodeCapture
            && missingNode.ChildPublicationPlan?.Actions.Any(action =>
                action.Kind == QueueAppendPublicationActionKind.AttemptPayloadStoreToOldTail
                && action.CapturedValue == 0x12345678) == true
            && missingNode.PublicationCaptureStates.FreshReloadedNewNode
                == GameSocketCompletionErrorQueuePublicationCaptureState.Missing
            && missingNode.StopBeforeRawFileOffset == 0x12CE3E8
            && !missingNode.ReachedBoundedProjectionBoundary);

        GameSocketCompletionErrorQueuePublicationLifecyclePlan dirtyUnreachedEsp = Project(
            acquisitionResult: 0x9000,
            tail: 0,
            attemptDiagnostic: true,
            eax: 0,
            edx: 7,
            oldTailNext: 0,
            postDiagnosticEsp: 0x1234);
        check("A supplied post-diagnostic ESP on the zero-next path dirties cleanliness but preserves the projected return bound",
            dirtyUnreachedEsp.ChildPublicationPlan?.Outcome
                == QueueAppendPublicationOutcome.ReturnedCapturedNewNode
            && dirtyUnreachedEsp.PublicationCaptureStates.PostDiagnosticEspBeforeCleanup
                == GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused
            && dirtyUnreachedEsp.ReachedBoundedProjectionBoundary
            && !dirtyUnreachedEsp.IsCleanPublicationProjection);

        GameSocketCompletionErrorQueuePublicationLifecyclePlan parentBoundary = Project(
            acquisitionResultCapture: QueueAppendAcquisitionCapture<uint>.Missing(),
            oldTailNext: 0);
        check("Parent acquisition boundaries keep child observations unused and preserve counter wrap",
            parentBoundary.AttemptPlan.Outcome
                == GameSocketCompletionErrorQueueAttemptLifecycleOutcome.ParentAcquisitionBoundary
            && !parentBoundary.PublicationProjectionEntered
            && parentBoundary.PublicationCaptureStates.OldTailNextPointer
                == GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused
            && parentBoundary.StopBeforeRawFileOffset == 0x12CE339
            && parentBoundary.AttemptPlan.ParentPlan.ChildAcquisitionPlan?.CounterNewValue == 0);

        GameSocketCompletionErrorQueuePublicationLifecyclePlan nullAcquisition = Project(
            acquisitionResult: 0);
        check("A captured zero acquisition result is not a publishable node",
            nullAcquisition.AttemptPlan.Outcome
                == GameSocketCompletionErrorQueueAttemptLifecycleOutcome.NullAcquisitionResultBoundary
            && nullAcquisition.AttemptPlan.ParentPlan.ChildAcquisitionPlan?.Outcome
                == QueueAppendAcquisitionOutcome.NullResultBranchProjected
            && !nullAcquisition.PublicationProjectionEntered
            && nullAcquisition.PublicationCaptureStates.OldTailNextPointer
                == GameSocketCompletionErrorQueuePublicationCaptureState.NotReached);

        GameSocketCompletionErrorQueuePublicationLifecyclePlan parentReturned = Project(
            parentQueueReturnedNormally: true,
            oldTailNext: 0);
        check("An observed outer return remains distinct and prevents re-admitting the parent projection",
            parentReturned.OuterQueueAppendReturnedNormally == true
            && parentReturned.OuterQueueAppendReturnObserved
            && parentReturned.AttemptPlan.ParentPlan.ParentPostCallContinuationEntered
            && !parentReturned.PublicationProjectionEntered
            && parentReturned.PublicationCaptureStates.OldTailNextPointer
                == GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused);

        GameSocketCompletionErrorQueuePublicationLifecyclePlan aliases = Project(
            acquisitionResult: 0x1260,
            tag: 3,
            tail: 0x1260,
            eax: 0x1260,
            edx: 3,
            oldTailNext: 0,
            newNodePointer: 0x1260);
        check("Queue, old-tail, and new-node aliases do not merge the fresh post-CAS link read",
            aliases.DerivedQueueReceiverPointer == 0x1260
            && aliases.CapturedOldTailPointer == 0x1260
            && aliases.CapturedNewNode == 0x1260
            && aliases.ChildPublicationPlan?.Capture.CapturedOldTailPointer == 0x1260
            && aliases.ChildPublicationPlan?.Actions.Any(action =>
                action.Kind == QueueAppendPublicationActionKind.CaptureOldTailNextPointer
                && action.Pointer == 0x1260
                && action.CapturedValue == 0) == true
            && aliases.ChildPublicationPlan?.Actions.Any(action =>
                action.Kind == QueueAppendPublicationActionKind.AttemptPayloadStoreToOldTail
                && action.Pointer == 0x1264) == true);

        bool rejectsUnreachedNextCapture = ThrowsArgumentException(() => Project(
            acquisitionResult: 0x9000,
            tag: 1,
            tail: 0x9000,
            eax: 0x9000,
            edx: 1,
            nextCapture: QueueAppendAcquisitionCapture<uint>.Unreached()));
        bool rejectsUnreachedDiagnosticCapture = ThrowsArgumentException(() => Project(
            acquisitionResult: 0x9000,
            tag: 1,
            tail: 0x9000,
            eax: 0x9000,
            edx: 1,
            oldTailNext: 0x1234,
            publicationDiagnosticCapture: QueueAppendAttemptCapture<bool>.Unreached()));
        bool rejectsUnreachedEspCapture = ThrowsArgumentException(() => Project(
            acquisitionResult: 0x9000,
            tag: 1,
            tail: 0x9000,
            eax: 0x9000,
            edx: 1,
            oldTailNext: 0x44,
            publicationDiagnosticCapture: QueueAppendAttemptCapture<bool>.Captured(true),
            postDiagnosticEspCapture: QueueAppendAcquisitionCapture<uint>.Unreached()));
        check("Reached child observations cannot be supplied as unreached",
            rejectsUnreachedNextCapture
            && rejectsUnreachedDiagnosticCapture
            && rejectsUnreachedEspCapture);
    }

    private static GameSocketCompletionErrorQueuePublicationLifecyclePlan Project(
        uint esi = 0x1000,
        uint oldState = 0,
        uint? fresh = 0xDEADBEEF,
        uint acquisitionResult = 0x9000,
        QueueAppendAcquisitionCapture<uint>? acquisitionResultCapture = null,
        uint tag = 7,
        uint tail = 3,
        bool? attemptDiagnostic = null,
        uint eax = 3,
        uint edx = 7,
        QueueAppendAttemptCapture<uint>? edxCapture = null,
        QueueAppendAcquisitionCapture<uint>? nextCapture = null,
        uint? oldTailNext = null,
        QueueAppendAttemptCapture<bool>? publicationDiagnosticCapture = null,
        uint? postDiagnosticEsp = null,
        QueueAppendAcquisitionCapture<uint>? postDiagnosticEspCapture = null,
        uint? payloadArgument = 2,
        QueueAppendAcquisitionCapture<uint>? payloadCapture = null,
        uint newNodePointer = 0x9000,
        QueueAppendAcquisitionCapture<uint>? newNodeCapture = null,
        bool? parentQueueReturnedNormally = null)
    {
        QueueAppendAcquisitionCapture<uint> effectiveNextCapture =
            nextCapture ?? CapturedAcquisition(oldTailNext);
        QueueAppendAttemptCapture<bool> effectivePublicationDiagnostic =
            publicationDiagnosticCapture
            ?? (effectiveNextCapture.Kind == QueueAppendAcquisitionCaptureKind.Captured
                && effectiveNextCapture.Value != 0
                    ? QueueAppendAttemptCapture<bool>.Missing()
                    : QueueAppendAttemptCapture<bool>.Unreached());

        return GameSocketCompletionErrorQueuePublicationLifecycleProjection.Project(
            new GameSocketCompletionErrorQueuePublicationLifecycleCaptures(
                new(
                    PostLogReleaseContinuationValidated: true,
                    InheritedEntryEsi: esi,
                    AtomicObservedDword: oldState,
                    FreshField108: fresh,
                    QueueAppendReturnedNormally: parentQueueReturnedNormally),
                QueueAppendAcquisitionCapture<uint>.Captured(uint.MaxValue),
                QueueAppendAcquisitionCapture<uint>.Captured(0x1E003BC),
                acquisitionResultCapture
                    ?? QueueAppendAcquisitionCapture<uint>.Captured(acquisitionResult),
                QueueAppendAttemptCapture<uint>.Captured(tag),
                QueueAppendAttemptCapture<uint>.Captured(tail),
                Captured(attemptDiagnostic),
                QueueAppendAttemptCapture<uint>.Captured(eax),
                edxCapture ?? QueueAppendAttemptCapture<uint>.Captured(edx),
                new(
                    effectiveNextCapture,
                    effectivePublicationDiagnostic,
                    postDiagnosticEspCapture ?? CapturedAcquisition(postDiagnosticEsp),
                    payloadCapture ?? CapturedAcquisition(payloadArgument),
                    newNodeCapture ?? QueueAppendAcquisitionCapture<uint>.Captured(newNodePointer))));
    }

    private static QueueAppendAcquisitionCapture<uint> CapturedAcquisition(uint? value) =>
        value.HasValue
            ? QueueAppendAcquisitionCapture<uint>.Captured(value.Value)
            : QueueAppendAcquisitionCapture<uint>.Unreached();

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

using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherFinalCallPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherFinalCallPreparationCapture Capture(
            bool? returnedNormally = true,
            uint? freshParentEax = 0x12345678,
            uint? freshPostCellEBP = 0x3010,
            uint? freshPostCellESI = 0x2F99,
            uint? freshStackScalarDWORD = 0x11223344,
            uint? freshFieldScalarDWORD = 0x55667788,
            uint? freshPostCellEBX = 0x4000,
            uint? freshCounterDWORD = 9,
            uint? freshEntryESP = 0x8000,
            uint? freshArgumentDWORD = 0xAABBCCDD)
        {
            var parentLock = new NetworkDispatcherLockGateCapture(
                ResultEAX: 1,
                EntryESIReceiver: 0x1000,
                FreshCounterValue: 1,
                EntryEBP: 0x2000,
                FreshArgumentByte: 1,
                OpaqueImportReturnedNormally: null);
            var parentCollection = new NetworkDispatcherCollectionGateCapture(
                parentLock,
                EntryESP: 0x7000,
                EntryECX: 0x4444,
                FreshThresholdDWORD: 1,
                OpaqueCallReturnedNormally: true,
                FreshPostLoggingESI: 0x2000,
                FreshEndDWORD: 0x2004,
                FreshStartDWORD: 0x2000,
                FreshReloadDWORD: 0x4000,
                FreshCollectionEntryDWORD: 1);
            var parentPreparation = new NetworkDispatcherEntryCallPreparationInputs(
                parentCollection,
                FreshEntryESP: 0x8000,
                FreshVptrDWORD: 0x5000);
            var parentCount = new NetworkDispatcherPostCallCountGateCapture(
                parentPreparation,
                VtableCallReturnedNormally: true,
                FreshPostCallESI: 0x3000,
                FreshEndDWORD: 0x3008,
                FreshPostCallEBX: 1,
                FreshStartDWORD: 0x3000);
            var parentCell = new NetworkDispatcherCellReturnAddressGateCapture(
                parentCount,
                returnedNormally,
                freshParentEax,
                freshPostCellEBP,
                freshPostCellESI);
            var parentFenced = new NetworkDispatcherFencedPublicationCapture(
                parentCell,
                freshStackScalarDWORD,
                freshFieldScalarDWORD,
                freshPostCellEBX,
                freshCounterDWORD);
            return new(
                parentFenced,
                freshEntryESP,
                freshArgumentDWORD);
        }

        check("The final push and indirect-call boundary bytes and offsets are pinned",
            Convert.ToHexString(
                Convert.FromHexString(
                    NetworkDispatcherFinalCallPreparationProjection.CapturedWindowBytes))
                == "FF75E8FF157C42AC01"
            && NetworkDispatcherFinalCallPreparationProjection.EntryRawFileOffset
                == 0x00BBDEAD
            && NetworkDispatcherFinalCallPreparationProjection.ArgumentPushRawFileOffset
                == 0x00BBDEAD
            && NetworkDispatcherFinalCallPreparationProjection.IndirectCallRawFileOffset
                == 0x00BBDEB0
            && NetworkDispatcherFinalCallPreparationProjection.ArgumentDisplacement
                == 0x18
            && NetworkDispatcherFinalCallPreparationProjection.PushWidth == 4
            && NetworkDispatcherFinalCallPreparationProjection.IndirectMemoryOperandAddress
                == 0x01AC427C
            && !NetworkDispatcherFinalCallPreparationProjection.ExecutesNativeInstructions);

        NetworkDispatcherFinalCallPreparationPlan notEqual = Project(
            Capture(freshArgumentDWORD: 0));
        check("The parent 99 route freshly reads the argument, pushes zero, and stops before the indirect call",
            notEqual.ParentPlan.EqualParentBranch == false
            && notEqual.ParentPlan.EntryRawFileOffset == 0x00BBDE99
            && notEqual.ChildGateAdmitted
            && notEqual.EntryRawFileOffset == 0x00BBDEAD
            && notEqual.EntryEBP == 0x3010
            && notEqual.ArgumentAddress == 0x2FF8
            && notEqual.ArgumentValue == 0
            && notEqual.Reads is [{
                RawFileOffset: 0x00BBDEAD,
                Address: 0x2FF8,
                Width: 4,
                Attempted: true,
                Captured: true,
                Value: 0,
                Committed: false
            }]
            && notEqual.Pushes is [{
                RawFileOffset: 0x00BBDEAD,
                Kind: NetworkDispatcherFinalCallPreparationPushKind.ArgumentDWORD,
                StackPointerBefore: 0x8000,
                StackPointerAfter: 0x7FFC,
                Value: 0,
                Attempted: true,
                Committed: true
            }]
            && notEqual.StackPointerAtStop == 0x7FFC
            && notEqual.FlagsAtStop == notEqual.ParentPlan.FlagsAtStop
            && notEqual.CallBoundaries is [{
                RawFileOffset: 0x00BBDEB0,
                IndirectMemoryOperandAddress: 0x01AC427C,
                CallSiteReached: true,
                TargetReadAttempted: false,
                TargetCaptured: false,
                CallAttempted: false
            }]
            && notEqual.StopAtRawFileOffsetExclusive == 0x00BBDEB0
            && notEqual.BoundedCompletionBoundaryReached
            && notEqual.ExactBoundedCompletion
            && notEqual.ExactCleanCompletion
            && notEqual.CaptureStates.FreshEntryESP
                == NetworkDispatcherFinalCallPreparationCaptureState.Captured
            && notEqual.CaptureStates.FreshArgumentDWORD
                == NetworkDispatcherFinalCallPreparationCaptureState.Captured
            && !notEqual.NativeInstructionsExecuted
            && !notEqual.IndirectTargetReadAttempted
            && !notEqual.IndirectCallAttempted);

        NetworkDispatcherFinalCallPreparationPlan equal = Project(
            Capture(
                freshPostCellESI: 0x2F98,
                freshStackScalarDWORD: null,
                freshArgumentDWORD: uint.MaxValue));
        check("The parent A4 route accepts the maximum fresh argument while its earlier stack input stays unused",
            equal.ParentPlan.EqualParentBranch == true
            && equal.ParentPlan.EntryRawFileOffset == 0x00BBDEA4
            && equal.ParentPlan.CaptureStates.FreshStackScalarDWORD
                == NetworkDispatcherFencedPublicationCaptureState.Unreached
            && equal.ChildGateAdmitted
            && equal.ArgumentValue == uint.MaxValue
            && equal.Reads.Single().Value == uint.MaxValue
            && equal.Pushes.Single().Value == uint.MaxValue
            && equal.Pushes.Single().StackPointerAfter == 0x7FFC
            && equal.FlagsAtStop == equal.ParentPlan.FlagsAtStop
            && equal.StopAtRawFileOffsetExclusive == 0x00BBDEB0
            && equal.ExactCleanCompletion);

        NetworkDispatcherFinalCallPreparationPlan wrappedEsp = Project(
            Capture(freshEntryESP: 0, freshArgumentDWORD: 0x76543210));
        NetworkDispatcherFinalCallPreparationPlan wrappedPush = Project(
            Capture(freshEntryESP: 2, freshArgumentDWORD: 0x76543210));
        check("A zero ESP is captured normally and PUSH wraps its four-byte decrement",
            wrappedEsp.CaptureStates.FreshEntryESP
                == NetworkDispatcherFinalCallPreparationCaptureState.Captured
            && wrappedEsp.Pushes.Single().StackPointerAfter == 0xFFFFFFFC
            && wrappedPush.Pushes.Single().StackPointerBefore == 2
            && wrappedPush.Pushes.Single().StackPointerAfter == 0xFFFFFFFE
            && wrappedPush.StackPointerAtStop == 0xFFFFFFFE
            && wrappedPush.ExactCleanCompletion);

        NetworkDispatcherFinalCallPreparationPlan missingArgument = Project(
            Capture(freshEntryESP: 0x8000, freshArgumentDWORD: null));
        NetworkDispatcherFinalCallPreparationPlan missingEsp = Project(
            Capture(freshEntryESP: null, freshArgumentDWORD: 0));
        check("Missing argument and missing ESP remain distinct stops with no invented call boundary",
            missingArgument.Outcome
                == NetworkDispatcherFinalCallPreparationOutcome.MissingFreshArgumentDWORD
            && missingArgument.Reads is [{
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false
            }]
            && missingArgument.Pushes.IsEmpty
            && missingArgument.CaptureStates.FreshArgumentDWORD
                == NetworkDispatcherFinalCallPreparationCaptureState.Missing
            && missingArgument.CaptureStates.FreshEntryESP
                == NetworkDispatcherFinalCallPreparationCaptureState.SuppliedUnused
            && missingArgument.StopAtRawFileOffsetExclusive == 0x00BBDEAD
            && !missingArgument.BoundedCompletionBoundaryReached
            && !missingArgument.ExactBoundedCompletion
            && missingEsp.Outcome
                == NetworkDispatcherFinalCallPreparationOutcome.MissingFreshEntryESP
            && missingEsp.Reads.Single().Captured
            && missingEsp.Pushes is [{
                StackPointerBefore: null,
                StackPointerAfter: null,
                Value: 0,
                Attempted: true,
                Committed: false
            }]
            && missingEsp.CaptureStates.FreshEntryESP
                == NetworkDispatcherFinalCallPreparationCaptureState.Missing
            && missingEsp.CaptureStates.FreshArgumentDWORD
                == NetworkDispatcherFinalCallPreparationCaptureState.Captured
            && missingEsp.StopAtRawFileOffsetExclusive == 0x00BBDEAD
            && missingEsp.CallBoundaries.IsEmpty
            && !missingEsp.BoundedCompletionBoundaryReached);

        NetworkDispatcherFinalCallPreparationCapture aliasCapture = Capture(
            freshPostCellEBP: 0x107C,
            freshPostCellESI: 0x1000,
            freshPostCellEBX: 0x1034,
            freshEntryESP: 0x1068,
            freshArgumentDWORD: 0xDEADBEEF);
        NetworkDispatcherFinalCallPreparationPlan alias = Project(aliasCapture);
        int publicationEffectIndex = Array.FindIndex(alias.OrderedEffects.ToArray(), effect =>
            effect.ParentEffect?.Kind
                == NetworkDispatcherFencedPublicationOrderedEffectKind.PublicationByteStore);
        int counterEffectIndex = Array.FindIndex(alias.OrderedEffects.ToArray(), effect =>
            effect.ParentEffect?.Kind
                == NetworkDispatcherFencedPublicationOrderedEffectKind.CounterDecrementReadWrite);
        int argumentReadEffectIndex = Array.FindIndex(alias.OrderedEffects.ToArray(), effect =>
            effect.Kind
                == NetworkDispatcherFinalCallPreparationOrderedEffectKind.FreshArgumentDWORDRead);
        check("The aliased argument read follows parent publication and counter writes and feeds the same-address push",
            alias.ArgumentAddress == 0x1064
            && alias.ParentPlan.PublicationByteAddress == alias.ArgumentAddress
            && alias.ParentPlan.CounterAddress == alias.ArgumentAddress
            && alias.Pushes.Single().StackPointerAfter == alias.ArgumentAddress
            && alias.Reads.Single().Value == 0xDEADBEEF
            && alias.Pushes.Single().Value == 0xDEADBEEF
            && publicationEffectIndex >= 0
            && counterEffectIndex > publicationEffectIndex
            && argumentReadEffectIndex > counterEffectIndex
            && alias.OrderedEffects.Skip(alias.ParentPlan.OrderedEffects.Length)
                .Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    NetworkDispatcherFinalCallPreparationOrderedEffectKind
                        .FreshArgumentDWORDRead,
                    NetworkDispatcherFinalCallPreparationOrderedEffectKind.Push
                }));

        NetworkDispatcherFinalCallPreparationCapture dirtyCapture = Capture(
            freshParentEax: null,
            freshPostCellEBP: null,
            freshPostCellESI: null,
            freshStackScalarDWORD: null,
            freshFieldScalarDWORD: null,
            freshPostCellEBX: null,
            freshCounterDWORD: null,
            freshEntryESP: 0x8000,
            freshArgumentDWORD: 0x1234);
        NetworkDispatcherFinalCallPreparationPlan dirtyParent = Project(dirtyCapture);
        check("An unclean parent preserves its actual bound and leaves supplied child captures unused",
            !dirtyParent.ChildGateAdmitted
            && !dirtyParent.ParentPlan.ExactCleanCompletion
            && dirtyParent.Outcome
                == NetworkDispatcherFinalCallPreparationOutcome.ParentHandoffNotAdmitted
            && dirtyParent.EntryRawFileOffset
                == dirtyParent.ParentPlan.StopAtRawFileOffsetExclusive
            && dirtyParent.StopAtRawFileOffsetExclusive
                == dirtyParent.ParentPlan.StopAtRawFileOffsetExclusive
            && dirtyParent.OrderedEffects.Length
                == dirtyParent.ParentPlan.OrderedEffects.Length
            && dirtyParent.Reads.IsEmpty
            && dirtyParent.Pushes.IsEmpty
            && dirtyParent.CallBoundaries.IsEmpty
            && dirtyParent.CaptureStates.FreshEntryESP
                == NetworkDispatcherFinalCallPreparationCaptureState.SuppliedUnused
            && dirtyParent.CaptureStates.FreshArgumentDWORD
                == NetworkDispatcherFinalCallPreparationCaptureState.SuppliedUnused
            && !dirtyParent.BoundedCompletionBoundaryReached);

        NetworkDispatcherFinalCallPreparationPlan reprojected = Project(aliasCapture);
        check("Reprojection preserves the immutable parent-effect prefix and performs no parent mutation",
            ReferenceEquals(alias.Capture, aliasCapture)
            && ReferenceEquals(reprojected.Capture, aliasCapture)
            && alias.ParentPlan.OrderedEffects.SequenceEqual(
                reprojected.ParentPlan.OrderedEffects)
            && alias.OrderedEffects.Take(alias.ParentPlan.OrderedEffects.Length)
                .Select(effect => effect.ParentEffect)
                .SequenceEqual(alias.ParentPlan.OrderedEffects.Select(effect =>
                    (NetworkDispatcherFencedPublicationOrderedEffect?)effect))
            && alias.OrderedEffects.Select(effect => effect.Sequence)
                .SequenceEqual(Enumerable.Range(0, alias.OrderedEffects.Length))
            && alias.Reads.Single().Address == reprojected.Reads.Single().Address
            && alias.Reads.Single().Value == reprojected.Reads.Single().Value);
    }

    private static NetworkDispatcherFinalCallPreparationPlan Project(
        NetworkDispatcherFinalCallPreparationCapture capture) =>
        NetworkDispatcherFinalCallPreparationProjection.Project(capture);
}

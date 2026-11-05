using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryCallPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryCallPreparationPlan Project(
            uint collectionEntry = 0u,
            uint? entryEsp = 0x8000,
            uint? vptr = null,
            bool emptyCollection = false,
            bool dirtyParent = false,
            byte parentArgument = 1)
        {
            var parentLock = new NetworkDispatcherLockGateCapture(
                ResultEAX: 1,
                EntryESIReceiver: 0x1000,
                FreshCounterValue: 1,
                EntryEBP: 0x2000,
                FreshArgumentByte: parentArgument,
                OpaqueImportReturnedNormally: dirtyParent ? true : null);
            var parentCollection = new NetworkDispatcherCollectionGateCapture(
                parentLock,
                EntryESP: 0x3000,
                EntryECX: 0x4444,
                FreshThresholdDWORD: 1,
                OpaqueCallReturnedNormally: true,
                FreshPostLoggingESI: 0x2000,
                FreshEndDWORD: emptyCollection ? 0x2000u : 0x2004u,
                FreshStartDWORD: 0x2000,
                FreshReloadDWORD: 0x4000,
                FreshCollectionEntryDWORD: collectionEntry);
            return NetworkDispatcherEntryCallPreparationProjection.Project(new(
                parentCollection,
                entryEsp,
                vptr));
        }

        static string Normalize(string value) =>
            string.Concat(value.Where(character => !char.IsWhiteSpace(character)));

        check("The entry-call window and both indirect-call boundaries are anchored exactly",
            NetworkDispatcherEntryCallPreparationProjection.EntryRawFileOffset == 0x00BBDE4D
            && NetworkDispatcherEntryCallPreparationProjection.RawToVirtualAddressDelta
                == 0x00400C00
            && Normalize(NetworkDispatcherEntryCallPreparationProjection.CapturedWindowBytes)
                == "57683B020000687F71B501687E71B501FF15A05EC10183C4108B078BCF6A01FF5010"
            && NetworkDispatcherEntryCallPreparationProjection.ZeroEntryPushEDIRawFileOffset
                == 0x00BBDE4D
            && NetworkDispatcherEntryCallPreparationProjection.ZeroEntryPushArgumentCountRawFileOffset
                == 0x00BBDE4E
            && NetworkDispatcherEntryCallPreparationProjection.ZeroEntryPushFirstAddressRawFileOffset
                == 0x00BBDE53
            && NetworkDispatcherEntryCallPreparationProjection.ZeroEntryPushSecondAddressRawFileOffset
                == 0x00BBDE58
            && NetworkDispatcherEntryCallPreparationProjection.ZeroEntryOpaqueImportCallRawFileOffset
                == 0x00BBDE5D
            && NetworkDispatcherEntryCallPreparationProjection.ZeroEntryOpaqueImportCellVirtualAddress
                == 0x01C15EA0
            && NetworkDispatcherEntryCallPreparationProjection.NonzeroEntryReadVptrRawFileOffset
                == 0x00BBDE66
            && NetworkDispatcherEntryCallPreparationProjection.NonzeroEntryMoveEDIToECXRawFileOffset
                == 0x00BBDE68
            && NetworkDispatcherEntryCallPreparationProjection.NonzeroEntryPushOneRawFileOffset
                == 0x00BBDE6A
            && NetworkDispatcherEntryCallPreparationProjection.NonzeroEntryVtableCallRawFileOffset
                == 0x00BBDE6C
            && NetworkDispatcherEntryCallPreparationProjection.VtableSlotOffset == 0x10
            && NetworkDispatcherEntryCallPreparationProjection.ArgumentCount == 0x23B
            && NetworkDispatcherEntryCallPreparationProjection.FirstAddressImmediate == 0x01B5717F
            && NetworkDispatcherEntryCallPreparationProjection.SecondAddressImmediate == 0x01B5717E
            && !NetworkDispatcherEntryCallPreparationProjection.ExecutesNativeInstructions);

        var zero = Project(collectionEntry: 0, entryEsp: 0x8000);
        check("A zero entry reuses fresh collection EDI and reaches the import-cell boundary after four pushes",
            zero.ParentCollectionGatePlan.Outcome
                == NetworkDispatcherCollectionGateOutcome.ZeroCollectionEntry
            && zero.ChildGateAdmitted
            && zero.Outcome
                == NetworkDispatcherEntryCallPreparationOutcome.ZeroEntryOpaqueImportBoundary
            && zero.ParentCollectionGatePlan.Capture.EntryESP == 0x3000
            && zero.Capture.FreshEntryESP == 0x8000
            && zero.EntryEDI == 0
            && zero.EntryESI == 0x2000
            && zero.EntryESI
                != zero.ParentCollectionGatePlan.Capture.ParentGate.EntryESIReceiver
            && zero.EntryEBX == 0
            && zero.EntryEAX == 0x4000
            && zero.FlagsAtStop == zero.ParentCollectionGatePlan.FlagsAtStop
            && zero.FlagsAtStop is { Zero: true, Sign: false, Parity: true }
            && zero.Pushes.Select(push => push.RawFileOffset).SequenceEqual(
                new uint[] { 0x00BBDE4D, 0x00BBDE4E, 0x00BBDE53, 0x00BBDE58 })
            && zero.Pushes.Select(push => push.Value).SequenceEqual(
                new uint?[] { 0, 0x23B, 0x01B5717F, 0x01B5717E })
            && zero.Pushes.Select(push => push.StackPointerAfter).SequenceEqual(
                new uint?[] { 0x7FFC, 0x7FF8, 0x7FF4, 0x7FF0 })
            && zero.Pushes.All(push => push.Attempted && push.Committed)
            && zero.Reads.IsEmpty
            && zero.CallBoundaries is [{
                RawFileOffset: 0x00BBDE5D,
                TargetKind: NetworkDispatcherEntryCallPreparationTargetKind.OpaqueImportCell,
                ImportCellVirtualAddress: 0x01C15EA0,
                VtableSlotAddress: null,
                CallSiteReached: true,
                TargetReadAttempted: false,
                TargetCaptured: false,
                CallAttempted: false
            }]
            && zero.StopAtRawFileOffsetExclusive == 0x00BBDE5D
            && zero.BoundedCompletionBoundaryReached
            && zero.ExactBoundedCompletion
            && zero.ExactCleanCompletion
            && zero.CaptureStates.FreshEntryESP
                == NetworkDispatcherEntryCallPreparationCaptureState.Captured
            && zero.CaptureStates.FreshVptrDWORD
                == NetworkDispatcherEntryCallPreparationCaptureState.Unreached
            && zero.OpaqueCallsExecuted == false
            && zero.VtableCallExecuted == false
            && zero.CollectionIterationModeled == false
            && zero.UnlockExecuted == false
            && zero.ReallocationModeled == false);

        var nonzero = Project(
            collectionEntry: 0x8000_0100,
            entryEsp: 2,
            vptr: 0xFFFF_FFF8);
        check("The nonzero route independently reads [EDI], moves EDI to ECX, and wraps stack and slot addresses",
            nonzero.ParentCollectionGatePlan.Outcome
                == NetworkDispatcherCollectionGateOutcome.NonzeroCollectionEntry
            && nonzero.Outcome
                == NetworkDispatcherEntryCallPreparationOutcome.NonzeroEntryVtableCallBoundary
            && nonzero.EntryEDI == 0x80000100
            && nonzero.EntryEAX == 0xFFFFFFF8
            && nonzero.EntryECX == 0x80000100
            && nonzero.EntryESI == 0x2000
            && nonzero.EntryEBX == 0
            && nonzero.Reads is [{
                RawFileOffset: 0x00BBDE66,
                Kind: NetworkDispatcherEntryCallPreparationReadKind.FreshVptrDWORD,
                Address: 0x80000100,
                Width: 4,
                Attempted: true,
                Captured: true,
                Value: 0xFFFFFFF8,
                Committed: false
            }]
            && nonzero.Pushes is [{
                RawFileOffset: 0x00BBDE6A,
                Kind: NetworkDispatcherEntryCallPreparationPushKind.One,
                StackPointerBefore: 2,
                StackPointerAfter: 0xFFFFFFFE,
                Value: 1,
                Attempted: true,
                Committed: true
            }]
            && nonzero.VtableSlotAddress == 8
            && nonzero.CallBoundaries is [{
                RawFileOffset: 0x00BBDE6C,
                TargetKind: NetworkDispatcherEntryCallPreparationTargetKind.VtableSlot,
                ImportCellVirtualAddress: null,
                VtableSlotAddress: 8,
                CallSiteReached: true,
                TargetReadAttempted: false,
                TargetCaptured: false,
                CallAttempted: false
            }]
            && nonzero.FlagsAtStop == nonzero.ParentCollectionGatePlan.FlagsAtStop
            && nonzero.FlagsAtStop is { Zero: false, Sign: true, Parity: true }
            && nonzero.InstructionOrder.Select(instruction => instruction.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x00BBDE66, 0x00BBDE68, 0x00BBDE6A, 0x00BBDE6C
                })
            && nonzero.StopAtRawFileOffsetExclusive == 0x00BBDE6C
            && nonzero.BoundedCompletionBoundaryReached
            && nonzero.ExactCleanCompletion
            && nonzero.CaptureStates.FreshEntryESP
                == NetworkDispatcherEntryCallPreparationCaptureState.Captured
            && nonzero.CaptureStates.FreshVptrDWORD
                == NetworkDispatcherEntryCallPreparationCaptureState.Captured);

        var missingZeroEsp = Project(collectionEntry: 0, entryEsp: null);
        var missingVptr = Project(
            collectionEntry: 0x1234,
            entryEsp: 0x8000,
            vptr: null);
        var missingNonzeroEsp = Project(
            collectionEntry: 0x1234,
            entryEsp: null,
            vptr: 0x5000);
        check("Missing fresh ESP or vptr stops at its first dependent instruction without inventing a boundary",
            missingZeroEsp.Outcome
                == NetworkDispatcherEntryCallPreparationOutcome.MissingFreshEntryESP
            && missingZeroEsp.StopAtRawFileOffsetExclusive == 0x00BBDE4D
            && missingZeroEsp.Pushes is [{
                StackPointerBefore: null,
                StackPointerAfter: null,
                Value: 0,
                Attempted: true,
                Committed: false
            }]
            && missingZeroEsp.FlagsAtStop
                == missingZeroEsp.ParentCollectionGatePlan.FlagsAtStop
            && !missingZeroEsp.BoundedCompletionBoundaryReached
            && !missingZeroEsp.ExactBoundedCompletion
            && missingZeroEsp.CaptureStates.FreshEntryESP
                == NetworkDispatcherEntryCallPreparationCaptureState.Missing
            && missingVptr.Outcome
                == NetworkDispatcherEntryCallPreparationOutcome.MissingFreshVptrDWORD
            && missingVptr.StopAtRawFileOffsetExclusive == 0x00BBDE66
            && missingVptr.Reads is [{
                Address: 0x1234,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false
            }]
            && missingVptr.Pushes.IsEmpty
            && missingVptr.InstructionOrder.Length == 1
            && missingVptr.CaptureStates.FreshEntryESP
                == NetworkDispatcherEntryCallPreparationCaptureState.SuppliedButUnused
            && !missingVptr.BoundedCompletionBoundaryReached
            && missingNonzeroEsp.Outcome
                == NetworkDispatcherEntryCallPreparationOutcome.MissingFreshEntryESP
            && missingNonzeroEsp.StopAtRawFileOffsetExclusive == 0x00BBDE6A
            && missingNonzeroEsp.EntryECX == 0x1234
            && missingNonzeroEsp.Pushes is [{
                StackPointerBefore: null,
                StackPointerAfter: null,
                Value: 1,
                Attempted: true,
                Committed: false
            }]
            && missingNonzeroEsp.CallBoundaries.IsEmpty
            && !missingNonzeroEsp.BoundedCompletionBoundaryReached);

        var unusedZeroVptr = Project(
            collectionEntry: 0,
            entryEsp: 0x9000,
            vptr: 0x7000);
        var zeroVptr = Project(
            collectionEntry: 0x1234,
            entryEsp: 0x9000,
            vptr: 0);
        check("A supplied zero-route vptr remains unused and dirties completion without erasing the reached 5D boundary",
            unusedZeroVptr.Outcome
                == NetworkDispatcherEntryCallPreparationOutcome.ZeroEntryOpaqueImportBoundary
            && unusedZeroVptr.Reads.IsEmpty
            && unusedZeroVptr.CaptureStates.FreshVptrDWORD
                == NetworkDispatcherEntryCallPreparationCaptureState.SuppliedButUnused
            && unusedZeroVptr.StopAtRawFileOffsetExclusive == 0x00BBDE5D
            && unusedZeroVptr.BoundedCompletionBoundaryReached
            && unusedZeroVptr.ExactBoundedCompletion
            && !unusedZeroVptr.ExactCleanCompletion
            && zeroVptr.ParentCollectionGatePlan.Outcome
                == NetworkDispatcherCollectionGateOutcome.NonzeroCollectionEntry
            && zeroVptr.EntryEAX == 0
            && zeroVptr.VtableSlotAddress == 0x10
            && zeroVptr.ExactCleanCompletion);

        var emptyParent = Project(collectionEntry: 0, emptyCollection: true);
        var dirtyParent = Project(collectionEntry: 0, dirtyParent: true);
        var rejectedParent = Project(collectionEntry: 0, parentArgument: 0);
        check("Rejected parents preserve their exact 83, 0E, and parent-selected bounds with no child effects",
            emptyParent.ParentCollectionGatePlan.StopAtRawFileOffsetExclusive == 0x00BBDE83
            && emptyParent.Outcome
                == NetworkDispatcherEntryCallPreparationOutcome.ParentGateDidNotSelectCollectionEntry
            && emptyParent.StopAtRawFileOffsetExclusive == 0x00BBDE83
            && emptyParent.ExactBoundedCompletion
            && !emptyParent.BoundedCompletionBoundaryReached
            && emptyParent.Reads.IsEmpty
            && emptyParent.Pushes.IsEmpty
            && emptyParent.CallBoundaries.IsEmpty
            && dirtyParent.ParentCollectionGatePlan.StopAtRawFileOffsetExclusive == 0x00BBDE0E
            && dirtyParent.Outcome
                == NetworkDispatcherEntryCallPreparationOutcome.ParentGateExitNotClean
            && dirtyParent.StopAtRawFileOffsetExclusive == 0x00BBDE0E
            && dirtyParent.ExactBoundedCompletion
            && dirtyParent.Reads.IsEmpty
            && dirtyParent.Pushes.IsEmpty
            && dirtyParent.CallBoundaries.IsEmpty
            && rejectedParent.Outcome
                == NetworkDispatcherEntryCallPreparationOutcome.ParentGateDidNotSelectCollectionEntry
            && rejectedParent.StopAtRawFileOffsetExclusive
                == rejectedParent.ParentCollectionGatePlan.StopAtRawFileOffsetExclusive
            && rejectedParent.ExactBoundedCompletion
            && rejectedParent.Reads.IsEmpty
            && rejectedParent.Pushes.IsEmpty
            && rejectedParent.CaptureStates.FreshEntryESP
                == NetworkDispatcherEntryCallPreparationCaptureState.SuppliedButUnused);

        check("Parent effects remain an immutable prefix followed by fresh vptr reads and committed stack writes",
            zero.OrderedEffects.Take(zero.ParentCollectionGatePlan.OrderedEffects.Length)
                .Select(effect => effect.ParentCollectionGateEffect)
                .SequenceEqual(zero.ParentCollectionGatePlan.OrderedEffects
                    .Select(effect =>
                        (NetworkDispatcherCollectionGateOrderedEffect?)effect))
            && zero.OrderedEffects.TakeLast(zero.Pushes.Length)
                .Select(effect => effect.Push)
                .SequenceEqual(zero.Pushes.Select(push =>
                    (NetworkDispatcherEntryCallPreparationPush?)push))
            && nonzero.OrderedEffects.TakeLast(2)
                .Select(effect => (effect.Read, effect.Push))
                .SequenceEqual(new[]
                {
                    ((NetworkDispatcherEntryCallPreparationRead?)nonzero.Reads.Single(),
                        (NetworkDispatcherEntryCallPreparationPush?)null),
                    ((NetworkDispatcherEntryCallPreparationRead?)null,
                        (NetworkDispatcherEntryCallPreparationPush?)nonzero.Pushes.Single())
                })
            && zero.OrderedEffects.Select(effect => effect.Sequence)
                .SequenceEqual(Enumerable.Range(0, zero.OrderedEffects.Length))
            && missingVptr.OrderedEffects.TakeLast(1).Single().Read == missingVptr.Reads.Single()
            && !NetworkDispatcherEntryCallPreparationProjection.ExecutesNativeInstructions);
    }
}

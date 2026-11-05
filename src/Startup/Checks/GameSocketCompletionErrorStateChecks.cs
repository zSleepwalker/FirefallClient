using Firefall.Reconstruction.Startup;

internal static class GameSocketCompletionErrorStateChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("The continuation bounds and offsets match the internal native sequence",
            GameSocketCompletionErrorStateProjection.ContinuationRawFileOffset == 0xBD2DEB
            && GameSocketCompletionErrorStateProjection.ContinuationVirtualAddress == 0xFD39EB
            && GameSocketCompletionErrorStateProjection.StateCompareExchangeRawFileOffset == 0xBD2DF8
            && GameSocketCompletionErrorStateProjection.TestEaxRawFileOffset == 0xBD2DFC
            && GameSocketCompletionErrorStateProjection.ZeroTestBranchRawFileOffset == 0xBD2DFE
            && GameSocketCompletionErrorStateProjection.FreshFieldReadRawFileOffset == 0xBD2E00
            && GameSocketCompletionErrorStateProjection.CopyDestinationWriteRawFileOffset == 0xBD2E06
            && GameSocketCompletionErrorStateProjection.QueueAppendCallRawFileOffset == 0xBD2E14
            && GameSocketCompletionErrorStateProjection.PostQueueAppendContinuationRawFileOffset == 0xBD2E19
            && GameSocketCompletionErrorStateProjection.QueueAppendTargetRawFileOffset == 0x12CE310
            && GameSocketCompletionErrorStateProjection.QueueAppendTargetVirtualAddress == 0x16CEF10
            && GameSocketCompletionErrorStateProjection.QueueAppendEventToken == 2
            && GameSocketCompletionErrorStateProjection.QueueAppendEventToken
                == (uint)GameSocketReceivedQueueEvent.PumpCloseRequest);

        GameSocketCompletionErrorStatePlan zeroOld = Project(
            true, 0x1000, 0, 0xFFFFFFFF, true);
        check("Zero old state commits ten then independently reads and copies the fresh field",
            zeroOld.Outcome == GameSocketCompletionErrorStateOutcome.BoundedAtQueueAppendReturn
            && zeroOld.Disposition
                == GameSocketCompletionErrorStateDisposition.BoundedAfterQueueAppendReturn
            && zeroOld.ReachedBoundedPostCallContinuation
            && zeroOld.AtomicOperationAttempted
            && zeroOld.StateCompareExchangeWriteCommitted == true
            && zeroOld.ZeroTestBranchTaken == false
            && zeroOld.Writes.Length == 2
            && zeroOld.Writes[0] == new GameSocketCompletionErrorStateWrite(
                0x124, 0x1124, 10, 0xBD2DF8)
            && zeroOld.Writes[1] == new GameSocketCompletionErrorStateWrite(
                0x128, 0x1128, 0xFFFFFFFF, 0xBD2E06)
            && zeroOld.EsiFreshFieldAddress == 0x1108
            && zeroOld.EsiCopyDestinationAddress == 0x1128
            && zeroOld.QueueObjectAddress == 0x1260
            && zeroOld.FlagsAfterCompareExchange is
                { Carry: false, Parity: true, AuxiliaryCarry: false, Zero: true, Sign: false, Overflow: false }
            && zeroOld.FlagsAfterTestEax is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: true, Sign: false, Overflow: false }
            && zeroOld.QueueAppendReturnedNormally == true
            && zeroOld.LastReachedRawFileOffset == 0xBD2E14
            && zeroOld.StopBeforeRawFileOffset == 0xBD2E19
            && zeroOld.FlagsAtStop is null
            && zeroOld.IsCleanCompletion
            && zeroOld.OrderedMemoryEffects.Length == 4
            && zeroOld.MemoryEffects[0] is
                { Kind: GameSocketCompletionErrorStateMemoryEffectKind.AtomicStateRead,
                  Address: 0x1124, Value: 0, Captured: true, Committed: false }
            && zeroOld.MemoryEffects[1] is
                { Kind: GameSocketCompletionErrorStateMemoryEffectKind.AtomicConditionalStateWrite,
                  Address: 0x1124, Value: 10, Captured: true, Committed: true }
            && zeroOld.MemoryEffects[2] is
                { Kind: GameSocketCompletionErrorStateMemoryEffectKind.FreshFieldRead,
                  Address: 0x1108, Value: 0xFFFFFFFF, Captured: true, Committed: false }
            && zeroOld.MemoryEffects[3] is
                { Kind: GameSocketCompletionErrorStateMemoryEffectKind.CopyDestinationWrite,
                  Address: 0x1128, Value: 0xFFFFFFFF, Captured: true, Committed: true }
            && zeroOld.UnusedCaptures.IsEmpty);

        GameSocketCompletionErrorStatePlan nonzeroOld = Project(
            true, 0x2000, 7, 0xAABBCCDD, true);
        check("Nonzero old state records a failed compare-exchange without a write and skips the fresh load",
            nonzeroOld.Outcome == GameSocketCompletionErrorStateOutcome.BoundedAtQueueAppendReturn
            && nonzeroOld.AtomicOperationAttempted
            && nonzeroOld.StateCompareExchangeWriteCommitted == false
            && nonzeroOld.ZeroTestBranchTaken == true
            && nonzeroOld.Writes.IsEmpty
            && nonzeroOld.CaptureStates.FreshField108
                == GameSocketCompletionErrorStateCaptureState.SuppliedButUnused
            && nonzeroOld.UnusedCaptures.Length == 1
            && nonzeroOld.UnusedCaptures[0]
                == GameSocketCompletionErrorStateCaptureKind.FreshField108
            && !nonzeroOld.ReachedInstructionRawFileOffsets.Contains(0xBD2E00)
            && nonzeroOld.HasActualBoundedCompletion
            && !nonzeroOld.IsCleanCompletion
            && nonzeroOld.EsiFreshFieldAddress is null
            && nonzeroOld.EsiCopyDestinationAddress is null
            && nonzeroOld.DerivedEsiFreshFieldAddress == 0x2108
            && nonzeroOld.DerivedEsiCopyDestinationAddress == 0x2128
            && nonzeroOld.QueueObjectAddress == 0x2260
            && nonzeroOld.FlagsAfterTestEax is
                { Carry: false, AuxiliaryCarry: null, Zero: false, Sign: false, Overflow: false }
            && nonzeroOld.FlagsAtStop is null
            && nonzeroOld.QueueAppendReturnedNormally == true);

        GameSocketCompletionErrorStatePlan missingAtomic = Project(
            true, 0x3000, null, 0x1234, true);
        check("Missing atomic observation records the attempted read and unknown conditional write",
            missingAtomic.Outcome == GameSocketCompletionErrorStateOutcome.MissingAtomicObservation
            && missingAtomic.Disposition
                == GameSocketCompletionErrorStateDisposition.BoundedAtAtomicObservation
            && missingAtomic.AtomicOperationAttempted
            && missingAtomic.AtomicReadAttempted
            && !missingAtomic.AtomicReadEffectCaptured
            && missingAtomic.StateCompareExchangeWriteCommitted is null
            && missingAtomic.Writes.IsEmpty
            && missingAtomic.MemoryEffects.Length == 2
            && missingAtomic.MemoryEffects[0] is
                { Kind: GameSocketCompletionErrorStateMemoryEffectKind.AtomicStateRead,
                  Address: 0x3124, Value: null, Attempted: true, Captured: false, Committed: false }
            && missingAtomic.MemoryEffects[1] is
                { Kind: GameSocketCompletionErrorStateMemoryEffectKind.AtomicConditionalStateWrite,
                  Address: 0x3124, Value: 10, Attempted: true, Captured: false, Committed: null }
            && missingAtomic.LastReachedRawFileOffset == 0xBD2DF8
            && missingAtomic.StopBeforeRawFileOffset == 0xBD2DF8
            && missingAtomic.FlagsAtStop == missingAtomic.FlagsBeforeAtomicOperation
            && missingAtomic.FlagsBeforeAtomicOperation is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: true, Sign: false, Overflow: false }
            && missingAtomic.EsiStateSlotAddress == 0x3124
            && missingAtomic.EsiFreshFieldAddress is null
            && missingAtomic.QueueObjectAddress is null
            && missingAtomic.DerivedEsiFreshFieldAddress == 0x3108
            && missingAtomic.UnusedCaptures.Contains(
                GameSocketCompletionErrorStateCaptureKind.FreshField108)
            && missingAtomic.UnusedCaptures.Contains(
                GameSocketCompletionErrorStateCaptureKind.QueueAppendReturnedNormally));

        GameSocketCompletionErrorStatePlan missingFresh = Project(
            true, 0x4000, 0, null, true);
        check("Missing fresh value records the attempted read and preserves prior effects and TEST flags",
            missingFresh.Outcome == GameSocketCompletionErrorStateOutcome.MissingFreshField108
            && missingFresh.Disposition
                == GameSocketCompletionErrorStateDisposition.BoundedAtFreshFieldRead
            && missingFresh.StateCompareExchangeWriteCommitted == true
            && missingFresh.Writes.Length == 1
            && missingFresh.Writes[0].Value == 10
            && missingFresh.MemoryEffects.Length == 3
            && missingFresh.FreshFieldReadAttempted
            && !missingFresh.FreshFieldReadEffectCaptured
            && missingFresh.MemoryEffects[2] is
                { Kind: GameSocketCompletionErrorStateMemoryEffectKind.FreshFieldRead,
                  Address: 0x4108, Value: null, Attempted: true, Captured: false, Committed: false }
            && missingFresh.FlagsAfterTestEax is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: true, Sign: false, Overflow: false }
            && missingFresh.FlagsAtStop == missingFresh.FlagsAfterTestEax
            && missingFresh.LastReachedRawFileOffset == 0xBD2E00
            && missingFresh.StopBeforeRawFileOffset == 0xBD2E00
            && missingFresh.EsiFreshFieldAddress == 0x4108
            && missingFresh.EsiCopyDestinationAddress is null
            && missingFresh.QueueObjectAddress is null
            && !missingFresh.QueueAppendCallSiteReached
            && missingFresh.CaptureStates.QueueAppendReturnedNormally
                == GameSocketCompletionErrorStateCaptureState.SuppliedButUnused);

        GameSocketCompletionErrorStatePlan zeroFresh = Project(
            true, 0x5000, 0, 0, true);
        check("A captured zero fresh field is still copied as a separate write",
            zeroFresh.Writes.Length == 2
            && zeroFresh.MemoryEffects.Length == 4
            && zeroFresh.Writes[1].Value == 0
            && zeroFresh.Writes[1].FieldOffset == 0x128
            && zeroFresh.CaptureStates.FreshField108
                == GameSocketCompletionErrorStateCaptureState.Captured);

        GameSocketCompletionErrorStatePlan allOnesOld = Project(
            true, 0x6000, 0xFFFFFFFF, null, true);
        check("All-ones old state preserves exact CMPXCHG and TEST flags while taking the skip branch",
            allOnesOld.FlagsAfterCompareExchange is
                { Carry: true, Parity: false, AuxiliaryCarry: true, Zero: false, Sign: false, Overflow: false }
            && allOnesOld.FlagsAfterTestEax is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: false, Sign: true, Overflow: false }
            && allOnesOld.ZeroTestBranchTaken == true
            && allOnesOld.CaptureStates.FreshField108
                == GameSocketCompletionErrorStateCaptureState.NotReached);

        GameSocketCompletionErrorStatePlan signBoundaryOld = Project(
            true, 0x7000, 0x80000000, null, true);
        check("The signed minimum old value sets compare overflow and preserves TEST sign",
            signBoundaryOld.FlagsAfterCompareExchange is
                { Carry: true, Parity: true, AuxiliaryCarry: false, Zero: false, Sign: true, Overflow: true }
            && signBoundaryOld.FlagsAfterTestEax is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: false, Sign: true, Overflow: false });

        GameSocketCompletionErrorStatePlan wrappedEsi = Project(
            true, 0xFFFFFF00, 0, 0x55, true);
        check("Derived and reached field addresses use unsigned 32-bit ESI wraparound",
            wrappedEsi.EsiStateSlotAddress == 0x24
            && wrappedEsi.EsiFreshFieldAddress == 0x08
            && wrappedEsi.EsiCopyDestinationAddress == 0x28
            && wrappedEsi.QueueObjectAddress == 0x160
            && wrappedEsi.DerivedEsiStateSlotAddress == 0x24
            && wrappedEsi.DerivedEsiFreshFieldAddress == 0x08
            && wrappedEsi.DerivedEsiCopyDestinationAddress == 0x28
            && wrappedEsi.DerivedQueueObjectAddress == 0x160
            && wrappedEsi.Writes[0].Address == 0x24
            && wrappedEsi.Writes[1].Address == 0x28);

        GameSocketCompletionErrorStatePlan unknownQueueReturn = Project(
            true, 0x8000, 3, 0x1234, null);
        check("An unknown opaque queue return stops at the call site without claiming return or cleanup",
            unknownQueueReturn.Outcome
                == GameSocketCompletionErrorStateOutcome.MissingQueueAppendReturnObservation
            && unknownQueueReturn.Disposition
                == GameSocketCompletionErrorStateDisposition.BoundedAtQueueAppendCallSite
            && unknownQueueReturn.QueueAppendCallSiteReached
            && unknownQueueReturn.QueueAppendReturnedNormally is null
            && unknownQueueReturn.LastReachedRawFileOffset == 0xBD2E14
            && unknownQueueReturn.StopBeforeRawFileOffset == 0xBD2E19
            && unknownQueueReturn.FlagsAfterTestEax is not null
            && unknownQueueReturn.FlagsAtStop == unknownQueueReturn.FlagsAfterTestEax
            && !unknownQueueReturn.ReachedBoundedPostCallContinuation);

        GameSocketCompletionErrorStatePlan abnormalQueueReturn = Project(
            true, 0x9000, 0, 0, false);
        check("A non-normal opaque queue result never reaches the post-call boundary",
            abnormalQueueReturn.Outcome
                == GameSocketCompletionErrorStateOutcome.QueueAppendDidNotReturnNormally
            && abnormalQueueReturn.QueueAppendReturnedNormally == false
            && abnormalQueueReturn.StopBeforeRawFileOffset == 0xBD2E19
            && abnormalQueueReturn.FlagsAtStop == abnormalQueueReturn.FlagsAfterTestEax
            && !abnormalQueueReturn.ReachedBoundedPostCallContinuation);

        GameSocketCompletionErrorStatePlan missingEntry = Project(
            null, 0xA000, 0, 0x10, true);
        check("Missing continuation admission leaves supplied inner captures visible as unused",
            missingEntry.Outcome
                == GameSocketCompletionErrorStateOutcome.MissingContinuationValidation
            && missingEntry.StopBeforeRawFileOffset == 0xBD2DEB
            && missingEntry.CaptureStates.InheritedEntryEsi
                == GameSocketCompletionErrorStateCaptureState.SuppliedButUnused
            && missingEntry.UnusedCaptures.Length == 4
            && missingEntry.Writes.IsEmpty);

        GameSocketCompletionErrorStatePlan rejectedEntry = Project(
            false, 0xB000, 0, 0x10, true);
        check("An unvalidated internal continuation is rejected without inferring an outer error value",
            rejectedEntry.Outcome
                == GameSocketCompletionErrorStateOutcome.ContinuationNotValidated
            && rejectedEntry.StopBeforeRawFileOffset == 0xBD2DEB
            && rejectedEntry.ReachedInstructionRawFileOffsets.IsEmpty
            && !rejectedEntry.InternalContinuationEntered
            && rejectedEntry.UnusedCaptures.Length == 4);

        GameSocketCompletionErrorStatePlan missingEsi = Project(
            true, null, 0, 0x10, true);
        check("Missing inherited ESI stops before address calculation and leaves later inputs unused",
            missingEsi.Outcome == GameSocketCompletionErrorStateOutcome.MissingInheritedEntryEsi
            && missingEsi.LastReachedRawFileOffset == 0xBD2DEB
            && missingEsi.StopBeforeRawFileOffset == 0xBD2DF0
            && missingEsi.InternalContinuationEntered
            && missingEsi.FlagsAtStop is null
            && missingEsi.UnusedCaptures.Length == 3
            && missingEsi.Writes.IsEmpty);
    }

    private static GameSocketCompletionErrorStatePlan Project(
        bool? continuationValidated,
        uint? esi,
        uint? atomicObserved,
        uint? fresh108,
        bool? queueAppendReturnedNormally) =>
        GameSocketCompletionErrorStateProjection.Project(new(
            continuationValidated,
            esi,
            atomicObserved,
            fresh108,
            queueAppendReturnedNormally));
}

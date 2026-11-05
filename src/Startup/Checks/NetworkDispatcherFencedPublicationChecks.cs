using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherFencedPublicationChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        byte[] bytes = Convert.FromHexString(
            NetworkDispatcherFencedPublicationProjection.CapturedWindowBytes);
        check("The fenced-publication bytes and bounded instruction offsets are pinned",
            string.Join(" ", bytes.Select(value => value.ToString("X2")))
                == "0F AE F8 0F AE E8 8B 45 F0 89 01 8B 01 C6 46 64 01 FF 4B 30"
            && bytes.Length == 20
            && NetworkDispatcherFencedPublicationProjection.NotEqualEntryRawFileOffset
                == 0x00BBDE99
            && NetworkDispatcherFencedPublicationProjection.SFENCERawFileOffset
                == 0x00BBDE99
            && NetworkDispatcherFencedPublicationProjection.LFENCERawFileOffset
                == 0x00BBDE9C
            && NetworkDispatcherFencedPublicationProjection.StackScalarReadRawFileOffset
                == 0x00BBDE9F
            && NetworkDispatcherFencedPublicationProjection.FieldScalarStoreRawFileOffset
                == 0x00BBDEA2
            && NetworkDispatcherFencedPublicationProjection.EqualEntryRawFileOffset
                == 0x00BBDEA4
            && NetworkDispatcherFencedPublicationProjection.FieldScalarReadRawFileOffset
                == 0x00BBDEA4
            && NetworkDispatcherFencedPublicationProjection.PublicationByteStoreRawFileOffset
                == 0x00BBDEA6
            && NetworkDispatcherFencedPublicationProjection.CounterDecrementRawFileOffset
                == 0x00BBDEAA
            && NetworkDispatcherFencedPublicationProjection.ContinuationRawFileOffset
                == 0x00BBDEAD
            && !NetworkDispatcherFencedPublicationProjection.ExecutesNativeInstructions);

        NetworkDispatcherFencedPublicationPlan notEqual = Project(
            freshEbp: 0x10,
            freshEsi: 0xFFFFFF99,
            freshStackScalar: 0x11223344,
            freshFieldScalar: 0x55667788,
            freshPostCellEbx: 0xFFFFFFD0,
            freshCounter: 1);
        NetworkDispatcherFencedPublicationOrderedEffect[] childEffects = notEqual.OrderedEffects
            .Skip(notEqual.ParentPlan.OrderedEffects.Length)
            .ToArray();
        check("The actual not-equal path uses fresh reads, preserves alias separation, and wraps stores",
            notEqual.ChildGateAdmitted
            && notEqual.EqualParentBranch == false
            && notEqual.EntryRawFileOffset
                == NetworkDispatcherFencedPublicationProjection.NotEqualEntryRawFileOffset
            && notEqual.StackScalarAddress == 0
            && notEqual.FieldAddress == 1
            && notEqual.StackScalarValue == 0x11223344
            && notEqual.FieldScalarStoreAttempted
            && notEqual.FieldScalarStoreCommitted
            && notEqual.FreshFieldScalarValue == 0x55667788
            && notEqual.FieldScalarStoreCommitted
            && notEqual.PublicationByteAddress
                == unchecked(0xFFFFFF99u + 0x64)
            && notEqual.PublicationByteValue == 1
            && notEqual.CounterAddress == 0
            && notEqual.CounterValueBefore == 1
            && notEqual.CounterValueAfter == 0
            && notEqual.FlagsAtParentBranch is {
                Carry: true,
                Zero: false
            }
            && notEqual.FlagsAfterCounterDecrement is {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && notEqual.FlagsAtStop == notEqual.FlagsAfterCounterDecrement
            && childEffects.Select(effect => effect.Kind).SequenceEqual(new[]
            {
                NetworkDispatcherFencedPublicationOrderedEffectKind.SFENCEObservation,
                NetworkDispatcherFencedPublicationOrderedEffectKind.LFENCEObservation,
                NetworkDispatcherFencedPublicationOrderedEffectKind.FreshStackScalarRead,
                NetworkDispatcherFencedPublicationOrderedEffectKind.FieldScalarStore,
                NetworkDispatcherFencedPublicationOrderedEffectKind.FreshFieldScalarRead,
                NetworkDispatcherFencedPublicationOrderedEffectKind.PublicationByteStore,
                NetworkDispatcherFencedPublicationOrderedEffectKind.CounterDecrementReadWrite
            })
            && childEffects[0].Width == 0
            && childEffects[1].Width == 0
            && childEffects[2] is { Address: 0, Width: 4, ReadValue: 0x11223344 }
            && childEffects[3] is { Address: 1, Width: 4, Value: 0x11223344 }
            && childEffects[4] is { Address: 1, Width: 4, ReadValue: 0x55667788 }
            && childEffects[3].Value != childEffects[4].ReadValue
            && childEffects[5] is { Width: 1, Value: 1, Committed: true }
            && childEffects[6] is {
                Address: 0,
                Width: 4,
                ReadValue: 1,
                Value: 0,
                Committed: true
            }
            && notEqual.StopAtRawFileOffsetExclusive
                == NetworkDispatcherFencedPublicationProjection.ContinuationRawFileOffset
            && notEqual.ExactCleanCompletion
            && !notEqual.NativeInstructionsExecuted
            && !notEqual.OpaqueCallAttempted
            && !notEqual.ThirdCallTargetEntered);

        NetworkDispatcherFencedPublicationPlan wrappedByte = Project(
            freshEbp: 0x10,
            freshEsi: 0xFFFFFF9C,
            freshStackScalar: 1,
            freshFieldScalar: 2);
        check("The publication byte address wraps independently from the parent field address",
            wrappedByte.FieldAddress == 4
            && wrappedByte.PublicationByteAddress == 0
            && wrappedByte.PublicationByteValue == 1);

        uint aliasingEBX = unchecked(0xFFFFFF99u + 0x34);
        NetworkDispatcherFencedPublicationPlan aliasedCounter = Project(
            freshEbp: 0x10,
            freshEsi: 0xFFFFFF99,
            freshStackScalar: 2,
            freshFieldScalar: 3,
            freshPostCellEbx: aliasingEBX,
            freshCounter: 0x12345678);
        check("An aliased counter address still uses its independent fresh counter scalar",
            aliasedCounter.PublicationByteAddress == aliasedCounter.CounterAddress
            && aliasedCounter.PublicationByteAddress == unchecked(0xFFFFFF99u + 0x64)
            && aliasedCounter.PublicationByteValue == 1
            && aliasedCounter.CounterValueBefore == 0x12345678
            && aliasedCounter.CounterValueAfter == 0x12345677
            && aliasedCounter.CounterValueBefore != 1u
            && aliasedCounter.CounterDecrementCommitted);

        NetworkDispatcherFencedPublicationPlan equal = Project(
            freshEbp: 0x10,
            freshEsi: 0xFFFFFF98,
            freshStackScalar: 0xDEADBEEF,
            freshFieldScalar: 0xAABBCCDD,
            freshCounter: 0);
        NetworkDispatcherFencedPublicationOrderedEffect[] equalEffects = equal.OrderedEffects
            .Skip(equal.ParentPlan.OrderedEffects.Length)
            .ToArray();
        check("The actual equal path skips fences and stack store but still reads, publishes, and decrements",
            equal.ChildGateAdmitted
            && equal.EqualParentBranch == true
            && equal.EntryRawFileOffset
                == NetworkDispatcherFencedPublicationProjection.EqualEntryRawFileOffset
            && equal.CaptureStates.FreshStackScalarDWORD
                == NetworkDispatcherFencedPublicationCaptureState.SuppliedUnused
            && equal.StackScalarAddress is null
            && equal.StackScalarValue is null
            && !equal.FieldScalarStoreAttempted
            && equal.FreshFieldScalarValue == 0xAABBCCDD
            && equal.CounterValueBefore == 0
            && equal.CounterValueAfter == uint.MaxValue
            && equal.FlagsAtParentBranch?.Carry == false
            && equal.FlagsAfterCounterDecrement is {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && equalEffects.Select(effect => effect.Kind).SequenceEqual(new[]
            {
                NetworkDispatcherFencedPublicationOrderedEffectKind.FreshFieldScalarRead,
                NetworkDispatcherFencedPublicationOrderedEffectKind.PublicationByteStore,
                NetworkDispatcherFencedPublicationOrderedEffectKind.CounterDecrementReadWrite
            })
            && equalEffects[0] is { RawFileOffset: 0x00BBDEA4, Width: 4 }
            && equalEffects[1] is { RawFileOffset: 0x00BBDEA6, Width: 1 }
            && equalEffects[2] is { RawFileOffset: 0x00BBDEAA, Width: 4 }
            && equal.StopAtRawFileOffsetExclusive
                == NetworkDispatcherFencedPublicationProjection.ContinuationRawFileOffset
            && equal.ExactBoundedCompletion
            && !equal.ExactCleanCompletion);

        NetworkDispatcherFencedPublicationPlan cleanEqual = Project(
            freshEbp: 0x10,
            freshEsi: 0xFFFFFF98,
            freshStackScalar: null,
            freshFieldScalar: 0xAABBCCDD,
            freshCounter: 0);
        check("An equal path is clean only when its skipped stack input is unreached",
            cleanEqual.EqualParentBranch == true
            && cleanEqual.CaptureStates.FreshStackScalarDWORD
                == NetworkDispatcherFencedPublicationCaptureState.Unreached
            && cleanEqual.ExactBoundedCompletion
            && cleanEqual.ExactCleanCompletion);

        NetworkDispatcherFencedPublicationPlan decrementOverflow = Project(
            freshEbp: 0x10,
            freshEsi: 0xFFFFFF98,
            freshFieldScalar: 0,
            freshCounter: 0x80000000);
        check("DEC models signed overflow while preserving parent carry",
            decrementOverflow.FlagsAtParentBranch?.Carry == false
            && decrementOverflow.CounterValueAfter == 0x7FFFFFFF
            && decrementOverflow.FlagsAfterCounterDecrement is {
                Carry: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            });

        NetworkDispatcherFencedPublicationPlan missingStack = Project(
            freshEbp: 0x10,
            freshEsi: 0xFFFFFF99,
            freshStackScalar: null,
            freshFieldScalar: 2,
            freshPostCellEbx: 3,
            freshCounter: 4);
        NetworkDispatcherFencedPublicationPlan missingField = Project(
            freshEbp: 0x10,
            freshEsi: 0xFFFFFF99,
            freshStackScalar: 2,
            freshFieldScalar: null,
            freshPostCellEbx: 3,
            freshCounter: 4);
        NetworkDispatcherFencedPublicationPlan missingEBX = Project(
            freshEbp: 0x10,
            freshEsi: 0xFFFFFF99,
            freshStackScalar: 2,
            freshFieldScalar: 3,
            freshPostCellEbx: null,
            freshCounter: 4);
        NetworkDispatcherFencedPublicationPlan missingCounter = Project(
            freshEbp: 0x10,
            freshEsi: 0xFFFFFF99,
            freshStackScalar: 2,
            freshFieldScalar: 3,
            freshPostCellEbx: 0xFFFFFFD0,
            freshCounter: null);
        check("Missing captures stop at the current instruction and preserve earlier committed effects",
            missingStack.Outcome
                == NetworkDispatcherFencedPublicationOutcome.MissingFreshStackScalarDWORD
            && missingStack.StopAtRawFileOffsetExclusive
                == NetworkDispatcherFencedPublicationProjection.StackScalarReadRawFileOffset
            && missingStack.ReachedInstructionRawFileOffsets[^1]
                == NetworkDispatcherFencedPublicationProjection.StackScalarReadRawFileOffset
            && !missingStack.FieldScalarStoreAttempted
            && missingStack.CaptureStates.FreshFieldScalarDWORD
                == NetworkDispatcherFencedPublicationCaptureState.SuppliedUnused
            && !missingStack.ExactBoundedCompletion
            && missingField.Outcome
                == NetworkDispatcherFencedPublicationOutcome.MissingFreshFieldScalarDWORD
            && missingField.StopAtRawFileOffsetExclusive
                == NetworkDispatcherFencedPublicationProjection.FieldScalarReadRawFileOffset
            && missingField.FieldScalarStoreCommitted
            && !missingField.PublicationByteStoreCommitted
            && missingField.CaptureStates.FreshPostCellEBX
                == NetworkDispatcherFencedPublicationCaptureState.SuppliedUnused
            && missingEBX.Outcome
                == NetworkDispatcherFencedPublicationOutcome.MissingFreshPostCellEBX
            && missingEBX.PublicationByteStoreCommitted
            && missingEBX.CounterDecrementAttempted
            && missingEBX.CounterAddress is null
            && !missingEBX.CounterDecrementCommitted
            && missingEBX.CaptureStates.FreshCounterDWORD
                == NetworkDispatcherFencedPublicationCaptureState.SuppliedUnused
            && missingEBX.FlagsAtStop == missingEBX.FlagsAtParentBranch
            && missingCounter.Outcome
                == NetworkDispatcherFencedPublicationOutcome.MissingFreshCounterDWORD
            && missingCounter.CounterAddress == 0
            && !missingCounter.CounterDecrementCommitted
            && missingCounter.FlagsAtStop == missingCounter.FlagsAtParentBranch
            && !missingEBX.ExactBoundedCompletion
            && !missingCounter.ExactBoundedCompletion);

        NetworkDispatcherFencedPublicationPlan pendingParent = Project(
            returnedNormally: null,
            freshStackScalar: 1,
            freshFieldScalar: 2,
            freshPostCellEbx: 3,
            freshCounter: 4);
        check("An unclean parent keeps its own stop and marks every new capture unused",
            !pendingParent.ChildGateAdmitted
            && pendingParent.Outcome
                == NetworkDispatcherFencedPublicationOutcome.ParentHandoffNotAdmitted
            && pendingParent.StopAtRawFileOffsetExclusive
                == pendingParent.ParentPlan.StopAtRawFileOffsetExclusive
            && pendingParent.EntryRawFileOffset
                == pendingParent.ParentPlan.StopAtRawFileOffsetExclusive
            && pendingParent.OrderedEffects
                .Skip(pendingParent.ParentPlan.OrderedEffects.Length).Any() == false
            && pendingParent.CaptureStates.FreshStackScalarDWORD
                == NetworkDispatcherFencedPublicationCaptureState.SuppliedUnused
            && pendingParent.CaptureStates.FreshFieldScalarDWORD
                == NetworkDispatcherFencedPublicationCaptureState.SuppliedUnused
            && pendingParent.CaptureStates.FreshPostCellEBX
                == NetworkDispatcherFencedPublicationCaptureState.SuppliedUnused
            && pendingParent.CaptureStates.FreshCounterDWORD
                == NetworkDispatcherFencedPublicationCaptureState.SuppliedUnused
            && pendingParent.ParentPlan.CaptureStates.FreshPostCellEAX
                == NetworkDispatcherCellReturnAddressGateCaptureState.SuppliedUnused
            && pendingParent.ParentPlan.CaptureStates.FreshPostCellEBP
                == NetworkDispatcherCellReturnAddressGateCaptureState.SuppliedUnused
            && pendingParent.ParentPlan.CaptureStates.FreshPostCellESI
                == NetworkDispatcherCellReturnAddressGateCaptureState.SuppliedUnused
            && !pendingParent.ChildGateReachedEndpoint
            && !pendingParent.OpaqueCallAttempted
            && !pendingParent.ThirdCallTargetEntered);

        NetworkDispatcherFencedPublicationPlan cleanPendingParent = Project(
            returnedNormally: null,
            freshParentEax: null,
            freshEbp: null,
            freshEsi: null,
            freshStackScalar: null,
            freshFieldScalar: null,
            freshPostCellEbx: null,
            freshCounter: null);
        check("A pending parent with no supplied post-cell captures retains the same boundary",
            !cleanPendingParent.ChildGateAdmitted
            && cleanPendingParent.Outcome
                == NetworkDispatcherFencedPublicationOutcome.ParentHandoffNotAdmitted
            && cleanPendingParent.StopAtRawFileOffsetExclusive
                == cleanPendingParent.ParentPlan.StopAtRawFileOffsetExclusive
            && cleanPendingParent.EntryRawFileOffset
                == cleanPendingParent.ParentPlan.StopAtRawFileOffsetExclusive
            && cleanPendingParent.ParentPlan.CaptureStates.FreshPostCellEAX
                == NetworkDispatcherCellReturnAddressGateCaptureState.Unreached
            && cleanPendingParent.ParentPlan.CaptureStates.FreshPostCellEBP
                == NetworkDispatcherCellReturnAddressGateCaptureState.Unreached
            && cleanPendingParent.ParentPlan.CaptureStates.FreshPostCellESI
                == NetworkDispatcherCellReturnAddressGateCaptureState.Unreached
            && !cleanPendingParent.ChildGateReachedEndpoint
            && cleanPendingParent.OrderedEffects
                .Skip(cleanPendingParent.ParentPlan.OrderedEffects.Length).Any() == false);
    }

    private static NetworkDispatcherFencedPublicationPlan Project(
        bool? returnedNormally = true,
        uint? freshParentEax = 0x12345678,
        uint? freshEbp = 0x3010,
        uint? freshEsi = 0x2F98,
        uint? freshStackScalar = 0x11223344,
        uint? freshFieldScalar = 0x55667788,
        uint? freshPostCellEbx = 0x4000,
        uint? freshCounter = 9)
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
        var parentCapture = new NetworkDispatcherCellReturnAddressGateCapture(
            parentCount,
            returnedNormally,
            freshParentEax,
            freshEbp,
            freshEsi);
        return NetworkDispatcherFencedPublicationProjection.Project(
            new(
                parentCapture,
                freshStackScalar,
                freshFieldScalar,
                freshPostCellEbx,
                freshCounter));
    }
}

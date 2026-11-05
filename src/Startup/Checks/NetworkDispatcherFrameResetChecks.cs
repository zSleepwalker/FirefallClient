using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherFrameResetChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("The frame reset bytes and RET4 boundary are exact",
            NetworkDispatcherFrameResetProjection.CapturedWindowBytes == "8BE55DC20400"
            && Convert.ToHexString(Convert.FromHexString(
                NetworkDispatcherFrameResetProjection.CapturedWindowBytes))
                == "8BE55DC20400"
            && NetworkDispatcherFrameResetProjection.EntryRawFileOffset == 0x00BBDEC4
            && NetworkDispatcherFrameResetProjection.MoveESPToEBPRawFileOffset
                == 0x00BBDEC4
            && NetworkDispatcherFrameResetProjection.PopEBPRawFileOffset == 0x00BBDEC6
            && NetworkDispatcherFrameResetProjection.RET4RawFileOffset == 0x00BBDEC7
            && NetworkDispatcherFrameResetProjection.DwordWidth == 4
            && NetworkDispatcherFrameResetProjection.ESPIncrement == 4
            && !NetworkDispatcherFrameResetProjection.ExecutesNativeInstructions);

        NetworkDispatcherFrameResetPlan normal = Project(savedEBP: 0);
        check("MOV ESP,EBP resets from the parent's post-call EBP, then POP EBP stops before RET4",
            normal.ChildGateAdmitted
            && normal.ParentESPAtStop == 0x1010
            && normal.EBPSourceForReset == 0x9010
            && normal.ESPAfterReset == 0x9010
            && normal.Reads is [
                { RawFileOffset: 0x00BBDEC6, Address: 0x9010, Width: 4,
                    Attempted: true, Captured: true, Value: 0, Committed: false }
            ]
            && normal.Pops is [
                { RawFileOffset: 0x00BBDEC6, StackAddress: 0x9010, StackValue: 0,
                    EBPBefore: 0x9010, EBPAfter: 0,
                    StackPointerBefore: 0x9010, StackPointerAfter: 0x9014,
                    StackReadCaptured: true, EBPDerivedFromRead: true,
                    ESPAdvancedInProjection: true }
            ]
            && normal.ESPAtStop == 0x9014
            && normal.EBPAtStop == 0
            && normal.ReachedInstructionRawFileOffsets
                .SequenceEqual(new uint[] { 0x00BBDEC4, 0x00BBDEC6 })
            && normal.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && normal.StopAtRawFileOffsetExclusive == 0x00BBDEC7
            && normal.BoundedCompletionBoundaryReached
            && normal.ExactBoundedCompletion
            && normal.ExactCleanCompletion
            && normal.FreshSavedEBPDWORDState
                == NetworkDispatcherFrameResetCaptureState.Captured
            && !normal.NativeInstructionsExecuted
            && !normal.NativeMemoryWritesCommitted
            && !normal.PopStackReadsAreMemoryWrites
            && !normal.RET4InstructionEntered
            && !normal.EAXReturnValueInferred
            && !normal.RET4StackAdjustmentProjected);

        NetworkDispatcherFrameResetPlan wraps = Project(
            savedEBP: 0xFFFFFFFF,
            postCallEBP: 0xFFFFFFFC);
        NetworkDispatcherFrameResetPlan aliasesEarlierPop = Project(
            savedEBP: 0xDEADBEEF,
            postCallEBP: 0x100C);
        check("The reset and POP arithmetic wrap, and an aliased earlier POP address gets a fresh value",
            wraps.ParentESPAtStop == 0x1010
            && wraps.Reads.Single().Address == 0xFFFFFFFC
            && wraps.Reads.Single().Value == 0xFFFFFFFF
            && wraps.Pops.Single().StackPointerAfter == 0
            && wraps.ESPAtStop == 0
            && wraps.EBPAtStop == 0xFFFFFFFF
            && wraps.ExactCleanCompletion
            && aliasesEarlierPop.ParentESPAtStop == 0x1010
            && aliasesEarlierPop.EBPSourceForReset == 0x100C
            && aliasesEarlierPop.Reads.Single().Address == 0x100C
            && aliasesEarlierPop.Reads.Single().Value == 0xDEADBEEF
            && aliasesEarlierPop.Pops.Single().EBPAfter == 0xDEADBEEF
            && aliasesEarlierPop.ParentPlan.RegistersAtStop.EBX == 0x44444444
            && aliasesEarlierPop.ExactCleanCompletion);

        NetworkDispatcherFrameResetPlan missing = Project(savedEBP: null);
        check("A missing saved-EBP read stops at C6 while retaining reset ESP, EBP, and unknown flags",
            missing.Outcome
                == NetworkDispatcherFrameResetOutcome.MissingFreshSavedEBPDWORD
            && missing.Reads is [
                { RawFileOffset: 0x00BBDEC6, Address: 0x9010,
                    Attempted: true, Captured: false, Value: null, Committed: false }
            ]
            && missing.Pops is [
                { EBPBefore: 0x9010, EBPAfter: 0x9010,
                    StackPointerBefore: 0x9010, StackPointerAfter: 0x9010,
                    Attempted: true, StackReadCaptured: false,
                    EBPDerivedFromRead: false, ESPAdvancedInProjection: false }
            ]
            && missing.ESPAfterReset == 0x9010
            && missing.ESPAtStop == 0x9010
            && missing.EBPAtStop == 0x9010
            && missing.FlagsAtStop == missing.ParentPlan.FlagsAtStop
            && missing.FreshSavedEBPDWORDState
                == NetworkDispatcherFrameResetCaptureState.Missing
            && missing.ReachedInstructionRawFileOffsets
                .SequenceEqual(new uint[] { 0x00BBDEC4, 0x00BBDEC6 })
            && missing.StopAtRawFileOffsetExclusive == 0x00BBDEC6
            && !missing.BoundedCompletionBoundaryReached
            && !missing.ExactBoundedCompletion
            && !missing.ExactCleanCompletion
            && !missing.RET4InstructionEntered
            && !missing.RET4StackAdjustmentProjected);

        NetworkDispatcherFrameResetPlan rejected = Project(
            savedEBP: 0x1234,
            collectionEntry: 0);
        check("A rejected parent forwards its bound and leaves the saved-EBP capture unused",
            !rejected.ChildGateAdmitted
            && rejected.Outcome == NetworkDispatcherFrameResetOutcome.ParentHandoffNotAdmitted
            && rejected.EntryRawFileOffset
                == rejected.ParentPlan.StopAtRawFileOffsetExclusive
            && rejected.StopAtRawFileOffsetExclusive
                == rejected.ParentPlan.StopAtRawFileOffsetExclusive
            && rejected.ExactBoundedCompletion
                == rejected.ParentPlan.ExactBoundedCompletion
            && rejected.Reads.IsEmpty
            && rejected.Pops.IsEmpty
            && rejected.OrderedEffects.All(effect => effect.ParentEffect.HasValue)
            && rejected.FreshSavedEBPDWORDState
                == NetworkDispatcherFrameResetCaptureState.SuppliedUnused);

        NetworkDispatcherFrameResetPlan reprojection =
            NetworkDispatcherFrameResetProjection.Project(normal.Capture);
        check("The parent effect prefix is preserved and projection is deterministic",
            ParentEffectPrefixPreserved(normal)
            && normal.OrderedEffects.Length
                == normal.ParentPlan.OrderedEffects.Length + 3
            && normal.OrderedEffects
                .Skip(normal.ParentPlan.OrderedEffects.Length)
                .Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    NetworkDispatcherFrameResetOrderedEffectKind
                        .LogicalMoveESPToEBPProjection,
                    NetworkDispatcherFrameResetOrderedEffectKind.SavedEBPDWORDRead,
                    NetworkDispatcherFrameResetOrderedEffectKind
                        .LogicalPopEBPAndESPProjection
                })
            && normal.OrderedEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(reprojection.OrderedEffects.Select(effect => effect.RawFileOffset))
            && normal.Reads.SequenceEqual(reprojection.Reads)
            && normal.Pops.SequenceEqual(reprojection.Pops)
            && reprojection.StopAtRawFileOffsetExclusive == 0x00BBDEC7
            && reprojection.ExactCleanCompletion);
    }

    private static NetworkDispatcherFrameResetPlan Project(
        uint? savedEBP = 0xAABBCCDD,
        uint? postCallEBP = 0x9010,
        uint? postCallESP = 0x1000,
        uint collectionEntry = 4270)
    {
        var registerRestore = new NetworkDispatcherRegisterRestoreCapture(
            ParentCapture(collectionEntry, postCallEBP),
            postCallESP,
            FreshPopECXDWORD: 0x11111111,
            FreshPopEDIDWORD: 0x22222222,
            FreshPopESIDWORD: 0x33333333,
            FreshPopEBXDWORD: 0x44444444);
        return NetworkDispatcherFrameResetProjection.Project(new(registerRestore, savedEBP));
    }

    private static NetworkDispatcherFinalReturnSehStoreCapture ParentCapture(
        uint collectionEntry,
        uint? postCallEBP)
    {
        var lockCapture = new NetworkDispatcherLockGateCapture(
            ResultEAX: 1,
            EntryESIReceiver: 0x1000,
            FreshCounterValue: 1,
            EntryEBP: 0x2000,
            FreshArgumentByte: 1,
            OpaqueImportReturnedNormally: null);
        var collection = new NetworkDispatcherCollectionGateCapture(
            lockCapture,
            EntryESP: 0x7000,
            EntryECX: 0x4444,
            FreshThresholdDWORD: 1,
            OpaqueCallReturnedNormally: true,
            FreshPostLoggingESI: 0x2000,
            FreshEndDWORD: 0x2004,
            FreshStartDWORD: 0x2000,
            FreshReloadDWORD: 0x4000,
            FreshCollectionEntryDWORD: collectionEntry);
        var preparation = new NetworkDispatcherEntryCallPreparationInputs(
            collection,
            FreshEntryESP: 0x8000,
            FreshVptrDWORD: collectionEntry == 0 ? null : 0x5000);
        var count = new NetworkDispatcherPostCallCountGateCapture(
            preparation,
            VtableCallReturnedNormally: true,
            FreshPostCallESI: 0x3000,
            FreshEndDWORD: 0x3008,
            FreshPostCallEBX: 1,
            FreshStartDWORD: 0x3000);
        var cell = new NetworkDispatcherCellReturnAddressGateCapture(
            count,
            OpaqueCellReturnedNormally: true,
            FreshPostCellEAX: 0x12345678,
            FreshPostCellEBP: 0x3010,
            FreshPostCellESI: 0x2F98);
        var publication = new NetworkDispatcherFencedPublicationCapture(
            cell,
            FreshStackScalarDWORD: null,
            FreshFieldScalarDWORD: 0x55667788,
            FreshPostCellEBX: 0x4000,
            FreshCounterDWORD: 9);
        var finalCall = new NetworkDispatcherFinalCallPreparationCapture(
            publication,
            FreshEntryESP: 0x9000,
            FreshArgumentDWORD: 0xAABBCCDD);
        var finalReturn = new NetworkDispatcherFinalReturnSehStoreCapture(
            finalCall,
            FreshIndirectTargetDWORD: 0x11223344,
            OpaqueCallReturnedNormally: true,
            FreshPostCallEBP: postCallEBP,
            FreshLocalMinusCDWORD: 0xAABBCCDD);
        return finalReturn;
    }

    private static bool ParentEffectPrefixPreserved(NetworkDispatcherFrameResetPlan plan)
    {
        if (plan.OrderedEffects.Length < plan.ParentPlan.OrderedEffects.Length)
        {
            return false;
        }

        for (int index = 0; index < plan.ParentPlan.OrderedEffects.Length; index++)
        {
            if (plan.OrderedEffects[index].ParentEffect
                    != plan.ParentPlan.OrderedEffects[index]
                || plan.OrderedEffects[index].RawFileOffset
                    != plan.ParentPlan.OrderedEffects[index].RawFileOffset
                || plan.OrderedEffects[index].Kind
                    != NetworkDispatcherFrameResetOrderedEffectKind
                        .ParentRegisterRestoreEffect)
            {
                return false;
            }
        }

        return true;
    }
}

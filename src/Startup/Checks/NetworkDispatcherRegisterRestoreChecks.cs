using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherRegisterRestoreChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("The four POP bytes and exclusive MOV ESP,EBP boundary are exact",
            NetworkDispatcherRegisterRestoreProjection.CapturedWindowBytes == "595F5E5B8BE5"
            && Convert.ToHexString(Convert.FromHexString(
                NetworkDispatcherRegisterRestoreProjection.CapturedWindowBytes))
                == "595F5E5B8BE5"
            && NetworkDispatcherRegisterRestoreProjection.EntryRawFileOffset == 0x00BBDEC0
            && NetworkDispatcherRegisterRestoreProjection.PopECXRawFileOffset == 0x00BBDEC0
            && NetworkDispatcherRegisterRestoreProjection.PopEDIRawFileOffset == 0x00BBDEC1
            && NetworkDispatcherRegisterRestoreProjection.PopESIRawFileOffset == 0x00BBDEC2
            && NetworkDispatcherRegisterRestoreProjection.PopEBXRawFileOffset == 0x00BBDEC3
            && NetworkDispatcherRegisterRestoreProjection.MoveESPToEBPRawFileOffset
                == 0x00BBDEC4
            && NetworkDispatcherRegisterRestoreProjection.DwordWidth == 4
            && NetworkDispatcherRegisterRestoreProjection.ESPIncrement == 4
            && !NetworkDispatcherRegisterRestoreProjection.ExecutesNativeInstructions);

        NetworkDispatcherRegisterRestorePlan normal = Project();
        check("Four independent captured DWORDs restore ECX, EDI, ESI, EBX in order through C4",
            normal.ChildGateAdmitted
            && normal.Outcome
                == NetworkDispatcherRegisterRestoreOutcome.BoundedBeforeMoveESPToEBP
            && normal.InitialESP == 0x1000
            && normal.CurrentESPAtStop == 0x1010
            && normal.RegistersAtStop is {
                ECX: 0x11111111,
                EDI: 0x22222222,
                ESI: 0x33333333,
                EBX: 0x44444444
            }
            && normal.Reads is [
                { RawFileOffset: 0x00BBDEC0, Address: 0x1000, Captured: true,
                    Value: 0x11111111, Committed: false },
                { RawFileOffset: 0x00BBDEC1, Address: 0x1004, Captured: true,
                    Value: 0x22222222, Committed: false },
                { RawFileOffset: 0x00BBDEC2, Address: 0x1008, Captured: true,
                    Value: 0x33333333, Committed: false },
                { RawFileOffset: 0x00BBDEC3, Address: 0x100C, Captured: true,
                    Value: 0x44444444, Committed: false }
            ]
            && normal.Pops is [
                { Register: NetworkDispatcherRegisterRestoreRegister.ECX,
                    StackPointerBefore: 0x1000, StackPointerAfter: 0x1004,
                    RegisterValueDerivedFromRead: true, ESPAdvancedInProjection: true },
                { Register: NetworkDispatcherRegisterRestoreRegister.EDI,
                    StackPointerBefore: 0x1004, StackPointerAfter: 0x1008,
                    RegisterValueDerivedFromRead: true, ESPAdvancedInProjection: true },
                { Register: NetworkDispatcherRegisterRestoreRegister.ESI,
                    StackPointerBefore: 0x1008, StackPointerAfter: 0x100C,
                    RegisterValueDerivedFromRead: true, ESPAdvancedInProjection: true },
                { Register: NetworkDispatcherRegisterRestoreRegister.EBX,
                    StackPointerBefore: 0x100C, StackPointerAfter: 0x1010,
                    RegisterValueDerivedFromRead: true, ESPAdvancedInProjection: true }
            ]
            && normal.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && normal.StopAtRawFileOffsetExclusive == 0x00BBDEC4
            && normal.BoundedCompletionBoundaryReached
            && normal.ExactBoundedCompletion
            && normal.ExactCleanCompletion
            && normal.CaptureStates.FreshPostCallESP
                == NetworkDispatcherRegisterRestoreCaptureState.Captured
            && normal.CaptureStates.FreshPopECXDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.Captured
            && normal.CaptureStates.FreshPopEDIDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.Captured
            && normal.CaptureStates.FreshPopESIDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.Captured
            && normal.CaptureStates.FreshPopEBXDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.Captured
            && !normal.NativeInstructionsExecuted
            && !normal.NativeMemoryWritesCommitted
            && !normal.PopStackReadsAreMemoryWrites
            && !normal.MoveESPToEBPInstructionEntered);

        NetworkDispatcherRegisterRestorePlan wrap = Project(
            esp: 0xFFFFFFF8,
            ecx: 0,
            edi: 0xFFFFFFFF,
            esi: 0,
            ebx: 0xFFFFFFFF);
        NetworkDispatcherRegisterRestorePlan zeroESP = Project(
            esp: 0,
            ecx: 0,
            edi: 0,
            esi: 0,
            ebx: 0);
        check("Zero values remain captured and ESP arithmetic wraps through the third and fourth reads",
            wrap.Reads.Select(read => read.Address)
                .SequenceEqual(new uint[] { 0xFFFFFFF8, 0xFFFFFFFC, 0, 4 })
            && wrap.Pops.Select(pop => pop.StackPointerAfter)
                .SequenceEqual(new uint[] { 0xFFFFFFFC, 0, 4, 8 })
            && wrap.CurrentESPAtStop == 8
            && wrap.RegistersAtStop is {
                ECX: 0,
                EDI: 0xFFFFFFFF,
                ESI: 0,
                EBX: 0xFFFFFFFF
            }
            && wrap.ExactCleanCompletion
            && zeroESP.Reads.Select(read => read.Address)
                .SequenceEqual(new uint[] { 0, 4, 8, 0xC })
            && zeroESP.CurrentESPAtStop == 0x10
            && zeroESP.RegistersAtStop is { ECX: 0, EDI: 0, ESI: 0, EBX: 0 }
            && zeroESP.CaptureStates.FreshPostCallESP
                == NetworkDispatcherRegisterRestoreCaptureState.Captured
            && zeroESP.CaptureStates.FreshPopEBXDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.Captured);

        NetworkDispatcherRegisterRestorePlan missingESP = Project(
            esp: null,
            ecx: 1,
            edi: 2,
            esi: 3,
            ebx: 4);
        check("Missing fresh ESP stops at C0 without attempting a stack read",
            missingESP.Outcome
                == NetworkDispatcherRegisterRestoreOutcome.MissingFreshPostCallESP
            && missingESP.Reads.IsEmpty
            && missingESP.Pops.IsEmpty
            && missingESP.ReachedInstructionRawFileOffsets.IsEmpty
            && missingESP.InitialESP is null
            && missingESP.CurrentESPAtStop is null
            && missingESP.RegistersAtStop is { ECX: null, EDI: null, ESI: null, EBX: null }
            && missingESP.CaptureStates.FreshPostCallESP
                == NetworkDispatcherRegisterRestoreCaptureState.Missing
            && missingESP.CaptureStates.FreshPopECXDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.SuppliedUnused
            && missingESP.CaptureStates.FreshPopEBXDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.SuppliedUnused
            && missingESP.FlagsAtStop == missingESP.ParentPlan.FlagsAtStop
            && missingESP.StopAtRawFileOffsetExclusive == 0x00BBDEC0
            && !missingESP.ExactBoundedCompletion
            && !missingESP.ExactCleanCompletion);

        NetworkDispatcherRegisterRestorePlan[] missingEach =
        [
            Project(ecx: null),
            Project(edi: null),
            Project(esi: null),
            Project(ebx: null)
        ];
        NetworkDispatcherRegisterRestoreOutcome[] expectedMissingOutcomes =
        [
            NetworkDispatcherRegisterRestoreOutcome.MissingFreshPopECXDWORD,
            NetworkDispatcherRegisterRestoreOutcome.MissingFreshPopEDIDWORD,
            NetworkDispatcherRegisterRestoreOutcome.MissingFreshPopESIDWORD,
            NetworkDispatcherRegisterRestoreOutcome.MissingFreshPopEBXDWORD
        ];
        bool missingStagesCorrect = true;
        for (int index = 0; index < missingEach.Length; index++)
        {
            NetworkDispatcherRegisterRestorePlan missing = missingEach[index];
            uint[] addresses = { 0x1000, 0x1004, 0x1008, 0x100C };
            NetworkDispatcherRegisterRestoreCaptureState[] captureStates =
            [
                missing.CaptureStates.FreshPopECXDWORD,
                missing.CaptureStates.FreshPopEDIDWORD,
                missing.CaptureStates.FreshPopESIDWORD,
                missing.CaptureStates.FreshPopEBXDWORD
            ];
            missingStagesCorrect &=
                missing.Outcome == expectedMissingOutcomes[index]
                && missing.Reads.Length == index + 1
                && missing.Reads[^1].Address == addresses[index]
                && missing.Reads[^1].Attempted
                && !missing.Reads[^1].Captured
                && !missing.Reads[^1].Committed
                && missing.Pops.Length == index + 1
                && !missing.Pops[^1].StackReadCaptured
                && !missing.Pops[^1].RegisterValueDerivedFromRead
                && !missing.Pops[^1].ESPAdvancedInProjection
                && missing.Pops[^1].StackPointerBefore == addresses[index]
                && missing.Pops[^1].StackPointerAfter == addresses[index]
                && missing.CurrentESPAtStop == addresses[index]
                && missing.ReachedInstructionRawFileOffsets.Length == index + 1
                && missing.StopAtRawFileOffsetExclusive
                    == (uint)(0x00BBDEC0 + index)
                && missing.FlagsAtStop == missing.ParentPlan.FlagsAtStop
                && !missing.ExactBoundedCompletion
                && !missing.ExactCleanCompletion;

            for (int earlier = 0; earlier < index; earlier++)
            {
                missingStagesCorrect &=
                    captureStates[earlier]
                        == NetworkDispatcherRegisterRestoreCaptureState.Captured;
            }

            missingStagesCorrect &=
                captureStates[index] == NetworkDispatcherRegisterRestoreCaptureState.Missing;
            for (int later = index + 1; later < captureStates.Length; later++)
            {
                missingStagesCorrect &= ValueForPop(missing.Capture, later).HasValue
                    ? captureStates[later]
                        == NetworkDispatcherRegisterRestoreCaptureState.SuppliedUnused
                    : captureStates[later]
                        == NetworkDispatcherRegisterRestoreCaptureState.Unreached;
            }
        }

        check("Every missing DWORD attempts exactly its own read, retains earlier POP state, and leaves later captures unused",
            missingStagesCorrect
            && missingEach[0].CurrentESPAtStop == 0x1000
            && missingEach[0].RegistersAtStop is { ECX: null, EDI: null, ESI: null, EBX: null }
            && missingEach[1].CurrentESPAtStop == 0x1004
            && missingEach[1].RegistersAtStop.ECX == 0x11111111
            && missingEach[1].RegistersAtStop.EDI is null
            && missingEach[2].CurrentESPAtStop == 0x1008
            && missingEach[2].RegistersAtStop is {
                ECX: 0x11111111, EDI: 0x22222222, ESI: null, EBX: null
            }
            && missingEach[3].CurrentESPAtStop == 0x100C
            && missingEach[3].RegistersAtStop is {
                ECX: 0x11111111, EDI: 0x22222222, ESI: 0x33333333, EBX: null
            });

        NetworkDispatcherRegisterRestorePlan dirtyParent = Project(
            collectionEntry: 0,
            esp: 0x1000,
            ecx: 1,
            edi: 2,
            esi: 3,
            ebx: 4);
        check("A rejected parent forwards its actual bound and leaves child captures unused",
            !dirtyParent.ChildGateAdmitted
            && dirtyParent.Outcome
                == NetworkDispatcherRegisterRestoreOutcome.ParentHandoffNotAdmitted
            && dirtyParent.EntryRawFileOffset
                == dirtyParent.ParentPlan.StopAtRawFileOffsetExclusive
            && dirtyParent.StopAtRawFileOffsetExclusive
                == dirtyParent.ParentPlan.StopAtRawFileOffsetExclusive
            && dirtyParent.ExactBoundedCompletion
                == dirtyParent.ParentPlan.ExactBoundedCompletion
            && dirtyParent.Reads.IsEmpty
            && dirtyParent.Pops.IsEmpty
            && dirtyParent.CaptureStates.FreshPostCallESP
                == NetworkDispatcherRegisterRestoreCaptureState.SuppliedUnused
            && dirtyParent.CaptureStates.FreshPopECXDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.SuppliedUnused);

        NetworkDispatcherRegisterRestorePlan parentWithoutCaptures = Project(
            collectionEntry: 0,
            esp: null,
            ecx: null,
            edi: null,
            esi: null,
            ebx: null);
        check("Unreached captures remain distinct from supplied-but-unused captures",
            parentWithoutCaptures.CaptureStates.FreshPostCallESP
                == NetworkDispatcherRegisterRestoreCaptureState.Unreached
            && parentWithoutCaptures.CaptureStates.FreshPopECXDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.Unreached
            && parentWithoutCaptures.CaptureStates.FreshPopEBXDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.Unreached
            && parentWithoutCaptures.Reads.IsEmpty
            && parentWithoutCaptures.Pops.IsEmpty);

        NetworkDispatcherRegisterRestorePlan reprojection =
            NetworkDispatcherRegisterRestoreProjection.Project(normal.Capture);
        check("The parent effect prefix and child effect order are preserved semantically",
            ParentEffectPrefixPreserved(normal)
            && normal.OrderedEffects.Length
                > normal.ParentPlan.OrderedEffects.Length
            && normal.OrderedEffects.Take(normal.ParentPlan.OrderedEffects.Length)
                .All(effect => effect.ParentEffect.HasValue)
            && normal.OrderedEffects
                .Skip(normal.ParentPlan.OrderedEffects.Length)
                .Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    NetworkDispatcherRegisterRestoreOrderedEffectKind
                        .FreshPostCallESPObservation,
                    NetworkDispatcherRegisterRestoreOrderedEffectKind.PopStackDWORDRead,
                    NetworkDispatcherRegisterRestoreOrderedEffectKind
                        .LogicalPopRegisterAndESPProjection,
                    NetworkDispatcherRegisterRestoreOrderedEffectKind.PopStackDWORDRead,
                    NetworkDispatcherRegisterRestoreOrderedEffectKind
                        .LogicalPopRegisterAndESPProjection,
                    NetworkDispatcherRegisterRestoreOrderedEffectKind.PopStackDWORDRead,
                    NetworkDispatcherRegisterRestoreOrderedEffectKind
                        .LogicalPopRegisterAndESPProjection,
                    NetworkDispatcherRegisterRestoreOrderedEffectKind.PopStackDWORDRead,
                    NetworkDispatcherRegisterRestoreOrderedEffectKind
                        .LogicalPopRegisterAndESPProjection
                })
            && normal.OrderedEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(reprojection.OrderedEffects.Select(effect => effect.RawFileOffset))
            && normal.OrderedEffects.Select(effect => effect.Kind)
                .SequenceEqual(reprojection.OrderedEffects.Select(effect => effect.Kind))
            && normal.Reads.Select(read => (read.RawFileOffset, read.Address, read.Value))
                .SequenceEqual(reprojection.Reads.Select(read =>
                    (read.RawFileOffset, read.Address, read.Value)))
            && reprojection.StopAtRawFileOffsetExclusive == 0x00BBDEC4
            && reprojection.ExactCleanCompletion);
    }

    private static NetworkDispatcherRegisterRestorePlan Project(
        uint? esp = 0x1000,
        uint? ecx = 0x11111111,
        uint? edi = 0x22222222,
        uint? esi = 0x33333333,
        uint? ebx = 0x44444444,
        uint collectionEntry = 1) =>
        NetworkDispatcherRegisterRestoreProjection.Project(new(
            ParentCapture(collectionEntry),
            esp,
            ecx,
            edi,
            esi,
            ebx));

    private static NetworkDispatcherFinalReturnSehStoreCapture ParentCapture(
        uint collectionEntry)
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
        return new(
            finalCall,
            FreshIndirectTargetDWORD: 0x11223344,
            OpaqueCallReturnedNormally: true,
            FreshPostCallEBP: 0x9010,
            FreshLocalMinusCDWORD: 0xAABBCCDD);
    }

    private static uint? ValueForPop(
        NetworkDispatcherRegisterRestoreCapture capture,
        int index) =>
        index switch
        {
            0 => capture.FreshPopECXDWORD,
            1 => capture.FreshPopEDIDWORD,
            2 => capture.FreshPopESIDWORD,
            3 => capture.FreshPopEBXDWORD,
            _ => null
        };

    private static bool ParentEffectPrefixPreserved(
        NetworkDispatcherRegisterRestorePlan plan)
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
                    != NetworkDispatcherRegisterRestoreOrderedEffectKind
                        .ParentFinalReturnSehStoreEffect)
            {
                return false;
            }
        }

        return true;
    }
}

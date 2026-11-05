using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherReturnTransferChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("The dispatcher RET 4 boundary and bytes are exact",
            NetworkDispatcherReturnTransferProjection.EntryRawFileOffset == 0x00BBDEC7
            && NetworkDispatcherReturnTransferProjection.RET4RawFileOffset == 0x00BBDEC7
            && NetworkDispatcherReturnTransferProjection.RET4InstructionByteLength == 3
            && NetworkDispatcherReturnTransferProjection.CapturedWindowBytes == "C20400"
            && Convert.ToHexString(Convert.FromHexString(
                NetworkDispatcherReturnTransferProjection.CapturedWindowBytes)) == "C20400"
            && NetworkDispatcherReturnTransferProjection.RET4InstructionBytes.SequenceEqual(
                new byte[] { 0xC2, 0x04, 0x00 })
            && NetworkDispatcherReturnTransferProjection.DwordWidth == 4
            && NetworkDispatcherReturnTransferProjection.ReturnTargetStackOffset == 0
            && NetworkDispatcherReturnTransferProjection.ReturnAddressBytes == 4
            && NetworkDispatcherReturnTransferProjection.ArgumentBytes == 4
            && !NetworkDispatcherReturnTransferProjection.ExecutesNativeInstructions);

        NetworkDispatcherReturnTransferPlan zeroTarget = Project(
            returnTarget: 0,
            savedEBP: 0xA1A1A1A1);
        check("A captured zero is a valid runtime target and uses the original post-call EBP stack",
            zeroTarget.ParentPlan.ExactCleanCompletion
            && zeroTarget.ParentPlan.ESPAtStop == 0x2004
            && zeroTarget.ParentPlan.EBPAtStop == 0xA1A1A1A1
            && zeroTarget.ParentPlan.EBPAtStop != zeroTarget.ParentPlan.EBPSourceForReset
            && zeroTarget.Outcome
                == NetworkDispatcherReturnTransferOutcome.BoundedAfterRET4Transfer
            && zeroTarget.Disposition
                == NetworkDispatcherReturnTransferDisposition.BoundedAfterRET4Transfer
            && zeroTarget.ReturnTargetCaptureState
                == NetworkDispatcherReturnTransferCaptureState.Captured
            && zeroTarget.ReturnTargetReadAddress == 0x2004
            && zeroTarget.Reads is [
                { InstructionRawFileOffset: 0x00BBDEC7, Address: 0x2004, Width: 4,
                    Attempted: true, Captured: true, Value: 0, Committed: false }
            ]
            && zeroTarget.Transfer is {
                TargetVirtualAddress: 0,
                ESPBeforeRET: 0x2004,
                ESPAfterRET: 0x200C,
                ReturnAddressBytes: 4,
                ArgumentBytes: 4,
                InstructionRawFileOffset: 0x00BBDEC7
            }
            && zeroTarget.SelectedDestinationVirtualAddress == 0
            && zeroTarget.StopAtRawFileOffsetExclusive is null
            && zeroTarget.LastReachedRawFileOffset == 0x00BBDEC7
            && zeroTarget.ReachedInstructionRawFileOffsets
                .SequenceEqual(new uint[] { 0x00BBDEC7 })
            && zeroTarget.ReturnTransferBoundaryReached
            && zeroTarget.ExactBoundedCompletion
            && zeroTarget.ExactCleanCompletion
            && zeroTarget.LogicalRET4StackAdjustmentProjected
            && !zeroTarget.NativeInstructionsExecuted
            && !zeroTarget.NativeRET4Executed
            && !zeroTarget.NativeMemoryWritesCommitted
            && !zeroTarget.ReturnTargetEntered
            && !zeroTarget.ReturnTargetIsRawFileOffset
            && !zeroTarget.EAXReturnValueInferred);

        NetworkDispatcherReturnTransferPlan maximumTarget = Project(
            returnTarget: uint.MaxValue,
            savedEBP: 0xB2B2B2B2);
        check("A maximum DWORD remains an unvalidated scalar destination",
            maximumTarget.SelectedDestinationVirtualAddress == uint.MaxValue
            && maximumTarget.Transfer?.TargetVirtualAddress == uint.MaxValue
            && maximumTarget.ExactCleanCompletion
            && !maximumTarget.ReturnTargetEntered
            && !maximumTarget.ReturnTargetIsRawFileOffset
            && !maximumTarget.EAXReturnValueInferred);

        NetworkDispatcherReturnTransferPlan missingTarget = Project(
            returnTarget: null,
            savedEBP: 0xC3C3C3C3);
        check("A missing fresh target stops at C7, preserves parent flags, and does not complete RET4",
            missingTarget.ParentPlan.BoundedCompletionBoundaryReached
            && missingTarget.Outcome
                == NetworkDispatcherReturnTransferOutcome.MissingFreshReturnTargetDWORD
            && missingTarget.Disposition
                == NetworkDispatcherReturnTransferDisposition.BoundedBeforeRET4
            && missingTarget.ProjectionEntered
            && missingTarget.ReturnTargetCaptureState
                == NetworkDispatcherReturnTransferCaptureState.Missing
            && missingTarget.Reads is [
                { InstructionRawFileOffset: 0x00BBDEC7, Address: 0x2004, Width: 4,
                    Attempted: true, Captured: false, Value: null, Committed: false }
            ]
            && missingTarget.ReturnTargetReadAddress == 0x2004
            && missingTarget.ReachedInstructionRawFileOffsets.IsEmpty
            && missingTarget.LastReachedRawFileOffset
                == missingTarget.ParentPlan.ReachedInstructionRawFileOffsets
                    .Select(offset => (uint?)offset).LastOrDefault()
            && missingTarget.StopAtRawFileOffsetExclusive == 0x00BBDEC7
            && missingTarget.SelectedDestinationVirtualAddress is null
            && missingTarget.Transfer is null
            && missingTarget.ESPBeforeRET == 0x2004
            && missingTarget.ESPAfterRET is null
            && !missingTarget.ReturnTransferBoundaryReached
            && !missingTarget.ExactBoundedCompletion
            && !missingTarget.ExactCleanCompletion
            && missingTarget.FlagsAtStop == missingTarget.ParentPlan.FlagsAtStop
            && missingTarget.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            });

        NetworkDispatcherReturnTransferPlan wrapsFromFC = Project(
            returnTarget: 0x10101010,
            postCallEBP: 0xFFFFFFF8,
            savedEBP: 1);
        NetworkDispatcherReturnTransferPlan wrapsFromZero = Project(
            returnTarget: 0x20202020,
            postCallEBP: 0xFFFFFFFC,
            savedEBP: 2);
        check("RET 4 stack adjustment wraps from both DWORD boundary values",
            wrapsFromFC.ParentPlan.ESPAtStop == 0xFFFFFFFC
            && wrapsFromFC.Transfer?.ESPBeforeRET == 0xFFFFFFFC
            && wrapsFromFC.Transfer?.ESPAfterRET == 4
            && wrapsFromZero.ParentPlan.ESPAtStop == 0
            && wrapsFromZero.Transfer?.ESPBeforeRET == 0
            && wrapsFromZero.Transfer?.ESPAfterRET == 8);

        NetworkDispatcherReturnTransferPlan aliasedRead = Project(
            returnTarget: 0xC20400,
            postCallEBP: 0x1008,
            savedEBP: 0xEEEEEEEE);
        check("The fresh return read is independent when its address aliases an earlier POP",
            aliasedRead.ParentPlan.ESPAtStop == 0x100C
            && aliasedRead.ParentPlan.EBPAtStop == 0xEEEEEEEE
            && aliasedRead.ParentPlan.EBPAtStop != 0x1008
            && aliasedRead.ParentPlan.ParentPlan.Reads.Any(read =>
                read.Address == 0x100C && read.Value == 0x44444444)
            && aliasedRead.ReturnTargetReadAddress == 0x100C
            && aliasedRead.Reads.Single().Value == 0xC20400
            && aliasedRead.SelectedDestinationVirtualAddress == 0xC20400
            && aliasedRead.Transfer?.ESPBeforeRET == 0x100C
            && aliasedRead.Transfer?.ESPAfterRET == 0x1014);

        NetworkDispatcherReturnTransferCapture originalCapture = Capture(
            returnTarget: 0,
            savedEBP: 0x33333333);
        NetworkDispatcherReturnTransferPlan original =
            NetworkDispatcherReturnTransferProjection.Project(originalCapture);
        NetworkDispatcherReturnTransferPlan reprojected =
            NetworkDispatcherReturnTransferProjection.Project(
                originalCapture with { FreshReturnTargetDWORD = 0x87654321 });
        check("Immutable captures can be reprojected without changing prior reads or transfers",
            original.Capture.FreshReturnTargetDWORD == 0
            && original.Reads.Single().Value == 0
            && original.SelectedDestinationVirtualAddress == 0
            && reprojected.Capture.FreshReturnTargetDWORD == 0x87654321
            && reprojected.Reads.Single().Value == 0x87654321
            && reprojected.SelectedDestinationVirtualAddress == 0x87654321
            && original.ParentPlan.Reads.SequenceEqual(reprojected.ParentPlan.Reads)
            && original.ParentPlan.Pops.SequenceEqual(reprojected.ParentPlan.Pops)
            && original.ESPAfterRET == reprojected.ESPAfterRET);

        check("Ordered effects retain the parent prefix before the fresh read and logical transfer",
            ParentEffectPrefixPreserved(zeroTarget)
            && zeroTarget.OrderedEffects.Length
                == zeroTarget.ParentPlan.OrderedEffects.Length + 2
            && zeroTarget.OrderedEffects
                .Skip(zeroTarget.ParentPlan.OrderedEffects.Length)
                .Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    NetworkDispatcherReturnTransferOrderedEffectKind.ReturnTargetDWORDRead,
                    NetworkDispatcherReturnTransferOrderedEffectKind.LogicalRET4Transfer
                })
            && zeroTarget.OrderedEffects[^2].Read == zeroTarget.Reads[0]
            && zeroTarget.OrderedEffects[^1].Transfer == zeroTarget.Transfer
            && zeroTarget.OrderedEffects.Select(effect => effect.Sequence)
                .SequenceEqual(Enumerable.Range(0, zeroTarget.OrderedEffects.Length))
            && zeroTarget.Reads.All(read => !read.Committed));

        NetworkDispatcherReturnTransferPlan rejectedParent = Project(
            returnTarget: 0xC20400,
            collectionEntry: 0);
        check("A rejected parent forwards its actual boundary and leaves the target unused",
            !rejectedParent.ProjectionEntered
            && rejectedParent.Outcome
                == NetworkDispatcherReturnTransferOutcome.ParentHandoffNotAdmitted
            && rejectedParent.ReturnTargetCaptureState
                == NetworkDispatcherReturnTransferCaptureState.SuppliedUnused
            && rejectedParent.Reads.IsEmpty
            && rejectedParent.Transfer is null
            && rejectedParent.EntryRawFileOffset
                == rejectedParent.ParentPlan.StopAtRawFileOffsetExclusive
            && rejectedParent.StopAtRawFileOffsetExclusive
                == rejectedParent.ParentPlan.StopAtRawFileOffsetExclusive
            && rejectedParent.LastReachedRawFileOffset
                == rejectedParent.ParentPlan.ReachedInstructionRawFileOffsets
                    .Select(offset => (uint?)offset).LastOrDefault()
            && rejectedParent.FlagsAtStop == rejectedParent.ParentPlan.FlagsAtStop
            && rejectedParent.ExactBoundedCompletion
                == rejectedParent.ParentPlan.ExactBoundedCompletion
            && !rejectedParent.ExactCleanCompletion);
    }

    private static NetworkDispatcherReturnTransferPlan Project(
        uint? returnTarget,
        uint? postCallEBP = 0x2000,
        uint? savedEBP = 0xAABBCCDD,
        uint collectionEntry = 4272) =>
        NetworkDispatcherReturnTransferProjection.Project(Capture(
            returnTarget,
            postCallEBP,
            savedEBP,
            collectionEntry));

    private static NetworkDispatcherReturnTransferCapture Capture(
        uint? returnTarget,
        uint? postCallEBP = 0x2000,
        uint? savedEBP = 0xAABBCCDD,
        uint collectionEntry = 4272)
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
        var registerRestore = new NetworkDispatcherRegisterRestoreCapture(
            finalReturn,
            FreshPostCallESP: 0x1000,
            FreshPopECXDWORD: 0x11111111,
            FreshPopEDIDWORD: 0x22222222,
            FreshPopESIDWORD: 0x33333333,
            FreshPopEBXDWORD: 0x44444444);
        var frameReset = new NetworkDispatcherFrameResetCapture(
            registerRestore,
            FreshSavedEBPDWORD: savedEBP);
        return new(frameReset, returnTarget);
    }

    private static bool ParentEffectPrefixPreserved(
        NetworkDispatcherReturnTransferPlan plan)
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
                    != NetworkDispatcherReturnTransferOrderedEffectKind.ParentFrameResetEffect)
            {
                return false;
            }
        }

        return true;
    }
}

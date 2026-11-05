using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherCellReturnAddressGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        byte[] bytes = ParseBytes(
            NetworkDispatcherCellReturnAddressGateProjection.CapturedWindowBytes);
        check("The cell-return address setup bytes and instruction offsets are pinned",
            string.Join(" ", bytes.Select(value => value.ToString("X2")))
                == "89 45 F0 8D 4E 68 8D 45 F0 3B C1 74 0B"
            && bytes.Length == 13
            && NetworkDispatcherCellReturnAddressGateProjection.OpaqueCellCallBoundaryRawFileOffset
                == 0x00BBDE86
            && NetworkDispatcherCellReturnAddressGateProjection.StackStoreRawFileOffset
                == 0x00BBDE8C
            && NetworkDispatcherCellReturnAddressGateProjection.LoadECXRawFileOffset
                == 0x00BBDE8F
            && NetworkDispatcherCellReturnAddressGateProjection.LoadEAXRawFileOffset
                == 0x00BBDE92
            && NetworkDispatcherCellReturnAddressGateProjection.CompareRawFileOffset
                == 0x00BBDE95
            && NetworkDispatcherCellReturnAddressGateProjection.EqualBranchRawFileOffset
                == 0x00BBDE97
            && NetworkDispatcherCellReturnAddressGateProjection.FallthroughDestinationRawFileOffset
                == 0x00BBDE99
            && NetworkDispatcherCellReturnAddressGateProjection.BranchTargetRawFileOffset
                == 0x00BBDEA4
            && NetworkDispatcherCellReturnAddressGateProjection.StackDisplacement == 0x10
            && NetworkDispatcherCellReturnAddressGateProjection.FieldOffsetFromESI == 0x68
            && !NetworkDispatcherCellReturnAddressGateProjection.ExecutesNativeInstructions);

        NetworkDispatcherCellReturnAddressGatePlan wrappedEqual = Project(
            freshEax: 0,
            freshEbp: 0x10,
            freshEsi: 0xFFFFFF98);
        check("A zero returned EAX is stored independently and both wrapped addresses compare equal",
            wrappedEqual.ChildGateAdmitted
            && wrappedEqual.OpaqueCellReturnedNormally == true
            && wrappedEqual.StackStoreAttempted
            && wrappedEqual.StackStoreCaptured
            && wrappedEqual.StackStoreCommitted
            && wrappedEqual.StackStoreAddress == 0
            && wrappedEqual.StackStoreValue == 0
            && wrappedEqual.Capture.FreshPostCellEAX == 0
            && wrappedEqual.ResultEAX == 0
            && wrappedEqual.FieldAddress == 0
            && wrappedEqual.ComparedEAX == 0
            && wrappedEqual.ComparedECX == 0
            && wrappedEqual.BranchTaken == true
            && wrappedEqual.SelectedDestinationRawFileOffset
                == NetworkDispatcherCellReturnAddressGateProjection.BranchTargetRawFileOffset
            && wrappedEqual.FlagsAtStop is {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && wrappedEqual.StopAtRawFileOffsetExclusive
                == NetworkDispatcherCellReturnAddressGateProjection.BranchTargetRawFileOffset
            && wrappedEqual.ChildGateReachedEndpoint
            && wrappedEqual.ExactBoundedCompletion
            && wrappedEqual.ExactCleanCompletion
            && !wrappedEqual.BranchTargetEntered
            && !wrappedEqual.FallthroughDestinationEntered
            && !wrappedEqual.CellTargetReadAttempted
            && !wrappedEqual.CellCallAttempted
            && wrappedEqual.ChildReadsEmpty);

        NetworkDispatcherCellReturnAddressGatePlan wrappedUnequal = Project(
            freshEax: 0,
            freshEbp: 0,
            freshEsi: 0xFFFFFF98);
        check("EBP-0x10 wraps to FFFFFFF0 and unequal CMP flags select the unentered fallthrough",
            wrappedUnequal.StackStoreAddress == 0xFFFFFFF0
            && wrappedUnequal.FieldAddress == 0
            && wrappedUnequal.Capture.FreshPostCellEAX == 0
            && wrappedUnequal.ResultEAX == 0xFFFFFFF0
            && wrappedUnequal.ResultEAX != wrappedUnequal.Capture.FreshPostCellEAX
            && wrappedUnequal.BranchTaken == false
            && wrappedUnequal.SelectedDestinationRawFileOffset
                == NetworkDispatcherCellReturnAddressGateProjection.FallthroughDestinationRawFileOffset
            && wrappedUnequal.FlagsAtStop is {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && !wrappedUnequal.FallthroughDestinationEntered
            && wrappedUnequal.ChildGateReachedEndpoint
            && wrappedUnequal.ExactCleanCompletion);

        NetworkDispatcherCellReturnAddressGatePlan auxiliaryBorrow = Project(
            freshEbp: 0x10000010,
            freshEsi: 0xFFFFFF99);
        NetworkDispatcherCellReturnAddressGatePlan signedOverflow = Project(
            freshEbp: 0x8000000F,
            freshEsi: 0xFFFFFF97);
        check("CMP models all six 32-bit flags, including auxiliary borrow and signed overflow",
            auxiliaryBorrow.ResultEAX == 0x10000000
            && auxiliaryBorrow.FieldAddress == 1
            && auxiliaryBorrow.FlagsAtStop is {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && signedOverflow.ResultEAX == 0x7FFFFFFF
            && signedOverflow.FieldAddress == 0xFFFFFFFF
            && signedOverflow.FlagsAtStop is {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && signedOverflow.BranchTaken == false);

        NetworkDispatcherCellReturnAddressGatePlan missingEBP = Project(
            freshEax: 0,
            freshEbp: null,
            freshEsi: 0x3000);
        check("Missing EBP attempts the store at an unknown address without inventing a write",
            missingEBP.Outcome
                == NetworkDispatcherCellReturnAddressGateOutcome.MissingFreshPostCellEBP
            && missingEBP.ReachedInstructionRawFileOffsets is [
                NetworkDispatcherCellReturnAddressGateProjection.StackStoreRawFileOffset]
            && missingEBP.StackStoreAttempted
            && !missingEBP.StackStoreCaptured
            && !missingEBP.StackStoreCommitted
            && missingEBP.StackStoreAddress is null
            && missingEBP.StackStoreValue is null
            && missingEBP.OrderedEffects[^1] is {
                Kind: NetworkDispatcherCellReturnAddressGateOrderedEffectKind.FreshPostCellStackStore,
                Address: null,
                Attempted: true,
                Committed: false
            }
            && missingEBP.CaptureStates.FreshPostCellEBP
                == NetworkDispatcherCellReturnAddressGateCaptureState.Missing
            && missingEBP.CaptureStates.FreshPostCellEAX
                == NetworkDispatcherCellReturnAddressGateCaptureState.SuppliedUnused
            && missingEBP.CaptureStates.FreshPostCellESI
                == NetworkDispatcherCellReturnAddressGateCaptureState.SuppliedUnused
            && missingEBP.ResultEAX is null
            && !missingEBP.ExactBoundedCompletion
            && missingEBP.FlagsAtStop == missingEBP.FlagsAfterCellReturn
            && missingEBP.StopAtRawFileOffsetExclusive
                == NetworkDispatcherCellReturnAddressGateProjection.StackStoreRawFileOffset);

        NetworkDispatcherCellReturnAddressGatePlan missingEAX = Project(
            freshEax: null,
            freshEbp: 0x3010,
            freshEsi: 0x3000);
        NetworkDispatcherCellReturnAddressGatePlan missingESI = Project(
            freshEbp: 0x3010,
            freshEsi: null);
        check("Missing EAX leaves the derived store address uncommitted; missing ESI preserves the committed store",
            missingEAX.StackStoreAttempted
            && missingEAX.StackStoreAddress == 0x3000
            && !missingEAX.ExactBoundedCompletion
            && !missingEAX.StackStoreCaptured
            && !missingEAX.StackStoreCommitted
            && missingEAX.OrderedEffects[^1] is {
                Address: 0x3000,
                Value: null,
                Attempted: true,
                Committed: false
            }
            && missingEAX.CaptureStates.FreshPostCellEAX
                == NetworkDispatcherCellReturnAddressGateCaptureState.Missing
            && missingEAX.CaptureStates.FreshPostCellESI
                == NetworkDispatcherCellReturnAddressGateCaptureState.SuppliedUnused
            && missingEAX.StopAtRawFileOffsetExclusive
                == NetworkDispatcherCellReturnAddressGateProjection.StackStoreRawFileOffset
            && missingESI.StackStoreCommitted
            && !missingESI.ExactBoundedCompletion
            && missingESI.StackStoreAddress == 0x3000
            && missingESI.StackStoreValue == 0x12345678
            && missingESI.CaptureStates.FreshPostCellESI
                == NetworkDispatcherCellReturnAddressGateCaptureState.Missing
            && missingESI.FieldAddress is null
            && missingESI.ResultEAX is null
            && missingESI.OrderedEffects[^1] is {
                Kind: NetworkDispatcherCellReturnAddressGateOrderedEffectKind.LoadEffectiveAddressECX,
                Address: null,
                Attempted: true,
                Committed: false
            }
            && missingESI.ChildReadsEmpty
            && missingESI.StopAtRawFileOffsetExclusive
                == NetworkDispatcherCellReturnAddressGateProjection.LoadECXRawFileOffset
            && missingESI.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            });

        NetworkDispatcherCellReturnAddressGatePlan pending = Project(
            returnedNormally: null,
            freshEax: 1,
            freshEbp: 0x3010,
            freshEsi: 0x3000);
        NetworkDispatcherCellReturnAddressGatePlan didNotReturn = Project(
            returnedNormally: false,
            freshEax: 1,
            freshEbp: 0x3010,
            freshEsi: 0x3000);
        check("Pending and false cell returns retain parent CMP flags and do not complete the child",
            pending.ChildGateAdmitted
            && pending.Outcome
                == NetworkDispatcherCellReturnAddressGateOutcome.MissingOpaqueCellReturnStatus
            && didNotReturn.Outcome
                == NetworkDispatcherCellReturnAddressGateOutcome.OpaqueCellDidNotReturnNormally
            && pending.StopAtRawFileOffsetExclusive
                == NetworkDispatcherCellReturnAddressGateProjection.OpaqueCellCallBoundaryRawFileOffset
            && didNotReturn.StopAtRawFileOffsetExclusive
                == pending.StopAtRawFileOffsetExclusive
            && pending.FlagsAtStop == ToFlags(pending.ParentPlan.FlagsAtStop)
            && didNotReturn.FlagsAtStop == ToFlags(didNotReturn.ParentPlan.FlagsAtStop)
            && !pending.ExactBoundedCompletion
            && !didNotReturn.ExactBoundedCompletion
            && pending.CaptureStates.FreshPostCellEAX
                == NetworkDispatcherCellReturnAddressGateCaptureState.SuppliedUnused
            && didNotReturn.CaptureStates.FreshPostCellEBP
                == NetworkDispatcherCellReturnAddressGateCaptureState.SuppliedUnused
            && pending.ReachedInstructionRawFileOffsets.IsEmpty
            && !didNotReturn.StackStoreAttempted);

        NetworkDispatcherCellReturnAddressGatePlan rejectedBranch = Project(
            collectionEntry: 1,
            end: 0x3008,
            ebx: 0,
            start: 0x3000,
            freshEax: 1,
            freshEbp: 0x3010,
            freshEsi: 0x3000);
        NetworkDispatcherCellReturnAddressGatePlan incompleteParent = Project(
            threshold: null,
            freshEax: 1,
            freshEbp: 0x3010,
            freshEsi: 0x3000);
        check("Rejected parent branches and incomplete parents retain their own bound and unused captures",
            !rejectedBranch.ChildGateAdmitted
            && rejectedBranch.ParentPlan.BelowCountBranchTaken == true
            && rejectedBranch.StopAtRawFileOffsetExclusive
                == rejectedBranch.ParentPlan.StopAtRawFileOffsetExclusive
            && rejectedBranch.ExactBoundedCompletion
                == rejectedBranch.ParentPlan.ExactBoundedCompletion
            && !rejectedBranch.ExactCleanCompletion
            && rejectedBranch.CaptureStates.FreshPostCellEAX
                == NetworkDispatcherCellReturnAddressGateCaptureState.SuppliedUnused
            && rejectedBranch.CaptureStates.OpaqueCellReturnedNormally
                == NetworkDispatcherCellReturnAddressGateCaptureState.SuppliedUnused
            && rejectedBranch.OrderedEffects.All(effect => effect.ParentEffect.HasValue)
            && !incompleteParent.ChildGateAdmitted
            && !incompleteParent.ParentPlan.ExactBoundedCompletion
            && !incompleteParent.ExactBoundedCompletion
            && incompleteParent.StopAtRawFileOffsetExclusive
                == incompleteParent.ParentPlan.StopAtRawFileOffsetExclusive
            && incompleteParent.CaptureStates.FreshPostCellESI
                == NetworkDispatcherCellReturnAddressGateCaptureState.SuppliedUnused);

        NetworkDispatcherCellReturnAddressGatePlan normal = Project();
        NetworkDispatcherCellReturnAddressGateFlags unknownFlags = new(
            null, null, null, null, null, null);
        check("The exact parent trace is an unchanged prefix before child effects",
            ParentEffectPrefixPreserved(normal)
            && normal.OrderedEffects.Length
                > normal.ParentPlan.OrderedEffects.Length
            && normal.OrderedEffects[normal.ParentPlan.OrderedEffects.Length].RawFileOffset
                == NetworkDispatcherCellReturnAddressGateProjection.OpaqueCellCallBoundaryRawFileOffset
            && normal.OrderedEffects[^1].Kind
                == NetworkDispatcherCellReturnAddressGateOrderedEffectKind.ConditionalBranch
            && normal.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[] {
                NetworkDispatcherCellReturnAddressGateProjection.StackStoreRawFileOffset,
                NetworkDispatcherCellReturnAddressGateProjection.LoadECXRawFileOffset,
                NetworkDispatcherCellReturnAddressGateProjection.LoadEAXRawFileOffset,
                NetworkDispatcherCellReturnAddressGateProjection.CompareRawFileOffset,
                NetworkDispatcherCellReturnAddressGateProjection.EqualBranchRawFileOffset
            })
            && normal.OrderedEffects
                .Skip(normal.ParentPlan.OrderedEffects.Length)
                .Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    NetworkDispatcherCellReturnAddressGateOrderedEffectKind.OpaqueCellReturnStatus,
                    NetworkDispatcherCellReturnAddressGateOrderedEffectKind.FreshPostCellStackStore,
                    NetworkDispatcherCellReturnAddressGateOrderedEffectKind.LoadEffectiveAddressECX,
                    NetworkDispatcherCellReturnAddressGateOrderedEffectKind.LoadEffectiveAddressEAX,
                    NetworkDispatcherCellReturnAddressGateOrderedEffectKind.CompareEAXECX,
                    NetworkDispatcherCellReturnAddressGateOrderedEffectKind.ConditionalBranch
                })
            && normal.ResultEAX == 0x3000
            && normal.Capture.FreshPostCellEAX == 0x12345678
            && normal.ResultEAX != normal.Capture.FreshPostCellEAX
            && normal.FlagsAfterCellReturn == unknownFlags
            && normal.FlagsAfterStore == unknownFlags
            && normal.FlagsAfterLEAECX == unknownFlags
            && normal.FlagsAfterLEAEAX == unknownFlags
            && normal.FlagsAfterCompare == normal.FlagsAtStop
            && normal.ChildReadsEmpty
            && !normal.CellTargetReadAttempted
            && !normal.CellCallAttempted);
    }

    private static NetworkDispatcherCellReturnAddressGatePlan Project(
        bool? returnedNormally = true,
        uint? freshEax = 0x12345678,
        uint? freshEbp = 0x3010,
        uint? freshEsi = 0x2F98,
        uint collectionEntry = 1,
        uint? end = 0x3008,
        uint? ebx = 1,
        uint? start = 0x3000,
        uint? threshold = 1)
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
            FreshThresholdDWORD: threshold,
            OpaqueCallReturnedNormally: true,
            FreshPostLoggingESI: 0x2000,
            FreshEndDWORD: 0x2004,
            FreshStartDWORD: 0x2000,
            FreshReloadDWORD: 0x4000,
            FreshCollectionEntryDWORD: collectionEntry);
        var parentPreparation = new NetworkDispatcherEntryCallPreparationInputs(
            parentCollection,
            FreshEntryESP: 0x8000,
            FreshVptrDWORD: collectionEntry == 0 ? null : 0x5000);
        var parent = new NetworkDispatcherPostCallCountGateCapture(
            parentPreparation,
            VtableCallReturnedNormally: true,
            FreshPostCallESI: 0x3000,
            FreshEndDWORD: end,
            FreshPostCallEBX: ebx,
            FreshStartDWORD: start);
        return NetworkDispatcherCellReturnAddressGateProjection.Project(
            new(parent, returnedNormally, freshEax, freshEbp, freshEsi));
    }

    private static bool ParentEffectPrefixPreserved(
        NetworkDispatcherCellReturnAddressGatePlan plan)
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
                || plan.OrderedEffects[index].Address
                    != plan.ParentPlan.OrderedEffects[index].Address
                || plan.OrderedEffects[index].Width
                    != plan.ParentPlan.OrderedEffects[index].Width
                || plan.OrderedEffects[index].Value
                    != plan.ParentPlan.OrderedEffects[index].Value
                || plan.OrderedEffects[index].Attempted
                    != plan.ParentPlan.OrderedEffects[index].Attempted
                || plan.OrderedEffects[index].Committed
                    != plan.ParentPlan.OrderedEffects[index].Committed)
            {
                return false;
            }
        }

        return true;
    }

    private static NetworkDispatcherCellReturnAddressGateFlags? ToFlags(
        NetworkDispatcherPostCallCountGateFlags? flags) =>
        flags is NetworkDispatcherPostCallCountGateFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;

    private static byte[] ParseBytes(string text) =>
        Convert.FromHexString(text);
}

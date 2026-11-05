using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherPostCallCountGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherPostCallCountGatePlan Project(
            uint collectionEntry = 1,
            bool? returnedNormally = true,
            uint? esi = 0x3000,
            uint? end = 0x3008,
            uint? ebx = 0,
            uint? start = 0x3000,
            bool dirtyParent = false,
            uint? threshold = 1)
        {
            var parentLock = new NetworkDispatcherLockGateCapture(
                ResultEAX: 1,
                EntryESIReceiver: 0x1000,
                FreshCounterValue: 1,
                EntryEBP: 0x2000,
                FreshArgumentByte: 1,
                OpaqueImportReturnedNormally: dirtyParent ? true : null);
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
            return NetworkDispatcherPostCallCountGateProjection.Project(new(
                parentPreparation,
                returnedNormally,
                esi,
                end,
                ebx,
                start));
        }

        static string Normalize(string value) =>
            string.Concat(value.Where(character => !char.IsWhiteSpace(character)));

        static bool SameFlags(
            NetworkDispatcherPostCallCountGateFlags? left,
            NetworkDispatcherPostCallCountGateFlags? right) =>
            left == right;

        check("The exact post-call instruction window and bounded call anchors are captured",
            NetworkDispatcherPostCallCountGateProjection.EntryRawFileOffset == 0x00BBDE6C
            && NetworkDispatcherPostCallCountGateProjection.RawToVirtualAddressDelta
                == 0x00400C00
            && Normalize(NetworkDispatcherPostCallCountGateProjection.CapturedWindowBytes)
                == "8B86A8000000432B86A4000000C1F8023BD872BD8D5E6CFF15244DAC01"
            && NetworkDispatcherPostCallCountGateProjection.VtableCallRawFileOffset
                == 0x00BBDE6C
            && NetworkDispatcherPostCallCountGateProjection.FreshEndDWORDReadRawFileOffset
                == 0x00BBDE6F
            && NetworkDispatcherPostCallCountGateProjection.IncrementPostCallEBXRawFileOffset
                == 0x00BBDE75
            && NetworkDispatcherPostCallCountGateProjection.FreshStartDWORDReadRawFileOffset
                == 0x00BBDE76
            && NetworkDispatcherPostCallCountGateProjection.ArithmeticShiftRightRawFileOffset
                == 0x00BBDE7C
            && NetworkDispatcherPostCallCountGateProjection.CompareCountRawFileOffset
                == 0x00BBDE7F
            && NetworkDispatcherPostCallCountGateProjection.BelowCountBranchRawFileOffset
                == 0x00BBDE81
            && NetworkDispatcherPostCallCountGateProjection.BelowCountContinuationRawFileOffset
                == 0x00BBDE40
            && NetworkDispatcherPostCallCountGateProjection.PrepareOpaqueCellCallRawFileOffset
                == 0x00BBDE83
            && NetworkDispatcherPostCallCountGateProjection.OpaqueCellCallBoundaryRawFileOffset
                == 0x00BBDE86
            && NetworkDispatcherPostCallCountGateProjection.OpaqueCellVirtualAddress
                == 0x01AC4D24
            && NetworkDispatcherPostCallCountGateProjection.EndOffsetFromESI == 0xA8
            && NetworkDispatcherPostCallCountGateProjection.StartOffsetFromESI == 0xA4
            && NetworkDispatcherPostCallCountGateProjection.EBXOffsetFromESI == 0x6C
            && NetworkDispatcherPostCallCountGateProjection.ShiftCount == 2
            && !NetworkDispatcherPostCallCountGateProjection.ExecutesNativeInstructions);

        NetworkDispatcherPostCallCountGatePlan below = Project();
        check("Only a clean nonzero-entry vtable-call boundary admits this gate",
            below.ParentEntryCallPreparationPlan.ExactCleanCompletion
            && below.ParentEntryCallPreparationPlan.Outcome
                == NetworkDispatcherEntryCallPreparationOutcome.NonzeroEntryVtableCallBoundary
            && below.ParentEntryCallPreparationPlan.StopAtRawFileOffsetExclusive == 0x00BBDE6C
            && below.ChildGateAdmitted
            && below.BelowCountBranchTaken == true
            && below.SelectedDestinationRawFileOffset == 0x00BBDE40
            && below.Outcome
                == NetworkDispatcherPostCallCountGateOutcome.BelowCountContinuationBoundary
            && below.ExactBoundedCompletion
            && below.ExactCleanCompletion
            && below.CountAfterIncrement == 1
            && below.ShiftedDifference == 2
            && below.CompareFlags?.Carry == true
            && below.StopAtRawFileOffsetExclusive == 0x00BBDE40
            && !below.OpaqueCellReadAttempted
            && !below.OpaqueCellCallAttempted
            && below.Reads.Select(read => read.Kind).SequenceEqual(new[]
            {
                NetworkDispatcherPostCallCountGateReadKind.FreshEndDWORD,
                NetworkDispatcherPostCallCountGateReadKind.FreshStartDWORD
            })
            && below.OrderedEffects.Skip(
                    below.ParentEntryCallPreparationPlan.OrderedEffects.Length)
                .Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    NetworkDispatcherPostCallCountGateOrderedEffectKind.VtableCallReturnStatus,
                    NetworkDispatcherPostCallCountGateOrderedEffectKind.FreshEndDWORDRead,
                    NetworkDispatcherPostCallCountGateOrderedEffectKind.PostCallEBXIncrement,
                    NetworkDispatcherPostCallCountGateOrderedEffectKind.FreshStartDWORDRead,
                    NetworkDispatcherPostCallCountGateOrderedEffectKind.SubtractStart,
                    NetworkDispatcherPostCallCountGateOrderedEffectKind.ArithmeticShiftRight,
                    NetworkDispatcherPostCallCountGateOrderedEffectKind.CompareCount,
                    NetworkDispatcherPostCallCountGateOrderedEffectKind.ConditionalBranch
                }));

        NetworkDispatcherPostCallCountGatePlan zeroParent = Project(
            collectionEntry: 0,
            returnedNormally: true,
            esi: 0x3000,
            end: 0x3008,
            ebx: 0,
            start: 0x3000);
        check("A clean zero-entry parent stops at 5D and leaves all child captures unused",
            zeroParent.ParentEntryCallPreparationPlan.ExactCleanCompletion
            && zeroParent.ParentEntryCallPreparationPlan.Outcome
                == NetworkDispatcherEntryCallPreparationOutcome.ZeroEntryOpaqueImportBoundary
            && zeroParent.ParentEntryCallPreparationPlan.StopAtRawFileOffsetExclusive
                == 0x00BBDE5D
            && !zeroParent.ChildGateAdmitted
            && zeroParent.Outcome
                == NetworkDispatcherPostCallCountGateOutcome.ParentPreparationDidNotSelectNonzeroVtableCall
            && zeroParent.StopAtRawFileOffsetExclusive == 0x00BBDE5D
            && zeroParent.CaptureStates.VtableCallReturnedNormally
                == NetworkDispatcherPostCallCountGateCaptureState.SuppliedUnused
            && zeroParent.VtableCallReturnedNormally is null
            && zeroParent.ExactBoundedCompletion
            && !zeroParent.ExactCleanCompletion
            && zeroParent.CaptureStates.FreshPostCallESI
                == NetworkDispatcherPostCallCountGateCaptureState.SuppliedUnused
            && zeroParent.Reads.IsEmpty
            && SameFlags(zeroParent.FlagsAtStop,
                zeroParent.ParentEntryCallPreparationPlan.FlagsAtStop is { } parentFlags
                    ? new(parentFlags.Carry, parentFlags.Parity, parentFlags.Auxiliary,
                        parentFlags.Zero, parentFlags.Sign, parentFlags.Overflow)
                    : null));

        NetworkDispatcherPostCallCountGatePlan pendingCall = Project(
            returnedNormally: null,
            esi: null,
            end: null,
            ebx: null,
            start: null);
        NetworkDispatcherPostCallCountGatePlan failedCall = Project(
            returnedNormally: false,
            esi: 0x3000,
            end: 0x3008,
            ebx: 1,
            start: 0x3000);
        check("A pending or non-normal vtable call stops at 6C with parent TEST flags intact",
            pendingCall.ChildGateAdmitted
            && pendingCall.Outcome
                == NetworkDispatcherPostCallCountGateOutcome.MissingVtableCallReturnStatus
            && pendingCall.StopAtRawFileOffsetExclusive == 0x00BBDE6C
            && pendingCall.CaptureStates.VtableCallReturnedNormally
                == NetworkDispatcherPostCallCountGateCaptureState.Missing
            && failedCall.Outcome
                == NetworkDispatcherPostCallCountGateOutcome.VtableCallDidNotReturnNormally
            && failedCall.StopAtRawFileOffsetExclusive == 0x00BBDE6C
            && !pendingCall.ExactBoundedCompletion
            && !failedCall.ExactBoundedCompletion
            && SameFlags(pendingCall.FlagsAtStop,
                pendingCall.ParentEntryCallPreparationPlan.FlagsAtStop is { } pendingParentFlags
                    ? new(pendingParentFlags.Carry, pendingParentFlags.Parity,
                        pendingParentFlags.Auxiliary, pendingParentFlags.Zero,
                        pendingParentFlags.Sign, pendingParentFlags.Overflow)
                    : null)
            && SameFlags(failedCall.FlagsAtStop,
                failedCall.ParentEntryCallPreparationPlan.FlagsAtStop is { } failedParentFlags
                    ? new(failedParentFlags.Carry, failedParentFlags.Parity,
                        failedParentFlags.Auxiliary, failedParentFlags.Zero,
                        failedParentFlags.Sign, failedParentFlags.Overflow)
                    : null)
            && failedCall.CaptureStates.FreshPostCallESI
                == NetworkDispatcherPostCallCountGateCaptureState.SuppliedUnused);

        NetworkDispatcherPostCallCountGatePlan missingESI = Project(
            esi: null,
            end: 0x1234,
            ebx: 0,
            start: 0);
        NetworkDispatcherPostCallCountGatePlan missingEnd = Project(
            end: null,
            ebx: 0,
            start: 0);
        NetworkDispatcherPostCallCountGatePlan missingEBX = Project(
            end: 0x3008,
            ebx: null,
            start: 0x3000);
        check("Normal return invalidates all six flags and requires fresh ESI, end, then EBX",
            missingESI.Outcome
                == NetworkDispatcherPostCallCountGateOutcome.MissingFreshPostCallESI
            && missingESI.Reads is [{
                RawFileOffset: 0x00BBDE6F,
                Kind: NetworkDispatcherPostCallCountGateReadKind.FreshEndDWORD,
                Address: null,
                Width: 4,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false
            }]
            && missingESI.FlagsAtStop == new NetworkDispatcherPostCallCountGateFlags(
                null, null, null, null, null, null)
            && !missingESI.ExactBoundedCompletion
            && missingEnd.Outcome
                == NetworkDispatcherPostCallCountGateOutcome.MissingFreshEndDWORD
            && missingEnd.Reads.Single().Address == 0x30A8
            && missingEnd.StopAtRawFileOffsetExclusive == 0x00BBDE6F
            && !missingEnd.ExactBoundedCompletion
            && missingEBX.Outcome
                == NetworkDispatcherPostCallCountGateOutcome.MissingFreshPostCallEBX
            && missingEBX.Reads.Single().Value == 0x3008
            && missingEBX.CaptureStates.FreshStartDWORD
                == NetworkDispatcherPostCallCountGateCaptureState.SuppliedUnused
            && missingEBX.FlagsAtStop == new NetworkDispatcherPostCallCountGateFlags(
                null, null, null, null, null, null)
            && !missingEBX.ExactBoundedCompletion
            && missingEBX.StopAtRawFileOffsetExclusive == 0x00BBDE75);

        NetworkDispatcherPostCallCountGatePlan dirtyParent = Project(dirtyParent: true);
        NetworkDispatcherPostCallCountGatePlan incompleteParent = Project(threshold: null);
        check("Rejected dirty and incomplete parents retain their own bound without admitting the child",
            dirtyParent.ParentEntryCallPreparationPlan.ExactBoundedCompletion
            && !dirtyParent.ParentEntryCallPreparationPlan.ExactCleanCompletion
            && dirtyParent.StopAtRawFileOffsetExclusive
                == dirtyParent.ParentEntryCallPreparationPlan.StopAtRawFileOffsetExclusive
            && dirtyParent.ExactBoundedCompletion
            && !dirtyParent.ExactCleanCompletion
            && !dirtyParent.ChildGateAdmitted
            && !incompleteParent.ParentEntryCallPreparationPlan.ExactBoundedCompletion
            && !incompleteParent.ParentEntryCallPreparationPlan.ExactCleanCompletion
            && incompleteParent.StopAtRawFileOffsetExclusive
                == incompleteParent.ParentEntryCallPreparationPlan.StopAtRawFileOffsetExclusive
            && !incompleteParent.ExactBoundedCompletion
            && !incompleteParent.ExactCleanCompletion
            && !incompleteParent.ChildGateAdmitted);

        NetworkDispatcherPostCallCountGatePlan missingStart = Project(
            end: 0,
            ebx: 0xFFFFFFFF,
            start: null);
        check("A missing start is reached only after INC and preserves its unknown carry",
            missingStart.Outcome
                == NetworkDispatcherPostCallCountGateOutcome.MissingFreshStartDWORD
            && missingStart.CountAfterIncrement == 0
            && missingStart.IncrementFlags?.Carry is null
            && missingStart.IncrementFlags?.Zero == true
            && missingStart.IncrementFlags?.Sign == false
            && missingStart.IncrementFlags?.Auxiliary == true
            && missingStart.FlagsAtStop == missingStart.IncrementFlags
            && missingStart.Reads[^1].RawFileOffset == 0x00BBDE76
            && missingStart.Reads[^1].Address == 0x30A4
            && missingStart.StopAtRawFileOffsetExclusive == 0x00BBDE76
            && !missingStart.ExactBoundedCompletion);

        NetworkDispatcherPostCallCountGatePlan equalCount = Project(
            end: 0x3008,
            ebx: 1,
            start: 0x3000);
        NetworkDispatcherPostCallCountGatePlan wrappedEBX = Project(
            esi: 0xFFFFFF80,
            end: 0xFFFFFFFC,
            ebx: 0xFFFFFFFF,
            start: 0xFFFFFFF8);
        NetworkDispatcherPostCallCountGatePlan negativeDifference = Project(
            end: 0x1000,
            ebx: 0,
            start: 0x1004);
        NetworkDispatcherPostCallCountGatePlan overflowCount = Project(
            end: 0x3008,
            ebx: 0x7FFFFFFF,
            start: 0x3000);
        check("Unsigned CMP/JB, DWORD wraps, SAR sign extension, and LEA address wraps are preserved",
            equalCount.CountAfterIncrement == 2
            && equalCount.ShiftedDifference == 2
            && equalCount.CompareFlags?.Carry == false
            && equalCount.BelowCountBranchTaken == false
            && equalCount.Outcome
                == NetworkDispatcherPostCallCountGateOutcome.OpaqueCellCallBoundary
            && equalCount.EBXAtOpaqueCallBoundary == 0x306C
            && equalCount.StopAtRawFileOffsetExclusive == 0x00BBDE86
            && equalCount.FlagsAtStop == equalCount.CompareFlags
            && !equalCount.OpaqueCellReadAttempted
            && !equalCount.OpaqueCellCallAttempted
            && equalCount.Reads.Length == 2
            && equalCount.OrderedEffects[^1].Kind
                == NetworkDispatcherPostCallCountGateOrderedEffectKind.PrepareOpaqueCellCall
            && equalCount.OrderedEffects[^1].Address == 0x306C
            && wrappedEBX.FreshEndAddress == 0x28
            && wrappedEBX.FreshStartAddress == 0x24
            && wrappedEBX.CountAfterIncrement == 0
            && wrappedEBX.DifferenceValue == 4
            && wrappedEBX.ShiftedDifference == 1
            && wrappedEBX.BelowCountBranchTaken == true
            && overflowCount.CountAfterIncrement == 0x80000000
            && overflowCount.IncrementFlags?.Overflow == true
            && overflowCount.IncrementFlags?.Sign == true
            && overflowCount.IncrementFlags?.Auxiliary == true
            && negativeDifference.DifferenceValue == 0xFFFFFFFC
            && negativeDifference.ShiftedDifference == 0xFFFFFFFF
            && negativeDifference.ShiftFlags?.Carry == false
            && negativeDifference.ShiftFlags?.Auxiliary is null
            && negativeDifference.ShiftFlags?.Overflow is null
            && negativeDifference.ShiftFlags?.Sign == true
            && negativeDifference.BelowCountBranchTaken == true);
    }
}

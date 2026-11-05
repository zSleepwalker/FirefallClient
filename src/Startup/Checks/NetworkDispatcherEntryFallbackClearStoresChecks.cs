using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackClearStoresChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFallbackClearStoresCapture Capture(
            uint? freshParentEBP = 0x2000,
            byte? freshArgumentByte = 0,
            uint? freshPostFallbackESI = 0x1000,
            uint? operationTarget = 0x12345678,
            bool? normalReturnObserved = true,
            uint? freshPostCallESP = 0x5000,
            uint? freshPostCallEBX = 0xA5A5,
            uint? freshCounterEBX = 0x1000,
            uint? freshCounterDword = 0,
            uint? initialReturnedEAX = 0)
        {
            var callerCapture = new NetworkIoCompletionClockGateCapture(
                EntryValidated: true,
                EntryEBP: 0x2000,
                EntryFlags: null,
                FreshGlobalPointer: 0x1000,
                FreshClockField: 500,
                CachedClockTargetVirtualAddress: 0x12345678,
                ClockReturnedNormally: true,
                ClockReturnEAX: 1000,
                FreshSavedPointerRead: 0x2222,
                DispatcherReturnedNormally: null);
            var frameCapture = new NetworkDispatcherEntryFramePrefixCapture(
                callerCapture,
                0x2000,
                0x2000,
                new NetworkIoCompletionClockGateFlags(
                    Carry: true,
                    Parity: false,
                    Auxiliary: true,
                    Zero: false,
                    Sign: true,
                    Overflow: false),
                0xAABBCCDD);
            NetworkDispatcherEntryFramePrefixPlan framePlan =
                NetworkDispatcherEntryFramePrefixProjection.Project(frameCapture);
            var registerCapture = new NetworkDispatcherEntryRegisterSaveCapture(
                frameCapture,
                framePlan,
                FreshEntryEBX: 0x11111111,
                FreshEntryESI: 0x123,
                FreshEntryEDI: 0x33333333);
            var cookieCapture = new NetworkDispatcherEntryCookiePreparationCapture(
                registerCapture,
                FreshCookieGlobalDWORD: 0);
            var lockCapture = new NetworkDispatcherEntryLockPreparationCapture(
                new NetworkDispatcherEntryFsPublicationCapture(cookieCapture),
                FreshEntryECX: 0x123);
            var fallbackPushCapture = new NetworkDispatcherEntryFallbackPushCapture(
                new NetworkDispatcherEntryReturnedTestBranchCapture(
                    new NetworkDispatcherEntryImportReturnCapture(
                        lockCapture,
                        0x12345678,
                        true,
                        initialReturnedEAX)),
                freshPostCallESP,
                freshPostCallEBX);
            var fallbackImportCapture = new NetworkDispatcherEntryFallbackImportReturnCapture(
                fallbackPushCapture,
                operationTarget,
                normalReturnObserved);
            var fallbackCounterCapture = new NetworkDispatcherEntryFallbackCounterCapture(
                fallbackImportCapture,
                freshCounterEBX,
                freshCounterDword);
            var compareCapture = new NetworkDispatcherEntryFallbackArgumentCompareCapture(
                fallbackCounterCapture,
                freshParentEBP,
                freshArgumentByte);
            return new(compareCapture, freshPostFallbackESI);
        }

        static NetworkDispatcherEntryFallbackClearStoresPlan Project(
            uint? freshParentEBP = 0x2000,
            byte? freshArgumentByte = 0,
            uint? freshPostFallbackESI = 0x1000,
            uint? operationTarget = 0x12345678,
            bool? normalReturnObserved = true,
            uint? freshPostCallESP = 0x5000,
            uint? freshPostCallEBX = 0xA5A5,
            uint? freshCounterEBX = 0x1000,
            uint? freshCounterDword = 0,
            uint? initialReturnedEAX = 0) =>
            NetworkDispatcherEntryFallbackClearStoresProjection.Project(Capture(
                freshParentEBP,
                freshArgumentByte,
                freshPostFallbackESI,
                operationTarget,
                normalReturnObserved,
                freshPostCallESP,
                freshPostCallEBX,
                freshCounterEBX,
                freshCounterDword,
                initialReturnedEAX));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("The released clear-store bytes, operands, widths, VA mapping, and branch boundary match IDA",
            NetworkDispatcherEntryFallbackArgumentCompareProjection
                .CompareRawFileOffset == 0x00BBDDF9
            && NetworkDispatcherEntryFallbackArgumentCompareProjection
                .BoundaryRawFileOffsetExclusive == NetworkDispatcherEntryFallbackClearStoresProjection
                    .EntryRawFileOffset
            && NetworkDispatcherEntryFallbackArgumentCompareProjection.ArgumentByteWidth == 1
            && Normalize(NetworkDispatcherEntryFallbackArgumentCompareProjection
                    .CompareInstructionBytes) == "807D0800"
            && NetworkDispatcherEntryFallbackArgumentCompareProjection.CompareRawFileOffset
                + NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta == 0x00FBE9F9
            && NetworkDispatcherEntryFallbackClearStoresProjection.EntryRawFileOffset
                == 0x00BBDDFD
            && NetworkDispatcherEntryFallbackClearStoresProjection.FrameDwordClearRawFileOffset
                == 0x00BBDDFD
            && NetworkDispatcherEntryFallbackClearStoresProjection.ReceiverByteClearRawFileOffset
                == 0x00BBDE04
            && NetworkDispatcherEntryFallbackClearStoresProjection.BoundaryRawFileOffsetExclusive
                == 0x00BBDE08
            && NetworkDispatcherEntryFallbackClearStoresProjection.EntryRawFileOffset
                + NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta == 0x00FBE9FD
            && NetworkDispatcherEntryFallbackClearStoresProjection.ReceiverByteClearRawFileOffset
                + NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta == 0x00FBEA04
            && NetworkDispatcherEntryFallbackClearStoresProjection.BoundaryRawFileOffsetExclusive
                + NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta == 0x00FBEA08
            && NetworkDispatcherEntryFallbackClearStoresProjection.FrameDwordDisplacement == 4
            && NetworkDispatcherEntryFallbackClearStoresProjection.FrameDwordWidth == 4
            && NetworkDispatcherEntryFallbackClearStoresProjection.FrameDwordClearValue == 0
            && NetworkDispatcherEntryFallbackClearStoresProjection
                .FrameDwordClearInstructionByteLength == 7
            && Normalize(NetworkDispatcherEntryFallbackClearStoresProjection
                    .FrameDwordClearInstructionBytes) == "C745FC00000000"
            && NetworkDispatcherEntryFallbackClearStoresProjection.ReceiverByteDisplacement == 0x64
            && NetworkDispatcherEntryFallbackClearStoresProjection.ReceiverByteWidth == 1
            && NetworkDispatcherEntryFallbackClearStoresProjection.ReceiverByteClearValue == 0
            && NetworkDispatcherEntryFallbackClearStoresProjection
                .ReceiverByteClearInstructionByteLength == 4
            && Normalize(NetworkDispatcherEntryFallbackClearStoresProjection
                    .ReceiverByteClearInstructionBytes) == "C6466400"
            && Normalize(NetworkDispatcherEntryFallbackClearStoresProjection
                    .BranchInstructionBytes) == "0F84BC000000"
            && NetworkDispatcherEntryFallbackClearStoresProjection.BranchInstructionByteLength == 6
            && Normalize(NetworkDispatcherEntryFallbackClearStoresProjection
                    .CapturedWindowBytes) == "C745FC00000000C64664000F84BC000000"
            && NetworkDispatcherEntryFallbackClearStoresProjection.FrameDwordClearRawFileOffset
                + NetworkDispatcherEntryFallbackClearStoresProjection
                    .FrameDwordClearInstructionByteLength
                == NetworkDispatcherEntryFallbackClearStoresProjection
                    .ReceiverByteClearRawFileOffset
            && NetworkDispatcherEntryFallbackClearStoresProjection.ReceiverByteClearRawFileOffset
                + NetworkDispatcherEntryFallbackClearStoresProjection
                    .ReceiverByteClearInstructionByteLength
                == NetworkDispatcherEntryFallbackClearStoresProjection
                    .BoundaryRawFileOffsetExclusive
            && !NetworkDispatcherEntryFallbackClearStoresProjection.ExecutesNativeInstructions);

        NetworkDispatcherEntryFallbackClearStoresPlan regular = Project(
            freshParentEBP: 0x2000,
            freshArgumentByte: 0x80,
            freshPostFallbackESI: 0x1000);
        check("The adjacent MOV stores reuse CMP-preserved EBP, freshly capture ESI, and preserve CMP flags",
            regular.ParentPlan.ExactCleanCompletion
            && regular.ParentPlan.CompareFlags is {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && regular.Outcome
                == NetworkDispatcherEntryFallbackClearStoresOutcome.CompletedBeforeBranch
            && regular.FreshPostFallbackESICaptureState
                == NetworkDispatcherEntryFallbackClearStoresCaptureState.Captured
            && regular.FreshPostFallbackESI == 0x1000
            && regular.FreshPostFallbackESI != 0x123
            && regular.FreshParentEBP == 0x2000
            && regular.FreshParentEBP == regular.ParentPlan.FreshPostFallbackEBP
            && regular.FrameAddress == 0x1FFC
            && regular.ReceiverAddress == 0x1064
            && regular.Stores.SequenceEqual(new[]
            {
                new NetworkDispatcherEntryFallbackClearStore(
                    0x00BBDDFD,
                    NetworkDispatcherEntryFallbackClearStoreKind.FrameDwordClear,
                    0x1FFC,
                    4,
                    0,
                    true,
                    true,
                    true),
                new NetworkDispatcherEntryFallbackClearStore(
                    0x00BBDE04,
                    NetworkDispatcherEntryFallbackClearStoreKind.ReceiverByteClear,
                    0x1064,
                    1,
                    0,
                    true,
                    true,
                    true)
            })
            && regular.FrameDwordClearProjected
            && regular.ReceiverByteClearAttempted
            && regular.ReceiverByteClearProjected
            && regular.CompletedInstructionOffsets.SequenceEqual(
                regular.ParentPlan.CompletedInstructionOffsets
                    .Add(0x00BBDDFD)
                    .Add(0x00BBDE04))
            && regular.EntryRawFileOffset == 0x00BBDDFD
            && regular.StopAtRawFileOffsetExclusive == 0x00BBDE08
            && regular.FlagsAtStop == regular.ParentPlan.CompareFlags
            && regular.CompareFlagsPreserved
            && regular.ParentEBPReusedOnlyAfterCompare
            && regular.ExactBoundedCompletion
            && regular.ExactCleanCompletion
            && !regular.BranchProjected
            && !regular.BranchExecuted
            && !regular.PostCallESIForwarded
            && !regular.NativeInstructionsExecuted
            && !regular.NativeMemoryReadsExecuted
            && !regular.NativeMemoryWritesExecuted);

        NetworkDispatcherEntryFallbackClearStoresPlan zeroRegisters = Project(
            freshParentEBP: 0,
            freshArgumentByte: 0,
            freshPostFallbackESI: 0);
        check("Captured zero registers remain distinct from missing and wrap the DWORD frame address",
            zeroRegisters.Outcome
                == NetworkDispatcherEntryFallbackClearStoresOutcome.CompletedBeforeBranch
            && zeroRegisters.FreshParentEBP == 0
            && zeroRegisters.FreshPostFallbackESI == 0
            && zeroRegisters.FreshPostFallbackESICaptureState
                == NetworkDispatcherEntryFallbackClearStoresCaptureState.Captured
            && zeroRegisters.FrameAddress == 0xFFFFFFFC
            && zeroRegisters.ReceiverAddress == 0x64
            && zeroRegisters.Stores[0] is {
                Address: 0xFFFFFFFC,
                Width: 4,
                Value: 0,
                Committed: true
            }
            && zeroRegisters.Stores[1] is {
                Address: 0x64,
                Width: 1,
                Value: 0,
                Committed: true
            }
            && zeroRegisters.ParentPlan.CompareFlags is { Zero: true }
            && zeroRegisters.FlagsAtStop == zeroRegisters.ParentPlan.CompareFlags
            && zeroRegisters.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackClearStoresPlan missingESI = Project(
            freshParentEBP: 0x2000,
            freshArgumentByte: 0,
            freshPostFallbackESI: null);
        check("Missing post-call ESI keeps the frame store but cannot invent the receiver destination",
            missingESI.Outcome
                == NetworkDispatcherEntryFallbackClearStoresOutcome.MissingFreshPostFallbackESI
            && missingESI.FreshPostFallbackESICaptureState
                == NetworkDispatcherEntryFallbackClearStoresCaptureState.Missing
            && missingESI.FrameDwordClearProjected
            && missingESI.FrameAddress == 0x1FFC
            && missingESI.ReceiverByteClearAttempted
            && !missingESI.ReceiverByteClearProjected
            && missingESI.ReceiverAddress is null
            && missingESI.Stores.Length == 2
            && missingESI.Stores[1] is {
                InstructionRawFileOffset: 0x00BBDE04,
                Kind: NetworkDispatcherEntryFallbackClearStoreKind.ReceiverByteClear,
                Address: null,
                Width: 1,
                Value: 0,
                Attempted: true,
                DestinationCaptured: false,
                Committed: false
            }
            && missingESI.CompletedInstructionOffsets.SequenceEqual(
                missingESI.ParentPlan.CompletedInstructionOffsets.Add(0x00BBDDFD))
            && missingESI.StopAtRawFileOffsetExclusive == 0x00BBDE04
            && missingESI.FlagsAtStop == missingESI.ParentPlan.CompareFlags
            && !missingESI.ExactBoundedCompletion
            && !missingESI.ExactCleanCompletion
            && !missingESI.BranchProjected);

        NetworkDispatcherEntryFallbackClearStoresPlan wrappedReceiver = Project(
            freshParentEBP: 0x2000,
            freshArgumentByte: 0,
            freshPostFallbackESI: 0xFFFFFF9C);
        check("The receiver-byte effective address wraps at 32 bits",
            wrappedReceiver.ReceiverAddress == 0
            && wrappedReceiver.Stores[1] is { Address: 0, Width: 1, Value: 0, Committed: true }
            && wrappedReceiver.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackClearStoresPlan aliasedStores = Project(
            freshParentEBP: 0x1068,
            freshArgumentByte: 0x80,
            freshPostFallbackESI: 0x1000);
        check("Aliased DWORD and byte clears remain two ordered logical writes",
            aliasedStores.FrameAddress == 0x1064
            && aliasedStores.ReceiverAddress == 0x1064
            && aliasedStores.Stores.Length == 2
            && aliasedStores.Stores[0] is {
                InstructionRawFileOffset: 0x00BBDDFD,
                Kind: NetworkDispatcherEntryFallbackClearStoreKind.FrameDwordClear,
                Address: 0x1064,
                Width: 4,
                Value: 0,
                Committed: true
            }
            && aliasedStores.Stores[1] is {
                InstructionRawFileOffset: 0x00BBDE04,
                Kind: NetworkDispatcherEntryFallbackClearStoreKind.ReceiverByteClear,
                Address: 0x1064,
                Width: 1,
                Value: 0,
                Committed: true
            }
            && aliasedStores.Stores[0] != aliasedStores.Stores[1]
            && aliasedStores.Stores[0].Kind != aliasedStores.Stores[1].Kind
            && aliasedStores.Stores[0].Width != aliasedStores.Stores[1].Width
            && aliasedStores.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackClearStoresPlan rejectedBranchParent = Project(
            freshParentEBP: 0x2000,
            freshArgumentByte: 0,
            freshPostFallbackESI: 0x1000,
            initialReturnedEAX: 1);
        NetworkDispatcherEntryFallbackClearStoresPlan missingCompareByte = Project(
            freshParentEBP: 0x2000,
            freshArgumentByte: null,
            freshPostFallbackESI: 0x1000);
        NetworkDispatcherEntryFallbackClearStoresPlan unreachedESI = Project(
            freshParentEBP: 0x2000,
            freshArgumentByte: 0,
            freshPostFallbackESI: null,
            initialReturnedEAX: 1);
        NetworkDispatcherEntryFallbackClearStoresPlan pendingParent = Project(
            freshParentEBP: 0x2000,
            freshArgumentByte: 0,
            freshPostFallbackESI: 0,
            normalReturnObserved: null);
        check("Rejected and incomplete parents preserve their actual stop bounds and leave ESI unused",
            rejectedBranchParent.Outcome
                == NetworkDispatcherEntryFallbackClearStoresOutcome
                    .ParentArgumentCompareNotAccepted
            && rejectedBranchParent.StopAtRawFileOffsetExclusive
                == rejectedBranchParent.ParentPlan.StopAtRawFileOffsetExclusive
            && rejectedBranchParent.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && rejectedBranchParent.FreshPostFallbackESICaptureState
                == NetworkDispatcherEntryFallbackClearStoresCaptureState.SuppliedUnused
            && rejectedBranchParent.Stores.IsEmpty
            && rejectedBranchParent.CompletedInstructionOffsets.SequenceEqual(
                rejectedBranchParent.ParentPlan.CompletedInstructionOffsets)
            && !rejectedBranchParent.ProjectionEntered
            && !rejectedBranchParent.ExactBoundedCompletion
            && missingCompareByte.Outcome
                == NetworkDispatcherEntryFallbackClearStoresOutcome
                    .ParentArgumentCompareNotAccepted
            && missingCompareByte.StopAtRawFileOffsetExclusive == 0x00BBDDF9
            && missingCompareByte.FreshPostFallbackESICaptureState
                == NetworkDispatcherEntryFallbackClearStoresCaptureState.SuppliedUnused
            && missingCompareByte.Stores.IsEmpty
            && unreachedESI.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && unreachedESI.FreshPostFallbackESICaptureState
                == NetworkDispatcherEntryFallbackClearStoresCaptureState.Unreached
            && unreachedESI.Stores.IsEmpty
            && pendingParent.StopAtRawFileOffsetExclusive
                == pendingParent.ParentPlan.StopAtRawFileOffsetExclusive
            && pendingParent.StopAtRawFileOffsetExclusive == 0x00BBDDF0
            && pendingParent.FreshPostFallbackESICaptureState
                == NetworkDispatcherEntryFallbackClearStoresCaptureState.SuppliedUnused
            && pendingParent.Stores.IsEmpty
            && !pendingParent.ExactCleanCompletion);

        var immutableCapture = Capture(
            freshParentEBP: 0x2000,
            freshArgumentByte: 0x80,
            freshPostFallbackESI: 0x1000);
        NetworkDispatcherEntryFallbackClearStoresPlan first =
            NetworkDispatcherEntryFallbackClearStoresProjection.Project(immutableCapture);
        NetworkDispatcherEntryFallbackClearStoresPlan second =
            NetworkDispatcherEntryFallbackClearStoresProjection.Project(immutableCapture);
        check("Reprojection compares parent and child semantics without record equality on instruction arrays",
            first.OrderedSemanticPrefixPreserved
            && second.OrderedSemanticPrefixPreserved
            && first.ParentPlan.Outcome == second.ParentPlan.Outcome
            && first.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                second.ParentPlan.CompletedInstructionOffsets)
            && first.Stores.SequenceEqual(second.Stores)
            && first.CompletedInstructionOffsets.SequenceEqual(second.CompletedInstructionOffsets)
            && first.FlagsAtStop == second.FlagsAtStop
            && first.StopAtRawFileOffsetExclusive == second.StopAtRawFileOffsetExclusive
            && first.ExactBoundedCompletion == second.ExactBoundedCompletion
            && first.ExactCleanCompletion == second.ExactCleanCompletion);
    }
}

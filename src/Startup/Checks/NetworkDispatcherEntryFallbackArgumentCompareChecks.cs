using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackArgumentCompareChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFallbackArgumentCompareCapture Capture(
            uint? freshPostFallbackEBP = 0x2000,
            byte? freshArgumentByte = 0,
            uint? freshCounterEBX = 0x1000,
            uint? freshCounterDword = 0,
            uint? operationTarget = 0x12345678,
            bool? normalReturnObserved = true,
            uint? freshPostCallESP = 0x5000,
            uint? freshPostCallEBX = 0xA5A5,
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
                FreshEntryESI: 0x18F,
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

            return new(fallbackCounterCapture, freshPostFallbackEBP, freshArgumentByte);
        }

        static NetworkDispatcherEntryFallbackArgumentComparePlan Project(
            uint? freshPostFallbackEBP = 0x2000,
            byte? freshArgumentByte = 0,
            uint? freshCounterEBX = 0x1000,
            uint? freshCounterDword = 0,
            uint? operationTarget = 0x12345678,
            bool? normalReturnObserved = true,
            uint? freshPostCallESP = 0x5000,
            uint? freshPostCallEBX = 0xA5A5,
            uint? initialReturnedEAX = 0) =>
            NetworkDispatcherEntryFallbackArgumentCompareProjection.Project(Capture(
                freshPostFallbackEBP,
                freshArgumentByte,
                freshCounterEBX,
                freshCounterDword,
                operationTarget,
                normalReturnObserved,
                freshPostCallESP,
                freshPostCallEBX,
                initialReturnedEAX));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("The byte-CMP bytes, raw bounds, argument displacement, and immediate match IDA",
            NetworkDispatcherEntryFallbackArgumentCompareProjection.EntryRawFileOffset
                == 0x00BBDDF9
            && NetworkDispatcherEntryFallbackArgumentCompareProjection.CompareRawFileOffset
                == 0x00BBDDF9
            && NetworkDispatcherEntryFallbackArgumentCompareProjection.EntryRawFileOffset
                + NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta
                == 0x00FBE9F9
            && NetworkDispatcherEntryFallbackArgumentCompareProjection.ArgumentOffsetFromEBP
                == 0x08
            && NetworkDispatcherEntryFallbackArgumentCompareProjection.ArgumentByteWidth == 1
            && NetworkDispatcherEntryFallbackArgumentCompareProjection.ComparedImmediate == 0
            && NetworkDispatcherEntryFallbackArgumentCompareProjection.CompareInstructionByteLength
                == 4
            && NetworkDispatcherEntryFallbackArgumentCompareProjection
                .BoundaryRawFileOffsetExclusive == 0x00BBDDFD
            && NetworkDispatcherEntryFallbackArgumentCompareProjection.CompareRawFileOffset
                + NetworkDispatcherEntryFallbackArgumentCompareProjection
                    .CompareInstructionByteLength
                == NetworkDispatcherEntryFallbackArgumentCompareProjection
                    .BoundaryRawFileOffsetExclusive
            && Normalize(NetworkDispatcherEntryFallbackArgumentCompareProjection
                    .CompareInstructionBytes) == "807D0800"
            && !NetworkDispatcherEntryFallbackArgumentCompareProjection.ExecutesNativeInstructions);

        NetworkDispatcherEntryFallbackArgumentCompareCapture nonzeroCapture =
            Capture(freshPostFallbackEBP: 0x3000, freshArgumentByte: 0x80);
        NetworkDispatcherEntryFallbackArgumentComparePlan nonzero =
            NetworkDispatcherEntryFallbackArgumentCompareProjection.Project(nonzeroCapture);
        check("A fresh EBP and independent byte read project only CMP and compute its six flags",
            nonzero.ParentFallbackCounterPlan.ExactCleanCompletion
            && nonzero.ParentFallbackCounterPlan.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackArgumentCompareProjection.EntryRawFileOffset
            && nonzero.Outcome
                == NetworkDispatcherEntryFallbackArgumentCompareOutcome.Compared
            && nonzero.FreshPostFallbackEBPCaptureState
                == NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Captured
            && nonzero.FreshArgumentByteCaptureState
                == NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Captured
            && nonzero.FreshPostFallbackEBP == 0x3000
            && nonzero.FreshPostFallbackEBP != 0x2000
            && nonzero.ArgumentAddress == 0x3008
            && nonzero.ArgumentByte == 0x80
            && nonzero.ArgumentRead is {
                RawFileOffset: 0x00BBDDF9,
                Address: 0x3008,
                Width: 1,
                Attempted: true,
                Captured: true,
                Value: 0x80,
                Committed: false
            }
            && nonzero.CompareFlags is {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonzero.FlagsAtStop == nonzero.CompareFlags
            && nonzero.CompareReached
            && nonzero.ArgumentCompareProjected
            && nonzero.CompletedInstructionOffsets.SequenceEqual(
                nonzero.ParentFallbackCounterPlan.CompletedInstructionOffsets
                    .Add(0x00BBDDF9))
            && nonzero.EntryRawFileOffset == 0x00BBDDF9
            && nonzero.StopAtRawFileOffsetExclusive == 0x00BBDDFD
            && nonzero.ExactBoundedCompletion
            && nonzero.ExactCleanCompletion
            && !nonzero.BranchProjected
            && !nonzero.EntryFrameClearProjected
            && !nonzero.ReceiverByteClearProjected
            && !nonzero.PostCallEBPForwarded
            && !nonzero.NativeInstructionsExecuted
            && !nonzero.NativeMemoryReadsExecuted
            && !nonzero.NativeMemoryWritesExecuted);

        NetworkDispatcherEntryFallbackArgumentComparePlan zeroArgument =
            Project(freshPostFallbackEBP: 0, freshArgumentByte: 0);
        check("Captured zero is distinct from missing and uses a wrapped fresh [EBP+8] address",
            zeroArgument.Outcome
                == NetworkDispatcherEntryFallbackArgumentCompareOutcome.Compared
            && zeroArgument.FreshPostFallbackEBP == 0
            && zeroArgument.ArgumentAddress == 8
            && zeroArgument.ArgumentByte == 0
            && zeroArgument.ArgumentRead is { Captured: true, Value: 0 }
            && zeroArgument.CompareFlags is {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroArgument.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackArgumentComparePlan missingEBP =
            Project(freshPostFallbackEBP: null, freshArgumentByte: 0);
        NetworkDispatcherEntryFallbackArgumentComparePlan missingArgument =
            Project(freshPostFallbackEBP: 0x2000, freshArgumentByte: null);
        check("Missing EBP and missing byte stop separately without fabricating a read or CMP",
            missingEBP.Outcome
                == NetworkDispatcherEntryFallbackArgumentCompareOutcome
                    .MissingFreshPostFallbackEBP
            && missingEBP.FreshPostFallbackEBPCaptureState
                == NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Missing
            && missingEBP.FreshArgumentByteCaptureState
                == NetworkDispatcherEntryFallbackArgumentCompareCaptureState.SuppliedUnused
            && missingEBP.ArgumentRead is null
            && missingEBP.ArgumentAddress is null
            && missingEBP.CompareFlags is null
            && missingEBP.FlagsAtStop
                == ToLockGateFlags(missingEBP.ParentFallbackCounterPlan.FlagsAtStop)
            && missingEBP.StopAtRawFileOffsetExclusive == 0x00BBDDF9
            && !missingEBP.CompareReached
            && !missingEBP.ExactBoundedCompletion
            && !missingEBP.ExactCleanCompletion
            && missingArgument.Outcome
                == NetworkDispatcherEntryFallbackArgumentCompareOutcome
                    .MissingFreshArgumentByte
            && missingArgument.FreshPostFallbackEBPCaptureState
                == NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Captured
            && missingArgument.FreshArgumentByteCaptureState
                == NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Missing
            && missingArgument.FreshPostFallbackEBP == 0x2000
            && missingArgument.ArgumentAddress == 0x2008
            && missingArgument.ArgumentRead is {
                RawFileOffset: 0x00BBDDF9,
                Address: 0x2008,
                Width: 1,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false
            }
            && missingArgument.CompareFlags is null
            && missingArgument.FlagsAtStop
                == ToLockGateFlags(missingArgument.ParentFallbackCounterPlan.FlagsAtStop)
            && missingArgument.StopAtRawFileOffsetExclusive == 0x00BBDDF9
            && !missingArgument.CompareReached
            && !missingArgument.ExactBoundedCompletion
            && !missingArgument.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackArgumentComparePlan alias =
            Project(
                freshPostFallbackEBP: 0x1028,
                freshArgumentByte: 0x99,
                freshCounterEBX: 0x1000,
                freshCounterDword: 0x40);
        check("The fresh argument read remains independent when it aliases the preceding counter write",
            alias.ParentFallbackCounterPlan.CounterAddress == 0x1030
            && alias.ParentFallbackCounterPlan.CounterAfter == 0x41
            && alias.ParentFallbackCounterPlan.CounterAccesses.Any(access =>
                access.Kind == NetworkDispatcherEntryFallbackCounterAccessKind.Write
                && access.Address == 0x1030
                && access.Value == 0x41)
            && alias.ArgumentAddress == 0x1030
            && alias.ArgumentRead is {
                Address: 0x1030,
                Captured: true,
                Value: 0x99
            }
            && alias.ArgumentByte == 0x99
            && alias.ArgumentByte != alias.ParentFallbackCounterPlan.CounterAfter
            && alias.CompareFlags is {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && alias.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackArgumentComparePlan wrappedAddress =
            Project(freshPostFallbackEBP: 0xFFFFFFF8, freshArgumentByte: 0xFF);
        check("The fresh argument address and byte comparison use DWORD wrap and byte-width flags",
            wrappedAddress.ArgumentAddress == 0
            && wrappedAddress.ArgumentRead is { Address: 0, Width: 1, Value: 0xFF }
            && wrappedAddress.CompareFlags is {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && wrappedAddress.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackArgumentComparePlan nonzeroBypass =
            Project(initialReturnedEAX: 1, freshPostFallbackEBP: 0x2000, freshArgumentByte: 0);
        NetworkDispatcherEntryFallbackArgumentComparePlan unreachedParent =
            Project(initialReturnedEAX: 1, freshPostFallbackEBP: null, freshArgumentByte: null);
        NetworkDispatcherEntryFallbackArgumentComparePlan pendingParent =
            Project(normalReturnObserved: null, freshPostFallbackEBP: 0x2000, freshArgumentByte: 0);
        check("A rejected nonzero branch or incomplete parent preserves actual bounds and leaves suffix inputs unused",
            nonzeroBypass.ParentFallbackCounterPlan.Outcome
                == NetworkDispatcherEntryFallbackCounterOutcome
                    .ParentFallbackImportReturnNotAccepted
            && nonzeroBypass.ParentFallbackCounterPlan.StopAtRawFileOffsetExclusive
                == 0x00BBDDF6
            && nonzeroBypass.Outcome
                == NetworkDispatcherEntryFallbackArgumentCompareOutcome
                    .ParentFallbackCounterNotAccepted
            && nonzeroBypass.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && nonzeroBypass.FreshPostFallbackEBPCaptureState
                == NetworkDispatcherEntryFallbackArgumentCompareCaptureState.SuppliedUnused
            && nonzeroBypass.FreshArgumentByteCaptureState
                == NetworkDispatcherEntryFallbackArgumentCompareCaptureState.SuppliedUnused
            && nonzeroBypass.ArgumentRead is null
            && nonzeroBypass.CompletedInstructionOffsets.SequenceEqual(
                nonzeroBypass.ParentFallbackCounterPlan.CompletedInstructionOffsets)
            && !nonzeroBypass.ProjectionEntered
            && !nonzeroBypass.CompareReached
            && !nonzeroBypass.ExactBoundedCompletion
            && unreachedParent.FreshPostFallbackEBPCaptureState
                == NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Unreached
            && unreachedParent.FreshArgumentByteCaptureState
                == NetworkDispatcherEntryFallbackArgumentCompareCaptureState.Unreached
            && unreachedParent.ArgumentRead is null
            && unreachedParent.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && pendingParent.ParentFallbackCounterPlan.StopAtRawFileOffsetExclusive
                == 0x00BBDDF0
            && pendingParent.StopAtRawFileOffsetExclusive == 0x00BBDDF0
            && pendingParent.FreshPostFallbackEBPCaptureState
                == NetworkDispatcherEntryFallbackArgumentCompareCaptureState.SuppliedUnused
            && pendingParent.FreshArgumentByteCaptureState
                == NetworkDispatcherEntryFallbackArgumentCompareCaptureState.SuppliedUnused
            && pendingParent.ArgumentRead is null
            && !pendingParent.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackArgumentCompareCapture reprojectCapture =
            Capture(freshPostFallbackEBP: 0x2000, freshArgumentByte: 0x80);
        NetworkDispatcherEntryFallbackArgumentComparePlan first =
            NetworkDispatcherEntryFallbackArgumentCompareProjection.Project(reprojectCapture);
        NetworkDispatcherEntryFallbackArgumentComparePlan second =
            NetworkDispatcherEntryFallbackArgumentCompareProjection.Project(reprojectCapture);
        check("Reprojection preserves semantic ancestry and instruction/read sequences",
            first.Outcome == second.Outcome
            && first.ParentFallbackCounterPlan.Outcome
                == second.ParentFallbackCounterPlan.Outcome
            && first.ParentFallbackCounterPlan.CounterAccesses.SequenceEqual(
                second.ParentFallbackCounterPlan.CounterAccesses)
            && first.ParentFallbackCounterPlan.CompletedInstructionOffsets.SequenceEqual(
                second.ParentFallbackCounterPlan.CompletedInstructionOffsets)
            && first.ArgumentRead == second.ArgumentRead
            && first.CompletedInstructionOffsets.SequenceEqual(second.CompletedInstructionOffsets)
            && first.FlagsAtStop == second.FlagsAtStop
            && first.StopAtRawFileOffsetExclusive == second.StopAtRawFileOffsetExclusive
            && first.ExactBoundedCompletion == second.ExactBoundedCompletion
            && first.ExactCleanCompletion == second.ExactCleanCompletion);
    }

    private static NetworkDispatcherLockGateFlags? ToLockGateFlags(
        NetworkDispatcherEntryFallbackCounterFlags? flags) =>
        flags is { } value
            ? new(value.Carry, value.Parity, value.Auxiliary,
                value.Zero, value.Sign, value.Overflow)
            : null;
}

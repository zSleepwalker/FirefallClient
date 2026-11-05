using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackCounterChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFallbackCounterCapture Capture(
            uint? freshPostFallbackEBX = 0x1000,
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

            return new(
                fallbackImportCapture,
                freshPostFallbackEBX,
                freshCounterDword);
        }

        static NetworkDispatcherEntryFallbackCounterPlan Project(
            uint? freshPostFallbackEBX = 0x1000,
            uint? freshCounterDword = 0,
            uint? operationTarget = 0x12345678,
            bool? normalReturnObserved = true,
            uint? freshPostCallESP = 0x5000,
            uint? freshPostCallEBX = 0xA5A5,
            uint? initialReturnedEAX = 0) =>
            NetworkDispatcherEntryFallbackCounterProjection.Project(Capture(
                freshPostFallbackEBX,
                freshCounterDword,
                operationTarget,
                normalReturnObserved,
                freshPostCallESP,
                freshPostCallEBX,
                initialReturnedEAX));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("The INC bytes, boundary, and captured window are exact",
            NetworkDispatcherEntryFallbackCounterProjection.EntryRawFileOffset
                == 0x00BBDDF6
            && NetworkDispatcherEntryFallbackCounterProjection.IncrementRawFileOffset
                == 0x00BBDDF6
            && NetworkDispatcherEntryFallbackCounterProjection.CounterAddressDisplacement
                == 0x30
            && NetworkDispatcherEntryFallbackCounterProjection.CounterDwordWidth == 4
            && NetworkDispatcherEntryFallbackCounterProjection.IncrementInstructionByteLength == 3
            && NetworkDispatcherEntryFallbackCounterProjection.BoundaryRawFileOffsetExclusive
                == 0x00BBDDF9
            && Normalize(NetworkDispatcherEntryFallbackCounterProjection.IncrementInstructionBytes)
                == "FF4330"
            && Normalize(NetworkDispatcherEntryFallbackCounterProjection.CapturedWindowBytes)
                == "FF158442AC0185C0750753FF157842AC01FF4330"
            && !NetworkDispatcherEntryFallbackCounterProjection.ExecutesNativeInstructions);

        NetworkDispatcherEntryFallbackCounterPlan normal = Project(
            freshPostFallbackEBX: 0x1000,
            freshCounterDword: 0x7FFFFFFF);
        check("Normal fallback INC uses fresh EBX and computes the signed-overflow flags",
            normal.ParentFallbackImportReturnPlan.ExactCleanCompletion
            && normal.ParentFallbackImportReturnPlan.StopAtRawFileOffsetExclusive
                == 0x00BBDDF6
            && normal.Outcome == NetworkDispatcherEntryFallbackCounterOutcome.Incremented
            && normal.FreshPostFallbackEBXCaptureState
                == NetworkDispatcherEntryFallbackCounterCaptureState.Captured
            && normal.FreshCounterDwordCaptureState
                == NetworkDispatcherEntryFallbackCounterCaptureState.Captured
            && normal.FreshPostFallbackEBX == 0x1000
            && normal.FreshPostFallbackEBX
                != normal.ParentFallbackImportReturnPlan.PreCallEBX
            && normal.CounterAddress == 0x1030
            && normal.CounterBefore == 0x7FFFFFFF
            && normal.CounterAfter == 0x80000000
            && normal.CounterAccesses.SequenceEqual(new[]
            {
                new NetworkDispatcherEntryFallbackCounterAccess(
                    0x00BBDDF6,
                    NetworkDispatcherEntryFallbackCounterAccessKind.Read,
                    0x1030,
                    true,
                    true,
                    0x7FFFFFFF),
                new NetworkDispatcherEntryFallbackCounterAccess(
                    0x00BBDDF6,
                    NetworkDispatcherEntryFallbackCounterAccessKind.Write,
                    0x1030,
                    true,
                    true,
                    0x80000000)
            })
            && normal.FlagsAtStop is {
                Carry: null,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && normal.CarryPreserved
            && normal.CounterIncrementReached
            && normal.CompletedInstructionOffsets.SequenceEqual(new uint[] {
                0x00BBDDE5, 0x00BBDDEB, 0x00BBDDED, 0x00BBDDEF, 0x00BBDDF0,
                0x00BBDDF6 })
            && normal.EntryRawFileOffset == 0x00BBDDF6
            && normal.StopAtRawFileOffsetExclusive == 0x00BBDDF9
            && normal.ExactBoundedCompletion
            && normal.ExactCleanCompletion
            && normal.CounterIncrementProjected
            && !normal.PostCallESPForwarded
            && !normal.PostCallEBXForwarded
            && !normal.PostCallESIForwarded
            && !normal.PostCallEBPForwarded
            && !normal.NativeInstructionsExecuted
            && !normal.NativeMemoryWritesExecuted);

        NetworkDispatcherEntryFallbackCounterPlan zeroCounter = Project(
            freshPostFallbackEBX: 0,
            freshCounterDword: 0);
        NetworkDispatcherEntryFallbackCounterPlan maximumCounter = Project(
            freshPostFallbackEBX: uint.MaxValue,
            freshCounterDword: uint.MaxValue);
        check("Zero and maximum counters wrap as DWORDs with INC flag semantics",
            zeroCounter.CounterAddress == 0x30
            && zeroCounter.CounterBefore == 0
            && zeroCounter.CounterAfter == 1
            && zeroCounter.FlagsAtStop is {
                Carry: null,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && maximumCounter.CounterAddress == 0x2F
            && maximumCounter.CounterBefore == uint.MaxValue
            && maximumCounter.CounterAfter == 0
            && maximumCounter.FlagsAtStop is {
                Carry: null,
                Parity: true,
                Auxiliary: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroCounter.ExactCleanCompletion
            && maximumCounter.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackCounterPlan wrappedAddress = Project(
            freshPostFallbackEBX: 0xFFFFFFD0,
            freshCounterDword: 1);
        check("The effective counter address wraps at 32 bits",
            wrappedAddress.CounterAddress == 0
            && wrappedAddress.CounterAccesses.All(access => access.Address == 0)
            && wrappedAddress.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackCounterPlan missingEBX = Project(
            freshPostFallbackEBX: null,
            freshCounterDword: 0x12345678);
        check("Missing fresh EBX performs no memory access and leaves the DWORD unused",
            missingEBX.Outcome
                == NetworkDispatcherEntryFallbackCounterOutcome.MissingFreshPostFallbackEBX
            && missingEBX.FreshPostFallbackEBXCaptureState
                == NetworkDispatcherEntryFallbackCounterCaptureState.Missing
            && missingEBX.FreshCounterDwordCaptureState
                == NetworkDispatcherEntryFallbackCounterCaptureState.SuppliedUnused
            && missingEBX.CounterAccesses.IsEmpty
            && missingEBX.CounterAddress is null
            && missingEBX.CounterBefore is null
            && missingEBX.CounterAfter is null
            && missingEBX.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingEBX.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && !missingEBX.CounterIncrementReached
            && !missingEBX.ExactBoundedCompletion
            && !missingEBX.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackCounterPlan missingDword = Project(
            freshPostFallbackEBX: 0x2000,
            freshCounterDword: null);
        check("Missing counter DWORD attempts only the fresh effective-address read",
            missingDword.Outcome
                == NetworkDispatcherEntryFallbackCounterOutcome.MissingFreshCounterDword
            && missingDword.FreshPostFallbackEBXCaptureState
                == NetworkDispatcherEntryFallbackCounterCaptureState.Captured
            && missingDword.FreshCounterDwordCaptureState
                == NetworkDispatcherEntryFallbackCounterCaptureState.Missing
            && missingDword.CounterAddress == 0x2030
            && missingDword.CounterAccesses.Length == 1
            && missingDword.CounterAccesses[0] is {
                RawFileOffset: 0x00BBDDF6,
                Kind: NetworkDispatcherEntryFallbackCounterAccessKind.Read,
                Address: 0x2030,
                Attempted: true,
                Captured: false,
                Value: null
            }
            && missingDword.CounterBefore is null
            && missingDword.CounterAfter is null
            && missingDword.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingDword.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && !missingDword.CounterIncrementReached
            && !missingDword.ExactBoundedCompletion
            && !missingDword.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackCounterPlan aliasedCounter = Project(
            freshPostFallbackEBX: 0x4FCC,
            freshCounterDword: 0xDEADBEEF);
        check("A fresh operation DWORD may alias the earlier PUSH address without borrowing its value",
            aliasedCounter.ParentFallbackImportReturnPlan.ParentFallbackPushPlan.Push is {
                StackAddress: 0x4FFC,
                Value: 0xA5A5
            }
            && aliasedCounter.CounterAddress == 0x4FFC
            && aliasedCounter.CounterBefore == 0xDEADBEEF
            && aliasedCounter.CounterBefore
                != aliasedCounter.ParentFallbackImportReturnPlan.ParentFallbackPushPlan.Push?.Value
            && aliasedCounter.CounterAfter == 0xDEADBEF0
            && aliasedCounter.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackCounterPlan nonzeroParent =
            Project(initialReturnedEAX: 1, freshPostFallbackEBX: 0x3000);
        check("A rejected nonzero TEST branch cannot enter the fallback counter",
            nonzeroParent.Outcome
                == NetworkDispatcherEntryFallbackCounterOutcome
                    .ParentFallbackImportReturnNotAccepted
            && nonzeroParent.ParentFallbackImportReturnPlan.StopAtRawFileOffsetExclusive
                == 0x00BBDDF6
            && !nonzeroParent.ParentFallbackImportReturnPlan.ParentFallbackPushPlan
                .ZeroFallthroughEntered
            && nonzeroParent.FreshPostFallbackEBXCaptureState
                == NetworkDispatcherEntryFallbackCounterCaptureState.SuppliedUnused
            && nonzeroParent.FreshCounterDwordCaptureState
                == NetworkDispatcherEntryFallbackCounterCaptureState.SuppliedUnused
            && nonzeroParent.CounterAccesses.IsEmpty
            && !nonzeroParent.ProjectionEntered
            && !nonzeroParent.CounterIncrementReached);

        NetworkDispatcherEntryFallbackCounterPlan pendingParent =
            Project(normalReturnObserved: null);
        NetworkDispatcherEntryFallbackCounterPlan missingTarget =
            Project(operationTarget: null);
        NetworkDispatcherEntryFallbackCounterPlan missingPushEBX =
            Project(freshPostCallEBX: null);
        check("Pending, missing-target, and missing-push parents keep their bounds and captures unused",
            pendingParent.ParentFallbackImportReturnPlan.StopAtRawFileOffsetExclusive
                == 0x00BBDDF0
            && pendingParent.StopAtRawFileOffsetExclusive == 0x00BBDDF0
            && pendingParent.FreshPostFallbackEBXCaptureState
                == NetworkDispatcherEntryFallbackCounterCaptureState.SuppliedUnused
            && pendingParent.CounterAccesses.IsEmpty
            && missingTarget.ParentFallbackImportReturnPlan.StopAtRawFileOffsetExclusive
                == 0x00BBDDF0
            && missingTarget.StopAtRawFileOffsetExclusive == 0x00BBDDF0
            && missingTarget.FreshPostFallbackEBXCaptureState
                == NetworkDispatcherEntryFallbackCounterCaptureState.SuppliedUnused
            && missingPushEBX.ParentFallbackImportReturnPlan.StopAtRawFileOffsetExclusive
                == 0x00BBDDEF
            && missingPushEBX.StopAtRawFileOffsetExclusive == 0x00BBDDEF
            && missingPushEBX.FreshPostFallbackEBXCaptureState
                == NetworkDispatcherEntryFallbackCounterCaptureState.SuppliedUnused
            && missingPushEBX.CounterAccesses.IsEmpty
            && !pendingParent.ExactCleanCompletion
            && !missingTarget.ExactCleanCompletion
            && !missingPushEBX.ExactCleanCompletion);
    }
}

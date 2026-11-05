using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackImportReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFallbackImportReturnCapture Capture(
            uint? operationTarget = 0x12345678,
            bool? normalReturnObserved = true,
            uint? initialReturnedEAX = 0,
            uint? freshPostCallESP = 0x5000,
            uint? freshPostCallEBX = 0xA5A5,
            uint? entryESI = 0x18F)
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
                FreshEntryESI: entryESI,
                FreshEntryEDI: 0x33333333);
            var cookieCapture = new NetworkDispatcherEntryCookiePreparationCapture(
                registerCapture,
                FreshCookieGlobalDWORD: 0);
            var lockCapture = new NetworkDispatcherEntryLockPreparationCapture(
                new NetworkDispatcherEntryFsPublicationCapture(cookieCapture),
                FreshEntryECX: 0x123);
            var entryCallCapture = new NetworkDispatcherEntryImportReturnCapture(
                lockCapture,
                0x12345678,
                true,
                initialReturnedEAX);
            var pushCapture = new NetworkDispatcherEntryFallbackPushCapture(
                new NetworkDispatcherEntryReturnedTestBranchCapture(entryCallCapture),
                freshPostCallESP,
                freshPostCallEBX);

            return new(pushCapture, operationTarget, normalReturnObserved);
        }

        static NetworkDispatcherEntryFallbackImportReturnPlan Project(
            uint? operationTarget = 0x12345678,
            bool? normalReturnObserved = true,
            uint? initialReturnedEAX = 0,
            uint? freshPostCallESP = 0x5000,
            uint? freshPostCallEBX = 0xA5A5,
            uint? entryESI = 0x18F) =>
            NetworkDispatcherEntryFallbackImportReturnProjection.Project(Capture(
                operationTarget,
                normalReturnObserved,
                initialReturnedEAX,
                freshPostCallESP,
                freshPostCallEBX,
                entryESI));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("The fallback call bytes, fixed import cell, and stop boundary are exact",
            NetworkDispatcherEntryFallbackImportReturnProjection.EntryRawFileOffset
                == 0x00BBDDF0
            && NetworkDispatcherEntryFallbackImportReturnProjection.IndirectCallRawFileOffset
                == 0x00BBDDF0
            && NetworkDispatcherEntryFallbackImportReturnProjection.IndirectCallCellVirtualAddress
                == 0x01AC4278
            && NetworkDispatcherEntryFallbackImportReturnProjection.OperationTargetDWORDWidth == 4
            && NetworkDispatcherEntryFallbackImportReturnProjection.CounterIncrementRawFileOffset
                == 0x00BBDDF6
            && NetworkDispatcherEntryFallbackImportReturnProjection.IndirectCallInstructionByteLength
                == 6
            && Normalize(
                NetworkDispatcherEntryFallbackImportReturnProjection.IndirectCallInstructionBytes)
                == "FF157842AC01"
            && Normalize(
                NetworkDispatcherEntryFallbackImportReturnProjection.CapturedWindowBytes)
                == "FF158442AC0185C0750753FF157842AC01FF4330"
            && !NetworkDispatcherEntryFallbackImportReturnProjection.ExecutesNativeInstructions);

        NetworkDispatcherEntryFallbackImportReturnPlan normal = Project();
        check("A normal return completes only the fallback CALL and stops before INC",
            normal.ParentFallbackPushPlan.ExactCleanCompletion
            && normal.Outcome
                == NetworkDispatcherEntryFallbackImportReturnOutcome.BoundedBeforeCounterIncrement
            && normal.TargetRead is {
                RawFileOffset: 0x00BBDDF0,
                Address: 0x01AC4278,
                Width: 4,
                Attempted: true,
                Captured: true,
                Value: 0x12345678
            }
            && normal.Call is {
                RawFileOffset: 0x00BBDDF0,
                IndirectCallCellVirtualAddress: 0x01AC4278,
                OperationTargetDWORD: 0x12345678,
                InstructionReached: true,
                Attempted: true,
                ReturnedNormally: true,
                Completed: true
            }
            && normal.OperationTargetCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.Captured
            && normal.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.Captured
            && normal.CompletedInstructionOffsets.SequenceEqual(new uint[] {
                0x00BBDDE5, 0x00BBDDEB, 0x00BBDDED, 0x00BBDDEF, 0x00BBDDF0 })
            && normal.EntryRawFileOffset == 0x00BBDDF0
            && normal.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && normal.CounterIncrementBoundaryReached
            && normal.PreCallESP == normal.ParentFallbackPushPlan.Push?.StackAddress
            && normal.PreCallEBX == normal.ParentFallbackPushPlan.Push?.Value
            && normal.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && normal.ExactBoundedCompletion
            && normal.ExactCleanCompletion
            && !normal.CounterIncrementProjected
            && !normal.PostCallESPForwarded
            && !normal.PostCallEBXForwarded
            && !normal.PostCallESIForwarded
            && !normal.PostCallEBPForwarded
            && !normal.PostCallEAXForwarded
            && !normal.NativeInstructionsExecuted
            && !normal.NativeMemoryWritesExecuted
            && !normal.ImportABIInferred
            && !normal.ImportMeaningInferred
            && !normal.OperationTargetMeaningInferred
            && !normal.OperationTargetIsRawFileOffset);

        NetworkDispatcherEntryFallbackImportReturnPlan missingTarget =
            Project(operationTarget: null, normalReturnObserved: true);
        check("A missing fresh target attempts only the DWORD read and leaves return status unused",
            missingTarget.Outcome
                == NetworkDispatcherEntryFallbackImportReturnOutcome
                    .MissingFreshOperationTargetDWORD
            && missingTarget.OperationTargetCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.Missing
            && missingTarget.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.SuppliedUnused
            && missingTarget.TargetRead is {
                RawFileOffset: 0x00BBDDF0,
                Address: 0x01AC4278,
                Width: 4,
                Attempted: true,
                Captured: false,
                Value: null
            }
            && missingTarget.Call is null
            && missingTarget.CompletedInstructionOffsets.SequenceEqual(new uint[] {
                0x00BBDDE5, 0x00BBDDEB, 0x00BBDDED, 0x00BBDDEF })
            && missingTarget.StopAtRawFileOffsetExclusive == 0x00BBDDF0
            && missingTarget.FlagsAtStop
                == missingTarget.ParentFallbackPushPlan.FlagsAtStop
            && !missingTarget.CounterIncrementBoundaryReached
            && !missingTarget.ExactBoundedCompletion
            && !missingTarget.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackImportReturnPlan pending =
            Project(normalReturnObserved: null);
        NetworkDispatcherEntryFallbackImportReturnPlan didNotReturn =
            Project(normalReturnObserved: false);
        check("Pending and non-normal returns stop at F0 with TEST flags retained",
            pending.Outcome
                == NetworkDispatcherEntryFallbackImportReturnOutcome.NormalReturnNotObserved
            && pending.OperationTargetCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.Captured
            && pending.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.Pending
            && pending.Call is {
                Attempted: true,
                ReturnedNormally: null,
                Completed: false
            }
            && pending.StopAtRawFileOffsetExclusive == 0x00BBDDF0
            && pending.FlagsAtStop == pending.ParentFallbackPushPlan.FlagsAtStop
            && !pending.CounterIncrementBoundaryReached
            && !pending.ExactBoundedCompletion
            && !pending.ExactCleanCompletion
            && didNotReturn.Outcome
                == NetworkDispatcherEntryFallbackImportReturnOutcome.CallDidNotReturnNormally
            && didNotReturn.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.Captured
            && didNotReturn.Call is {
                Attempted: true,
                ReturnedNormally: false,
                Completed: false
            }
            && didNotReturn.StopAtRawFileOffsetExclusive == 0x00BBDDF0
            && didNotReturn.FlagsAtStop == didNotReturn.ParentFallbackPushPlan.FlagsAtStop
            && !didNotReturn.CounterIncrementBoundaryReached
            && !didNotReturn.ExactBoundedCompletion
            && !didNotReturn.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackImportReturnPlan zeroTarget =
            Project(operationTarget: 0);
        NetworkDispatcherEntryFallbackImportReturnPlan maximumTarget =
            Project(operationTarget: uint.MaxValue);
        check("Zero and maximum operation targets remain captured scalar DWORDs",
            zeroTarget.Call?.OperationTargetDWORD == 0
            && zeroTarget.OperationTargetCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.Captured
            && zeroTarget.ExactCleanCompletion
            && maximumTarget.Call?.OperationTargetDWORD == uint.MaxValue
            && maximumTarget.OperationTargetCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.Captured
            && maximumTarget.ExactCleanCompletion
            && !zeroTarget.ImportABIInferred
            && !zeroTarget.ImportMeaningInferred
            && !zeroTarget.OperationTargetMeaningInferred
            && !maximumTarget.ImportABIInferred
            && !maximumTarget.ImportMeaningInferred
            && !maximumTarget.OperationTargetMeaningInferred);

        NetworkDispatcherEntryFallbackImportReturnPlan aliasedRead =
            Project(
                operationTarget: 0xDEADBEEF,
                freshPostCallESP: 0x01AC427C,
                freshPostCallEBX: 0x11112222);
        check("The fresh target read may alias the prior PUSH cell without inheriting its value",
            aliasedRead.ParentFallbackPushPlan.Push is {
                StackAddress: 0x01AC4278,
                Value: 0x11112222
            }
            && aliasedRead.TargetRead is {
                Address: 0x01AC4278,
                Captured: true,
                Value: 0xDEADBEEF
            }
            && aliasedRead.Call?.OperationTargetDWORD == 0xDEADBEEF
            && aliasedRead.ParentFallbackPushPlan.Push?.Value
                != aliasedRead.TargetRead?.Value
            && aliasedRead.PreCallESP == aliasedRead.ParentFallbackPushPlan.Push?.StackAddress
            && aliasedRead.PreCallEBX == aliasedRead.ParentFallbackPushPlan.Push?.Value
            && aliasedRead.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackImportReturnPlan rejectedNonzeroBranch =
            Project(initialReturnedEAX: 1, operationTarget: 0x5555);
        check("A rejected nonzero branch fabricates no fallback target read or CALL",
            rejectedNonzeroBranch.Outcome
                == NetworkDispatcherEntryFallbackImportReturnOutcome.ParentFallbackPushNotAccepted
            && rejectedNonzeroBranch.ParentFallbackPushPlan.StopAtRawFileOffsetExclusive
                == 0x00BBDDF6
            && rejectedNonzeroBranch.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && rejectedNonzeroBranch.TargetRead is null
            && rejectedNonzeroBranch.Call is null
            && rejectedNonzeroBranch.OperationTargetCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.SuppliedUnused
            && rejectedNonzeroBranch.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.SuppliedUnused
            && !rejectedNonzeroBranch.ProjectionEntered
            && !rejectedNonzeroBranch.CounterIncrementBoundaryReached);

        NetworkDispatcherEntryFallbackImportReturnPlan missingParentESP =
            Project(freshPostCallESP: null);
        NetworkDispatcherEntryFallbackImportReturnPlan missingParentEBX =
            Project(freshPostCallEBX: null);
        check("Missing parent PUSH inputs retain their original EF bounds and suppress the CALL",
            missingParentESP.ParentFallbackPushPlan.StopAtRawFileOffsetExclusive
                == 0x00BBDDEF
            && missingParentESP.StopAtRawFileOffsetExclusive == 0x00BBDDEF
            && missingParentESP.TargetRead is null
            && missingParentESP.Call is null
            && missingParentESP.OperationTargetCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.SuppliedUnused
            && missingParentEBX.ParentFallbackPushPlan.StopAtRawFileOffsetExclusive
                == 0x00BBDDEF
            && missingParentEBX.StopAtRawFileOffsetExclusive == 0x00BBDDEF
            && missingParentEBX.TargetRead is null
            && missingParentEBX.Call is null
            && missingParentEBX.OperationTargetCaptureState
                == NetworkDispatcherEntryFallbackImportReturnCaptureState.SuppliedUnused);
    }
}

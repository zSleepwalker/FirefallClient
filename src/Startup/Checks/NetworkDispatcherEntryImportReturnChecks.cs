using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryImportReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryImportReturnCapture Capture(
            uint? operationTarget = 0x12345678,
            bool? normalReturnObserved = true,
            uint? returnedEAX = 0x87654321,
            uint? entryESI = 0x22222222,
            bool forgeLegacyParentPlan = false)
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
            if (forgeLegacyParentPlan)
                framePlan = framePlan with
                {
                    StopAtRawFileOffsetExclusive = 0x00BBDDC3
                };

            var registerCapture = new NetworkDispatcherEntryRegisterSaveCapture(
                frameCapture,
                framePlan,
                FreshEntryEBX: 0x11111111,
                FreshEntryESI: entryESI,
                FreshEntryEDI: 0x33333333);
            var cookieCapture = new NetworkDispatcherEntryCookiePreparationCapture(
                registerCapture,
                FreshCookieGlobalDWORD: 0);
            var lockPreparationCapture =
                new NetworkDispatcherEntryLockPreparationCapture(
                    new NetworkDispatcherEntryFsPublicationCapture(cookieCapture),
                    FreshEntryECX: 0x123);

            return new(
                lockPreparationCapture,
                operationTarget,
                normalReturnObserved,
                returnedEAX);
        }

        static NetworkDispatcherEntryImportReturnPlan Project(
            uint? operationTarget = 0x12345678,
            bool? normalReturnObserved = true,
            uint? returnedEAX = 0x87654321,
            uint? entryESI = 0x22222222,
            bool forgeLegacyParentPlan = false) =>
            NetworkDispatcherEntryImportReturnProjection.Project(Capture(
                operationTarget,
                normalReturnObserved,
                returnedEAX,
                entryESI,
                forgeLegacyParentPlan));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("The entry call bytes and stop boundary are exact",
            NetworkDispatcherEntryImportReturnProjection.EntryRawFileOffset
                == 0x00BBDDE5
            && NetworkDispatcherEntryImportReturnProjection.IndirectCallRawFileOffset
                == 0x00BBDDE5
            && NetworkDispatcherEntryImportReturnProjection.IndirectCallCellVirtualAddress
                == 0x01AC4284
            && NetworkDispatcherEntryImportReturnProjection.OperationTargetDWORDWidth == 4
            && NetworkDispatcherEntryImportReturnProjection.ExistingLockGateEntryRawFileOffset
                == 0x00BBDDEB
            && NetworkDispatcherEntryImportReturnProjection.IndirectCallInstructionByteLength == 6
            && Normalize(NetworkDispatcherEntryImportReturnProjection.IndirectCallInstructionBytes)
                == "FF158442AC01"
            && Normalize(NetworkDispatcherEntryImportReturnProjection.CapturedWindowBytes)
                == "FF158442AC0185C0750753FF157842AC01FF4330"
            && !NetworkDispatcherEntryImportReturnProjection.ExecutesNativeInstructions);

        NetworkDispatcherEntryImportReturnPlan normal = Project();
        check("Normal return requires fresh EAX and stops before the existing TEST",
            normal.ParentLockPreparationPlan.ExactLockPreparationProjected
            && normal.Outcome
                == NetworkDispatcherEntryImportReturnOutcome.BoundedBeforeExistingLockGate
            && normal.TargetRead is {
                RawFileOffset: 0x00BBDDE5,
                Address: 0x01AC4284,
                Width: 4,
                Attempted: true,
                Captured: true,
                Value: 0x12345678
            }
            && normal.Call is
            {
                RawFileOffset: 0x00BBDDE5,
                IndirectCallCellVirtualAddress: 0x01AC4284,
                OperationTargetDWORD: 0x12345678,
                InstructionReached: true,
                Attempted: true,
                ReturnedNormally: true,
                Completed: true
            }
            && normal.OperationTargetCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.Captured
            && normal.NormalReturnCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.Captured
            && normal.ReturnedEAXCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.Captured
            && normal.CompletedInstructionOffsets.SequenceEqual(new uint[] { 0x00BBDDE5 })
            && normal.EntryRawFileOffset == 0x00BBDDE5
            && normal.StopAtRawFileOffsetExclusive == 0x00BBDDEB
            && normal.ExistingLockGateEntryBoundaryReached
            && normal.ExistingLockGateEntryEAX == 0x87654321
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
            && normal.PreCallESP == normal.ParentLockPreparationPlan.CurrentESPAtStop
            && normal.PreCallEBP == normal.ParentLockPreparationPlan.EntryEBP
            && normal.PreCallEBX == normal.ParentLockPreparationPlan.CurrentEBXAtStop
            && normal.PreCallESI == normal.ParentLockPreparationPlan.CurrentESIAtStop
            && !normal.ExistingLockGateTESTReached
            && !normal.ExistingLockGateProjected
            && !normal.ExistingLockGateBranchProjected
            && !normal.PostCallESPForwarded
            && !normal.PostCallEBPForwarded
            && !normal.PostCallEBXForwarded
            && !normal.PostCallESIForwarded
            && !normal.NativeInstructionsExecuted
            && !normal.NativeMemoryWritesExecuted
            && !normal.ImportABIInferred
            && !normal.ImportMeaningInferred
            && !normal.OperationTargetMeaningInferred
            && !normal.OperationTargetIsRawFileOffset);

        NetworkDispatcherEntryImportReturnPlan missingTarget =
            Project(operationTarget: null, normalReturnObserved: true, returnedEAX: 0);
        check("A missing fresh target stops at E5 and preserves the parent's flags",
            missingTarget.Outcome
                == NetworkDispatcherEntryImportReturnOutcome.MissingFreshOperationTargetDWORD
            && missingTarget.OperationTargetCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.Missing
            && missingTarget.NormalReturnCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.SuppliedUnused
            && missingTarget.ReturnedEAXCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.SuppliedUnused
            && missingTarget.TargetRead is
            {
                RawFileOffset: 0x00BBDDE5,
                Address: 0x01AC4284,
                Width: 4,
                Attempted: true,
                Captured: false,
                Value: null
            }
            && missingTarget.Call is null
            && missingTarget.CompletedInstructionOffsets.IsEmpty
            && missingTarget.StopAtRawFileOffsetExclusive == 0x00BBDDE5
            && missingTarget.FlagsAtStop
                == missingTarget.ParentLockPreparationPlan.FlagsAtStop
            && !missingTarget.ExistingLockGateEntryBoundaryReached
            && !missingTarget.ExactBoundedCompletion
            && !missingTarget.ExactCleanCompletion);

        NetworkDispatcherEntryImportReturnPlan missingTargetAndEAX =
            Project(operationTarget: null, normalReturnObserved: true, returnedEAX: null);
        check("Missing target with absent returned EAX remains unreached and has no logical call",
            missingTargetAndEAX.TargetRead is
            {
                Address: 0x01AC4284,
                Attempted: true,
                Captured: false,
                Value: null
            }
            && missingTargetAndEAX.Call is null
            && missingTargetAndEAX.ReturnedEAXCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.Unreached
            && missingTargetAndEAX.StopAtRawFileOffsetExclusive == 0x00BBDDE5);

        NetworkDispatcherEntryImportReturnPlan pending =
            Project(normalReturnObserved: null, returnedEAX: 0);
        check("A pending return stops at E5 with later observations unused",
            pending.Outcome
                == NetworkDispatcherEntryImportReturnOutcome.NormalReturnNotObserved
            && pending.OperationTargetCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.Captured
            && pending.NormalReturnCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.Pending
            && pending.ReturnedEAXCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.SuppliedUnused
            && pending.Call?.Attempted == true
            && pending.Call?.ReturnedNormally is null
            && pending.StopAtRawFileOffsetExclusive == 0x00BBDDE5
            && pending.FlagsAtStop == pending.ParentLockPreparationPlan.FlagsAtStop
            && !pending.ExactBoundedCompletion
            && !pending.ExactCleanCompletion);

        NetworkDispatcherEntryImportReturnPlan didNotReturn =
            Project(normalReturnObserved: false, returnedEAX: uint.MaxValue);
        check("A non-normal return stops at E5 and preserves unused returned data",
            didNotReturn.Outcome
                == NetworkDispatcherEntryImportReturnOutcome.CallDidNotReturnNormally
            && didNotReturn.NormalReturnCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.Captured
            && didNotReturn.ReturnedEAXCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.SuppliedUnused
            && didNotReturn.Call?.ReturnedNormally == false
            && didNotReturn.Call?.Completed == false
            && didNotReturn.CompletedInstructionOffsets.IsEmpty
            && didNotReturn.StopAtRawFileOffsetExclusive == 0x00BBDDE5
            && didNotReturn.FlagsAtStop
                == didNotReturn.ParentLockPreparationPlan.FlagsAtStop
            && !didNotReturn.ExistingLockGateEntryBoundaryReached
            && !didNotReturn.ExactBoundedCompletion
            && !didNotReturn.ExactCleanCompletion);

        NetworkDispatcherEntryImportReturnPlan missingEAX =
            Project(returnedEAX: null);
        check("A normal return with missing fresh EAX reaches EB but leaves output unknown",
            missingEAX.Outcome
                == NetworkDispatcherEntryImportReturnOutcome.MissingFreshReturnedEAX
            && missingEAX.ReturnedEAXCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.Missing
            && missingEAX.ExistingLockGateEntryBoundaryReached
            && missingEAX.ExistingLockGateEntryEAX is null
            && missingEAX.CompletedInstructionOffsets.SequenceEqual(new uint[] { 0x00BBDDE5 })
            && missingEAX.StopAtRawFileOffsetExclusive == 0x00BBDDEB
            && missingEAX.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingEAX.ExactBoundedCompletion
            && !missingEAX.ExactCleanCompletion
            && missingEAX.ParentLockPreparationPlan.ExactLockPreparationProjected);

        NetworkDispatcherEntryImportReturnPlan zeroScalars =
            Project(operationTarget: 0, returnedEAX: 0);
        NetworkDispatcherEntryImportReturnPlan maximumScalars =
            Project(operationTarget: uint.MaxValue, returnedEAX: uint.MaxValue);
        check("Zero and maximum values remain captured scalars without ABI or import claims",
            zeroScalars.Call?.OperationTargetDWORD == 0
            && zeroScalars.ExistingLockGateEntryEAX == 0
            && zeroScalars.ExactCleanCompletion
            && maximumScalars.Call?.OperationTargetDWORD == uint.MaxValue
            && maximumScalars.ExistingLockGateEntryEAX == uint.MaxValue
            && maximumScalars.ExactCleanCompletion
            && !zeroScalars.ImportABIInferred
            && !zeroScalars.ImportMeaningInferred
            && !zeroScalars.OperationTargetMeaningInferred
            && !maximumScalars.ImportABIInferred
            && !maximumScalars.ImportMeaningInferred
            && !maximumScalars.OperationTargetMeaningInferred);

        NetworkDispatcherEntryImportReturnPlan targetIndependentOfStore =
            Project(operationTarget: 0xABCDEF01);
        check("The fresh target scalar is independently captured from prior local-store data",
            targetIndependentOfStore.ParentLockPreparationPlan.DwordWrite?.Address == 0x1FE4
            && targetIndependentOfStore.ParentLockPreparationPlan.DwordWrite?.Value == 0x18F
            && targetIndependentOfStore.Capture.FreshOperationTargetDWORD == 0xABCDEF01
            && targetIndependentOfStore.Call?.OperationTargetDWORD == 0xABCDEF01
            && targetIndependentOfStore.OperationTargetCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.Captured
            && targetIndependentOfStore.ExactCleanCompletion);

        NetworkDispatcherEntryImportReturnCapture immutableCapture = Capture(
            operationTarget: 0,
            normalReturnObserved: true,
            returnedEAX: 0);
        NetworkDispatcherEntryImportReturnPlan original =
            NetworkDispatcherEntryImportReturnProjection.Project(immutableCapture);
        NetworkDispatcherEntryImportReturnPlan reprojected =
            NetworkDispatcherEntryImportReturnProjection.Project(
                immutableCapture with
                {
                    FreshOperationTargetDWORD = uint.MaxValue,
                    NormalReturnObserved = false,
                    FreshReturnedEAX = uint.MaxValue
                });
        check("Immutable input reprojection does not mutate prior call or parent evidence",
            original.Capture.FreshOperationTargetDWORD == 0
            && original.Capture.NormalReturnObserved == true
            && original.Capture.FreshReturnedEAX == 0
            && original.Call?.OperationTargetDWORD == 0
            && original.ExistingLockGateEntryEAX == 0
            && reprojected.Capture.FreshOperationTargetDWORD == uint.MaxValue
            && reprojected.Capture.NormalReturnObserved == false
            && reprojected.Capture.FreshReturnedEAX == uint.MaxValue
            && reprojected.Call?.OperationTargetDWORD == uint.MaxValue
            && reprojected.Outcome
                == NetworkDispatcherEntryImportReturnOutcome.CallDidNotReturnNormally
            && reprojected.ExistingLockGateEntryEAX is null
            && original.ParentLockPreparationPlan.CompletedInstructionOffsets.SequenceEqual(
                reprojected.ParentLockPreparationPlan.CompletedInstructionOffsets)
            && original.ParentLockPreparationPlan.FlagsAtStop
                == reprojected.ParentLockPreparationPlan.FlagsAtStop);

        NetworkDispatcherEntryImportReturnPlan rejectedParent = Project(
            operationTarget: 0x4444,
            normalReturnObserved: true,
            returnedEAX: 0x5555,
            entryESI: null);
        check("Rejected parent bounds and flags are retained while supplied call observations stay unused",
            rejectedParent.Outcome
                == NetworkDispatcherEntryImportReturnOutcome.ParentLockPreparationNotAccepted
            && rejectedParent.EntryRawFileOffset
                == rejectedParent.ParentLockPreparationPlan.StopAtRawFileOffsetExclusive
            && rejectedParent.StopAtRawFileOffsetExclusive
                == rejectedParent.ParentLockPreparationPlan.StopAtRawFileOffsetExclusive
            && rejectedParent.FlagsAtStop
                == rejectedParent.ParentLockPreparationPlan.FlagsAtStop
            && rejectedParent.OperationTargetCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.SuppliedUnused
            && rejectedParent.NormalReturnCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.SuppliedUnused
            && rejectedParent.ReturnedEAXCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.SuppliedUnused
            && rejectedParent.Call is null
            && rejectedParent.CompletedInstructionOffsets.IsEmpty
            && !rejectedParent.ProjectionEntered);

        NetworkDispatcherEntryImportReturnPlan forgedParent = Project(
            forgeLegacyParentPlan: true);
        check("Parent admission is based on immutable exact-lock-preparation reprojection",
            normal.ParentLockPreparationPlan.ExactLockPreparationProjected
            && forgedParent.ParentLockPreparationPlan.ExactLockPreparationProjected
            && normal.ParentLockPreparationPlan.CompletedInstructionOffsets.SequenceEqual(
                forgedParent.ParentLockPreparationPlan.CompletedInstructionOffsets)
            && normal.Outcome == forgedParent.Outcome
            && normal.CompletedInstructionOffsets.SequenceEqual(
                forgedParent.CompletedInstructionOffsets)
            && normal.Call == forgedParent.Call
            && normal.FlagsAtStop == forgedParent.FlagsAtStop
            && normal.StopAtRawFileOffsetExclusive
                == forgedParent.StopAtRawFileOffsetExclusive);
    }
}

using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryReturnedTestBranchChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryImportReturnCapture ParentCapture(
            uint? target = 0x12345678,
            bool? returnedNormally = true,
            uint? returnedEAX = 0x87654321,
            uint? entryESI = 0x22222222,
            bool rejectParent = false)
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

            if (rejectParent)
            {
                var rejectedRegisterCapture = registerCapture with
                {
                    FreshEntryESI = null
                };
                lockCapture = new NetworkDispatcherEntryLockPreparationCapture(
                    new NetworkDispatcherEntryFsPublicationCapture(
                        new NetworkDispatcherEntryCookiePreparationCapture(
                            rejectedRegisterCapture,
                            FreshCookieGlobalDWORD: 0)),
                    FreshEntryECX: 0x123);
            }

            return new(lockCapture, target, returnedNormally, returnedEAX);
        }

        static NetworkDispatcherEntryReturnedTestBranchPlan Project(
            uint? target = 0x12345678,
            bool? returnedNormally = true,
            uint? returnedEAX = 0x87654321,
            uint? entryESI = 0x22222222,
            bool rejectParent = false) =>
            NetworkDispatcherEntryReturnedTestBranchProjection.Project(
                new NetworkDispatcherEntryReturnedTestBranchCapture(
                    ParentCapture(target, returnedNormally, returnedEAX, entryESI, rejectParent)));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("The returned TEST/JNZ bytes and exclusive target boundaries are exact",
            NetworkDispatcherEntryReturnedTestBranchProjection.EntryRawFileOffset
                == 0x00BBDDE5
            && NetworkDispatcherEntryReturnedTestBranchProjection.TestRawFileOffset
                == 0x00BBDDEB
            && NetworkDispatcherEntryReturnedTestBranchProjection.BranchRawFileOffset
                == 0x00BBDDED
            && NetworkDispatcherEntryReturnedTestBranchProjection.ZeroFallthroughRawFileOffset
                == 0x00BBDDEF
            && NetworkDispatcherEntryReturnedTestBranchProjection.NonzeroTargetRawFileOffset
                == 0x00BBDDF6
            && NetworkDispatcherEntryReturnedTestBranchProjection.TargetCounterIncrementRawFileOffset
                == 0x00BBDDF6
            && NetworkDispatcherEntryReturnedTestBranchProjection.TargetLockPushRawFileOffset
                == 0x00BBDDEF
            && Normalize(NetworkDispatcherEntryReturnedTestBranchProjection.TestAndBranchBytes)
                == "85C07507"
            && Normalize(NetworkDispatcherEntryReturnedTestBranchProjection.CapturedWindowBytes)
                == "FF158442AC0185C0750753FF157842AC01FF4330"
            && !NetworkDispatcherEntryReturnedTestBranchProjection.ExecutesNativeInstructions);

        NetworkDispatcherEntryReturnedTestBranchPlan zero = Project(
            returnedEAX: 0);
        check("Zero returned EAX takes the JNZ fallthrough and stops before PUSH EBX",
            zero.ParentEntryImportReturnPlan.ExactCleanCompletion
            && zero.Outcome
                == NetworkDispatcherEntryReturnedTestBranchOutcome.TestBranchProjected
            && zero.TestReached
            && zero.TestBranchTaken == false
            && zero.BranchInstructionReached
            && zero.SelectedDestinationRawFileOffset == 0x00BBDDEF
            && zero.StopAtRawFileOffsetExclusive == 0x00BBDDEF
            && zero.ExistingLockGateTestPlan?.Outcome
                == NetworkDispatcherLockGateOutcome.MissingReceiverBeforePush
            && zero.ExistingLockGateTestPlan?.TestFlags is {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.CompletedInstructionOffsets.SequenceEqual(new uint[] {
                0x00BBDDE5, 0x00BBDDEB, 0x00BBDDED })
            && !zero.TargetInstructionEntered
            && zero.ExactBoundedCompletion
            && zero.ExactCleanCompletion);

        NetworkDispatcherEntryReturnedTestBranchPlan highBit = Project(
            returnedEAX: 0x80000000);
        check("A high-bit nonzero EAX takes JNZ to F6 before INC EBX+30",
            highBit.Outcome
                == NetworkDispatcherEntryReturnedTestBranchOutcome.TestBranchProjected
            && highBit.TestReached
            && highBit.TestBranchTaken == true
            && highBit.BranchInstructionReached
            && highBit.SelectedDestinationRawFileOffset == 0x00BBDDF6
            && highBit.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && highBit.ExistingLockGateTestPlan?.Outcome
                == NetworkDispatcherLockGateOutcome.MissingReceiverBeforeIncrement
            && highBit.ExistingLockGateTestPlan?.TestFlags is {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && highBit.ExistingLockGateTestPlan?.IncrementedCounterValue is null
            && !highBit.TargetInstructionEntered
            && highBit.ExactBoundedCompletion
            && highBit.ExactCleanCompletion);

        NetworkDispatcherEntryReturnedTestBranchPlan maxValue = Project(
            returnedEAX: uint.MaxValue);
        check("Maximum returned EAX is nonzero and preserves exact TEST flags",
            maxValue.TestBranchTaken == true
            && maxValue.SelectedDestinationRawFileOffset == 0x00BBDDF6
            && maxValue.ExistingLockGateTestPlan?.TestFlags is {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && maxValue.ExactCleanCompletion);

        NetworkDispatcherEntryReturnedTestBranchPlan missingEAX = Project(
            returnedEAX: null);
        check("A normal return missing fresh EAX stops at EB without running TEST",
            missingEAX.Outcome
                == NetworkDispatcherEntryReturnedTestBranchOutcome.MissingFreshReturnedEAX
            && missingEAX.EntryImportReturnBoundaryReached
            && !missingEAX.TestReached
            && !missingEAX.BranchInstructionReached
            && missingEAX.StopAtRawFileOffsetExclusive == 0x00BBDDEB
            && missingEAX.ParentEntryImportReturnPlan.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && !missingEAX.ExactBoundedCompletion
            && !missingEAX.ExactCleanCompletion);

        NetworkDispatcherEntryReturnedTestBranchPlan missingTarget = Project(
            target: null,
            returnedEAX: 0);
        check("Missing target retains the parent stop and leaves returned TEST inputs unused",
            missingTarget.Outcome
                == NetworkDispatcherEntryReturnedTestBranchOutcome.EntryReturnPrefixNotAccepted
            && missingTarget.ParentEntryImportReturnPlan.Outcome
                == NetworkDispatcherEntryImportReturnOutcome.MissingFreshOperationTargetDWORD
            && missingTarget.StopAtRawFileOffsetExclusive == 0x00BBDDE5
            && !missingTarget.EntryImportReturnBoundaryReached
            && !missingTarget.TestReached
            && missingTarget.ParentEntryImportReturnPlan.ReturnedEAXCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.SuppliedUnused
            && !missingTarget.ExactCleanCompletion);

        NetworkDispatcherEntryReturnedTestBranchPlan pending = Project(
            returnedNormally: null,
            returnedEAX: 0);
        NetworkDispatcherEntryReturnedTestBranchPlan didNotReturn = Project(
            returnedNormally: false,
            returnedEAX: uint.MaxValue);
        check("Pending and non-normal returns retain their actual parent bounds",
            pending.ParentEntryImportReturnPlan.Outcome
                == NetworkDispatcherEntryImportReturnOutcome.NormalReturnNotObserved
            && pending.StopAtRawFileOffsetExclusive == 0x00BBDDE5
            && !pending.TestReached
            && didNotReturn.ParentEntryImportReturnPlan.Outcome
                == NetworkDispatcherEntryImportReturnOutcome.CallDidNotReturnNormally
            && didNotReturn.StopAtRawFileOffsetExclusive == 0x00BBDDE5
            && !didNotReturn.TestReached
            && !pending.ExactCleanCompletion
            && !didNotReturn.ExactCleanCompletion);

        NetworkDispatcherEntryReturnedTestBranchPlan rejectedParent = Project(
            returnedEAX: 0,
            rejectParent: true);
        check("Rejected parent preserves its bound and leaves supplied observations unused",
            rejectedParent.Outcome
                == NetworkDispatcherEntryReturnedTestBranchOutcome.EntryReturnPrefixNotAccepted
            && rejectedParent.ParentEntryImportReturnPlan.Outcome
                == NetworkDispatcherEntryImportReturnOutcome.ParentLockPreparationNotAccepted
            && rejectedParent.StopAtRawFileOffsetExclusive
                == rejectedParent.ParentEntryImportReturnPlan.StopAtRawFileOffsetExclusive
            && rejectedParent.ParentEntryImportReturnPlan.ReturnedEAXCaptureState
                == NetworkDispatcherEntryImportReturnCaptureState.SuppliedUnused
            && !rejectedParent.ProjectionEntered
            && !rejectedParent.TestReached);

        NetworkDispatcherEntryReturnedTestBranchCapture immutableCapture =
            new(ParentCapture(returnedEAX: 0));
        NetworkDispatcherEntryReturnedTestBranchPlan original =
            NetworkDispatcherEntryReturnedTestBranchProjection.Project(immutableCapture);
        NetworkDispatcherEntryReturnedTestBranchPlan reprojected =
            NetworkDispatcherEntryReturnedTestBranchProjection.Project(
                immutableCapture with
                {
                    ParentEntryImportReturnCapture =
                        immutableCapture.ParentEntryImportReturnCapture with
                        {
                            FreshReturnedEAX = 0x80000000
                        }
                });
        check("Immutable capture reprojection changes only the new TEST branch evidence",
            original.Capture.ParentEntryImportReturnCapture.FreshReturnedEAX == 0
            && original.TestBranchTaken == false
            && original.StopAtRawFileOffsetExclusive == 0x00BBDDEF
            && reprojected.Capture.ParentEntryImportReturnCapture.FreshReturnedEAX
                == 0x80000000
            && reprojected.TestBranchTaken == true
            && reprojected.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && original.ParentEntryImportReturnPlan.ParentLockPreparationPlan
                .CompletedInstructionOffsets.SequenceEqual(
                    reprojected.ParentEntryImportReturnPlan.ParentLockPreparationPlan
                        .CompletedInstructionOffsets));

        NetworkDispatcherEntryReturnedTestBranchPlan effectFree = Project(
            returnedEAX: 0);
        check("The prefix performs no calls, counter reads or writes, or post-call register forwarding",
            effectFree.ExistingLockGateTestPlan is {
                Reads.IsEmpty: true,
                Writes.IsEmpty: true,
                Pushes.IsEmpty: true,
                OpaqueCalls.IsEmpty: true
            }
            && effectFree.FlagsAtTestEntry is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && effectFree.FlagsAtStop is {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && !effectFree.PostCallESPForwarded
            && !effectFree.PostCallEBPForwarded
            && !effectFree.PostCallEBXForwarded
            && !effectFree.PostCallESIForwarded
            && !effectFree.NativeMemoryWritesExecuted
            && !effectFree.ExistingLockGateCounterReadProjected
            && !effectFree.ExistingLockGateCounterWriteProjected
            && !effectFree.ExistingLockGatePushProjected
            && !effectFree.ExistingLockGateImportCallProjected);
    }
}

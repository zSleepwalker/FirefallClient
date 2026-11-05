using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackPushChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryImportReturnCapture ParentCapture(
            uint? target = 0x12345678,
            bool? returnedNormally = true,
            uint? returnedEAX = 0x87654321,
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

            return new(lockCapture, target, returnedNormally, returnedEAX);
        }

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        static NetworkDispatcherEntryFallbackPushPlan ProjectFallbackPush(
            uint? returnedEAX = 0,
            uint? freshPostCallESP = 0x5000,
            uint? freshPostCallEBX = 0xA5A5,
            uint? entryESI = 0x18F,
            uint? operationTarget = 0x12345678,
            bool? returnedNormally = true) =>
            NetworkDispatcherEntryFallbackPushProjection.Project(
                new NetworkDispatcherEntryFallbackPushCapture(
                    new NetworkDispatcherEntryReturnedTestBranchCapture(
                        ParentCapture(
                            target: operationTarget,
                            returnedNormally: returnedNormally,
                            returnedEAX: returnedEAX,
                            entryESI: entryESI)),
                    freshPostCallESP,
                    freshPostCallEBX));

        check("The zero fallthrough PUSH bytes, bounds, and captured window are exact",
            NetworkDispatcherEntryFallbackPushProjection.EntryRawFileOffset
                == 0x00BBDDEF
            && NetworkDispatcherEntryFallbackPushProjection.PushRawFileOffset
                == 0x00BBDDEF
            && NetworkDispatcherEntryFallbackPushProjection.FallbackImportCallRawFileOffset
                == 0x00BBDDF0
            && NetworkDispatcherEntryFallbackPushProjection.PushInstructionByteLength == 1
            && NetworkDispatcherEntryFallbackPushProjection.StackPointerDecrement == 4
            && Normalize(NetworkDispatcherEntryFallbackPushProjection.PushInstructionBytes)
                == "53"
            && Normalize(NetworkDispatcherEntryFallbackPushProjection.CapturedWindowBytes)
                == "FF158442AC0185C0750753FF157842AC01FF4330"
            && !NetworkDispatcherEntryFallbackPushProjection.ExecutesNativeInstructions);

        NetworkDispatcherEntryFallbackPushPlan projectedPush = ProjectFallbackPush(
            freshPostCallESP: 0xCAFEBABE,
            freshPostCallEBX: 0xABCD1234);
        check("Zero TEST pushes fresh post-call EBX at fresh post-call ESP minus four",
            projectedPush.ParentTestBranchPlan.ExactCleanCompletion
            && projectedPush.Outcome
                == NetworkDispatcherEntryFallbackPushOutcome.PushProjected
            && projectedPush.ZeroFallthroughEntered
            && projectedPush.FreshPostCallESPCaptureState
                == NetworkDispatcherEntryFallbackPushCaptureState.Captured
            && projectedPush.FreshPostCallEBXCaptureState
                == NetworkDispatcherEntryFallbackPushCaptureState.Captured
            && projectedPush.Push is {
                RawFileOffset: 0x00BBDDEF,
                StackAddress: 0xCAFEBABA,
                Value: 0xABCD1234,
                Attempted: true,
                Committed: true
            }
            && projectedPush.ParentTestBranchPlan.ParentEntryImportReturnPlan.PreCallESI
                == 0x123
            && projectedPush.ParentTestBranchPlan.ParentEntryImportReturnPlan.PreCallEBX
                == 0x18F
            && projectedPush.ParentTestBranchPlan.ExistingLockGateTestPlan?.DerivedLockPointer
                is null
            && projectedPush.ParentTestBranchPlan.ExistingLockGateTestPlan?.Pushes.IsEmpty
                == true
            && projectedPush.ParentTestBranchPlan.ParentEntryImportReturnPlan.PreCallESP
                .HasValue
            && projectedPush.Capture.FreshPostCallEBX
                != projectedPush.ParentTestBranchPlan.ParentEntryImportReturnPlan.PreCallEBX
            && projectedPush.ParentTestBranchPlan.ParentEntryImportReturnPlan.PreCallESP
                != projectedPush.Capture.FreshPostCallESP
            && projectedPush.CompletedInstructionOffsets.SequenceEqual(new uint[] {
                0x00BBDDE5, 0x00BBDDEB, 0x00BBDDED, 0x00BBDDEF })
            && projectedPush.StopAtRawFileOffsetExclusive == 0x00BBDDF0
            && projectedPush.FlagsAtTestEntry is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && projectedPush.FlagsBeforePush
                == projectedPush.ParentTestBranchPlan.FlagsAtStop
            && projectedPush.FlagsAtStop == projectedPush.FlagsBeforePush
            && projectedPush.FlagsAtStop is { Auxiliary: null }
            && !projectedPush.FallbackImportCallEntered
            && !projectedPush.FallbackImportCallProjected
            && !projectedPush.FallbackImportTargetRead
            && !projectedPush.FallbackImportReturnObserved
            && !projectedPush.PushInstructionExecuted
            && !projectedPush.NativeMemoryWritesExecuted
            && !projectedPush.PreCallESPForwarded
            && !projectedPush.PreCallESILockBaseUsed
            && !projectedPush.PostCallESPForwarded
            && !projectedPush.PostCallEBXForwarded
            && projectedPush.ExactBoundedCompletion
            && projectedPush.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackPushPlan zeroStackPointer = ProjectFallbackPush(
            returnedEAX: 0,
            freshPostCallESP: 0,
            freshPostCallEBX: 0);
        check("Zero fresh ESP and EBX are captured values and wrap the stack address",
            zeroStackPointer.Push?.StackAddress == uint.MaxValue - 3
            && zeroStackPointer.Push?.Value == 0
            && zeroStackPointer.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackPushPlan maxStackPointer = ProjectFallbackPush(
            returnedEAX: 0,
            freshPostCallESP: uint.MaxValue,
            freshPostCallEBX: uint.MaxValue);
        check("Maximum ESP and EBX are captured values and project the wrapped stack address",
            maxStackPointer.Push?.StackAddress == uint.MaxValue - 4
            && maxStackPointer.Push?.Value == uint.MaxValue
            && maxStackPointer.FlagsAtStop is { Auxiliary: null }
            && maxStackPointer.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackPushPlan missingFreshESP = ProjectFallbackPush(
            freshPostCallESP: null,
            freshPostCallEBX: 0xBEEF);
        check("Missing ESP stops before PUSH and leaves supplied EBX unused",
            missingFreshESP.Outcome
                == NetworkDispatcherEntryFallbackPushOutcome.MissingFreshPostCallESP
            && missingFreshESP.StopAtRawFileOffsetExclusive == 0x00BBDDEF
            && missingFreshESP.FreshPostCallESPCaptureState
                == NetworkDispatcherEntryFallbackPushCaptureState.Missing
            && missingFreshESP.FreshPostCallEBXCaptureState
                == NetworkDispatcherEntryFallbackPushCaptureState.SuppliedUnused
            && missingFreshESP.Push is null
            && !missingFreshESP.ExactBoundedCompletion
            && !missingFreshESP.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackPushPlan missingFreshEBX = ProjectFallbackPush(
            freshPostCallESP: 0x6000,
            freshPostCallEBX: null);
        check("Missing EBX retains fresh ESP but does not project a store",
            missingFreshEBX.Outcome
                == NetworkDispatcherEntryFallbackPushOutcome.MissingFreshPostCallEBX
            && missingFreshEBX.StopAtRawFileOffsetExclusive == 0x00BBDDEF
            && missingFreshEBX.FreshPostCallESPCaptureState
                == NetworkDispatcherEntryFallbackPushCaptureState.Captured
            && missingFreshEBX.FreshPostCallEBXCaptureState
                == NetworkDispatcherEntryFallbackPushCaptureState.Missing
            && missingFreshEBX.Capture.FreshPostCallESP == 0x6000
            && missingFreshEBX.Push is null
            && !missingFreshEBX.ExactBoundedCompletion
            && !missingFreshEBX.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackPushPlan nonzeroFallback = ProjectFallbackPush(
            returnedEAX: uint.MaxValue,
            freshPostCallESP: 0x7000,
            freshPostCallEBX: 0xBEEF);
        check("Nonzero returned EAX rejects this fallback at F6 with suffix captures unused",
            nonzeroFallback.Outcome
                == NetworkDispatcherEntryFallbackPushOutcome.NonzeroTestBranchRejected
            && nonzeroFallback.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && nonzeroFallback.ParentTestBranchPlan.TestBranchTaken == true
            && nonzeroFallback.ParentTestBranchPlan.FlagsAtStop is {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonzeroFallback.FreshPostCallESPCaptureState
                == NetworkDispatcherEntryFallbackPushCaptureState.SuppliedUnused
            && nonzeroFallback.FreshPostCallEBXCaptureState
                == NetworkDispatcherEntryFallbackPushCaptureState.SuppliedUnused
            && nonzeroFallback.Push is null
            && !nonzeroFallback.ZeroFallthroughEntered
            && !nonzeroFallback.FallbackImportCallEntered
            && !nonzeroFallback.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackPushPlan missingReturnedEAX = ProjectFallbackPush(
            returnedEAX: null,
            freshPostCallESP: 0x8000,
            freshPostCallEBX: 0xCAFE);
        NetworkDispatcherEntryFallbackPushPlan pendingReturn = ProjectFallbackPush(
            returnedEAX: 0,
            freshPostCallESP: 0x9000,
            freshPostCallEBX: 0xD00D,
            returnedNormally: null);
        check("Missing and pending parent returns preserve EB and E5 stops with unused suffix captures",
            missingReturnedEAX.StopAtRawFileOffsetExclusive == 0x00BBDDEB
            && missingReturnedEAX.FreshPostCallESPCaptureState
                == NetworkDispatcherEntryFallbackPushCaptureState.SuppliedUnused
            && missingReturnedEAX.FreshPostCallEBXCaptureState
                == NetworkDispatcherEntryFallbackPushCaptureState.SuppliedUnused
            && missingReturnedEAX.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingReturnedEAX.Push is null
            && pendingReturn.StopAtRawFileOffsetExclusive == 0x00BBDDE5
            && pendingReturn.FreshPostCallESPCaptureState
                == NetworkDispatcherEntryFallbackPushCaptureState.SuppliedUnused
            && pendingReturn.FreshPostCallEBXCaptureState
                == NetworkDispatcherEntryFallbackPushCaptureState.SuppliedUnused
            && pendingReturn.FlagsAtStop
                == pendingReturn.ParentTestBranchPlan.FlagsAtStop
            && pendingReturn.Push is null
            && !missingReturnedEAX.FallbackImportCallEntered
            && !pendingReturn.FallbackImportCallEntered);

        var fallbackCapture = new NetworkDispatcherEntryFallbackPushCapture(
            new NetworkDispatcherEntryReturnedTestBranchCapture(
                ParentCapture(returnedEAX: 0)),
            FreshPostCallESP: 0xA000,
            FreshPostCallEBX: 0x1111);
        NetworkDispatcherEntryFallbackPushPlan immutableFallback =
            NetworkDispatcherEntryFallbackPushProjection.Project(fallbackCapture);
        NetworkDispatcherEntryFallbackPushPlan changedFallback =
            NetworkDispatcherEntryFallbackPushProjection.Project(
                fallbackCapture with { FreshPostCallEBX = 0x2222 });
        check("Fallback projection consumes immutable captures without accepting a forged plan",
            immutableFallback.Capture.FreshPostCallEBX == 0x1111
            && immutableFallback.Push?.Value == 0x1111
            && changedFallback.Capture.FreshPostCallEBX == 0x2222
            && changedFallback.Push?.Value == 0x2222
            && immutableFallback.Push?.Value == 0x1111
            && immutableFallback.Capture.FreshPostCallESP == 0xA000
            && changedFallback.Capture.FreshPostCallESP == 0xA000);
    }
}

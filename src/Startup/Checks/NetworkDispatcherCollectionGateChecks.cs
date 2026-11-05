using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherCollectionGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The collection window, signed threshold, branch targets, and opaque call anchor are exact",
            NetworkDispatcherCollectionGateProjection.EntryRawFileOffset == 0x00BBDE0E
            && NetworkDispatcherCollectionGateProjection.EntryVirtualAddress == 0x00FBEA0E
            && NetworkDispatcherCollectionGateProjection.EntryVirtualAddress
                - NetworkDispatcherCollectionGateProjection.EntryRawFileOffset
                == NetworkDispatcherCollectionGateProjection.RawToVirtualAddressDelta
            && NetworkDispatcherCollectionGateProjection.RawToVirtualAddressDelta == 0x00400C00
            && Normalize(NetworkDispatcherCollectionGateProjection.CapturedWindowBytes)
                == "833D9085E601017F14516A00683C4FBB016A01B98085E601E8054270008B86A800000033DB2B86A4000000C1F80285C074438B86A40000008B3C9885FF751957683B020000687F71B501687E71B501FF15A05EC10183C4108B078BCF"
            && NetworkDispatcherCollectionGateProjection.ThresholdDWORDVirtualAddress == 0x01E68590
            && NetworkDispatcherCollectionGateProjection.ThresholdCompareRawFileOffset == 0x00BBDE0E
            && NetworkDispatcherCollectionGateProjection.ThresholdBranchRawFileOffset == 0x00BBDE15
            && NetworkDispatcherCollectionGateProjection.FirstPushRawFileOffset == 0x00BBDE17
            && NetworkDispatcherCollectionGateProjection.ZeroPushRawFileOffset == 0x00BBDE18
            && NetworkDispatcherCollectionGateProjection.AddressImmediatePushRawFileOffset == 0x00BBDE1A
            && NetworkDispatcherCollectionGateProjection.OnePushRawFileOffset == 0x00BBDE1F
            && NetworkDispatcherCollectionGateProjection.SetECXImmediateRawFileOffset == 0x00BBDE21
            && NetworkDispatcherCollectionGateProjection.OpaqueCallRawFileOffset == 0x00BBDE26
            && NetworkDispatcherCollectionGateProjection.OpaqueCallTargetRawFileOffset == 0x012C2030
            && NetworkDispatcherCollectionGateProjection.OpaqueCallTargetVirtualAddress == 0x016C2C30
            && NetworkDispatcherCollectionGateProjection.ECXImmediateValue == 0x01E68580
            && NetworkDispatcherCollectionGateProjection.PushedAddressImmediate == 0x01BB4F3C
            && NetworkDispatcherCollectionGateProjection.CollectionStartRawFileOffset == 0x00BBDE2B
            && NetworkDispatcherCollectionGateProjection.ZeroEBXRawFileOffset == 0x00BBDE31
            && NetworkDispatcherCollectionGateProjection.SubtractStartRawFileOffset == 0x00BBDE33
            && NetworkDispatcherCollectionGateProjection.ShiftDifferenceRawFileOffset == 0x00BBDE39
            && NetworkDispatcherCollectionGateProjection.CollectionTestRawFileOffset == 0x00BBDE3C
            && NetworkDispatcherCollectionGateProjection.EmptyContinuationRawFileOffset == 0x00BBDE83
            && NetworkDispatcherCollectionGateProjection.ReloadStartRawFileOffset == 0x00BBDE40
            && NetworkDispatcherCollectionGateProjection.ReadCollectionEntryRawFileOffset == 0x00BBDE46
            && NetworkDispatcherCollectionGateProjection.CollectionEntryTestRawFileOffset == 0x00BBDE49
            && NetworkDispatcherCollectionGateProjection.ZeroEntryContinuationRawFileOffset == 0x00BBDE4D
            && NetworkDispatcherCollectionGateProjection.NonzeroEntryContinuationRawFileOffset == 0x00BBDE66
            && !NetworkDispatcherCollectionGateProjection.ExecutesNativeInstructions);

        NetworkDispatcherCollectionGatePlan missingThreshold =
            NetworkDispatcherCollectionGateProjection.Project(Capture());
        check("Missing threshold data is distinct from a captured zero and stops at CMP",
            missingThreshold.Outcome
                == NetworkDispatcherCollectionGateOutcome.MissingThresholdDWORD
            && missingThreshold.Reads is [{
                RawFileOffset: 0x00BBDE0E,
                Kind: NetworkDispatcherCollectionGateReadKind.ThresholdDWORD,
                Address: 0x01E68590,
                Width: 4,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false
            }]
            && !missingThreshold.ThresholdCompareReached
            && missingThreshold.ThresholdFlags is null
            && missingThreshold.CaptureStates.ThresholdDWORD
                == NetworkDispatcherCollectionGateCaptureState.Missing
            && missingThreshold.FlagsAtStop?.Carry == missingThreshold.ParentGatePlan.FlagsAtStop?.Carry
            && missingThreshold.FlagsAtStop?.Parity == missingThreshold.ParentGatePlan.FlagsAtStop?.Parity
            && missingThreshold.FlagsAtStop?.Auxiliary == missingThreshold.ParentGatePlan.FlagsAtStop?.Auxiliary
            && missingThreshold.FlagsAtStop?.Zero == missingThreshold.ParentGatePlan.FlagsAtStop?.Zero
            && missingThreshold.FlagsAtStop?.Sign == missingThreshold.ParentGatePlan.FlagsAtStop?.Sign
            && missingThreshold.FlagsAtStop?.Overflow == missingThreshold.ParentGatePlan.FlagsAtStop?.Overflow
            && missingThreshold.StopAtRawFileOffsetExclusive == 0x00BBDE0E
            && !missingThreshold.BoundedContinuationEntered);

        NetworkDispatcherCollectionGatePlan missingESP =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 0x80000000));
        NetworkDispatcherCollectionGatePlan pendingCall =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 1,
                esp: 0x2000,
                ecx: null));
        NetworkDispatcherCollectionGatePlan missingCallStatus =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 0,
                esp: 0x2000,
                ecx: 0x3333));
        NetworkDispatcherCollectionGatePlan failedCall =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 0,
                esp: 0x2000,
                ecx: 0x3333,
                callReturned: false));
        check("Signed-negative thresholds select the opaque-call path; pending calls preserve CMP flags",
            missingESP.Outcome
                == NetworkDispatcherCollectionGateOutcome.MissingEntryESPBeforePush
            && missingESP.ThresholdBranchTaken == false
            && missingESP.ThresholdFlags is {
                Carry: false, Parity: true, Auxiliary: true, Zero: false,
                Sign: false, Overflow: true
            }
            && missingESP.FlagsAtStop == missingESP.ThresholdFlags
            && missingESP.Pushes is [{
                RawFileOffset: 0x00BBDE17,
                StackPointerBefore: null,
                StackPointerAfter: null,
                Value: null,
                Attempted: true,
                Committed: false
            }]
            && missingESP.OpaqueCalls.IsEmpty
            && pendingCall.Outcome
                == NetworkDispatcherCollectionGateOutcome.MissingEntryECXBeforePush
            && pendingCall.FlagsAtStop == pendingCall.ThresholdFlags
            && pendingCall.CaptureStates.EntryECX
                == NetworkDispatcherCollectionGateCaptureState.Missing
            && pendingCall.Pushes is [{
                StackPointerBefore: 0x2000,
                StackPointerAfter: 0x1FFC,
                Value: null,
                Attempted: true,
                Committed: false
            }]
            && missingCallStatus.Outcome
                == NetworkDispatcherCollectionGateOutcome.MissingOpaqueCallReturnStatus
            && missingCallStatus.Pushes.Length == 4
            && missingCallStatus.OpaqueCalls is [{
                TargetRawFileOffset: 0x012C2030,
                TargetVirtualAddress: 0x016C2C30,
                ReturnedNormally: null
            }]
            && missingCallStatus.FlagsAtStop == missingCallStatus.ThresholdFlags
            && missingCallStatus.CaptureStates.OpaqueCallReturnStatus
                == NetworkDispatcherCollectionGateCaptureState.Missing
            && failedCall.Outcome
                == NetworkDispatcherCollectionGateOutcome.OpaqueCallDidNotReturnNormally
            && failedCall.OpaqueCallReturnInvalidatedFlags == false
            && failedCall.CaptureStates.OpaqueCallReturnStatus
                == NetworkDispatcherCollectionGateCaptureState.Captured
            && failedCall.FlagsAtStop == failedCall.ThresholdFlags
            && failedCall.StopAtRawFileOffsetExclusive == 0x00BBDE26);

        NetworkDispatcherCollectionGatePlan directThreshold =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                end: 0,
                start: 0,
                reload: 0x3000,
                collectionEntry: 0));
        NetworkDispatcherCollectionGatePlan signedHighBit =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 0x80000002,
                esp: 0x4000,
                ecx: 0x5000,
                callReturned: true,
                postLoggingESI: 0x1000,
                end: 0,
                start: 0));
        NetworkDispatcherCollectionGatePlan directClean =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                end: 0,
                start: 0));
        check("Signed JG sends 2 directly to collection but sends a high-bit DWORD through the opaque call",
            directThreshold.ThresholdBranchTaken == true
            && directThreshold.ThresholdSelectedDestinationRawFileOffset == 0x00BBDE2B
            && directThreshold.OpaqueCalls.IsEmpty
            && directThreshold.Pushes.IsEmpty
            && directThreshold.Outcome
                == NetworkDispatcherCollectionGateOutcome.EmptyShiftedDifference
            && directClean.ExactCleanExitAdmitted
            && directClean.ChildGateAdmitted
            && directClean.Capture.FreshThresholdDWORD == 2
            && directClean.ParentBoundedCompletionBoundaryReached
            && directClean.CaptureStates.EntryECX
                == NetworkDispatcherCollectionGateCaptureState.Unreached
            && !directClean.UnreachedEntryECXCaptureProvided
            && directClean.Reads.Any(read =>
                read.Kind == NetworkDispatcherCollectionGateReadKind.FreshEndDWORD
                && read.Captured && read.Value == 0)
            && directClean.ThresholdFlags is {
                Carry: false, Parity: false, Auxiliary: false, Zero: false,
                Sign: false, Overflow: false
            }
            && signedHighBit.ThresholdBranchTaken == false
            && signedHighBit.ThresholdSelectedDestinationRawFileOffset == 0x00BBDE17
            && signedHighBit.OpaqueCalls is [{ ReturnedNormally: true }]
            && signedHighBit.OpaqueCallReturnInvalidatedFlags == true
            && signedHighBit.FlagsAtStop is {
                Carry: false, Parity: true, Auxiliary: null, Zero: true,
                Sign: false, Overflow: false
            }
            && signedHighBit.ExactCleanExitAdmitted
            && signedHighBit.Reads[0].Address == 0x01E68590);

        NetworkDispatcherCollectionGatePlan missingEndDirect =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                start: 0x3000,
                reload: 0x4000,
                collectionEntry: 1));
        NetworkDispatcherCollectionGatePlan missingEndAfterCall =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 1,
                esp: 0x2000,
                ecx: 0x3333,
                callReturned: true,
                postLoggingESI: 0x1000));
        NetworkDispatcherCollectionGatePlan missingStart =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                end: 3,
                reload: 0x4000,
                collectionEntry: 1));
        check("Missing end preserves incoming CMP/unknown flags; missing start preserves XOR flags",
            missingEndDirect.Outcome
                == NetworkDispatcherCollectionGateOutcome.MissingFreshEndDWORD
            && missingEndDirect.Reads[^1] is {
                Address: 0x10A8, Attempted: true, Captured: false,
                Value: null, Committed: false
            }
            && missingEndDirect.FlagsAtStop == missingEndDirect.ThresholdFlags
            && missingEndDirect.UnreachedFreshStartCaptureProvided
            && missingEndDirect.UnreachedFreshReloadCaptureProvided
            && missingEndDirect.UnreachedCollectionEntryCaptureProvided
            && missingEndDirect.CaptureStates.FreshEndDWORD
                == NetworkDispatcherCollectionGateCaptureState.Missing
            && missingEndDirect.CaptureStates.FreshStartDWORD
                == NetworkDispatcherCollectionGateCaptureState.SuppliedUnused
            && missingEndDirect.ExactCleanExitAdmitted == false
            && missingEndAfterCall.Outcome
                == NetworkDispatcherCollectionGateOutcome.MissingFreshEndDWORD
            && missingEndAfterCall.FlagsAtStop is {
                Carry: null, Parity: null, Auxiliary: null, Zero: null,
                Sign: null, Overflow: null
            }
            && missingStart.Outcome
                == NetworkDispatcherCollectionGateOutcome.MissingFreshStartDWORD
            && missingStart.FlagsAtStop is {
                Carry: false, Parity: true, Auxiliary: null, Zero: true,
                Sign: false, Overflow: false
            }
            && missingStart.FlagsAfterZeroEBX is {
                Carry: false, Parity: true, Auxiliary: null, Zero: true,
                Sign: false, Overflow: false
            }
            && missingStart.ShiftFlags is null
            && missingStart.CaptureStates.FreshStartDWORD
                == NetworkDispatcherCollectionGateCaptureState.Missing
            && missingStart.Reads[^1] is {
                RawFileOffset: 0x00BBDE33,
                Address: 0x10A4,
                Captured: false,
                Value: null
            }
            && missingStart.UnreachedFreshReloadCaptureProvided
            && missingStart.UnreachedCollectionEntryCaptureProvided);

        NetworkDispatcherCollectionGatePlan emptyDifference =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                end: 3,
                start: 0,
                reload: 0xDEADBEEF,
                collectionEntry: 1));
        NetworkDispatcherCollectionGatePlan negativeDifference =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                end: uint.MaxValue,
                start: 0,
                reload: 0x2000,
                collectionEntry: 0));
        check("SUB wraps as DWORD, SAR is arithmetic by two, and empty selection leaves later captures unused",
            emptyDifference.Outcome
                == NetworkDispatcherCollectionGateOutcome.EmptyShiftedDifference
            && emptyDifference.DifferenceValue == 3
            && emptyDifference.ShiftedDifference == 0
            && emptyDifference.ShiftFlags is {
                Carry: true, Parity: true, Auxiliary: null, Zero: true,
                Sign: false, Overflow: null
            }
            && emptyDifference.EmptyBranchTaken == true
            && emptyDifference.SelectedDestinationRawFileOffset == 0x00BBDE83
            && emptyDifference.Reads is [
                { Kind: NetworkDispatcherCollectionGateReadKind.ThresholdDWORD,
                    Captured: true, Value: 2 },
                { Kind: NetworkDispatcherCollectionGateReadKind.FreshEndDWORD,
                    Captured: true, Value: 3 },
                { Kind: NetworkDispatcherCollectionGateReadKind.FreshStartDWORD,
                    Captured: true, Value: 0 }
            ]
            && emptyDifference.UnreachedFreshReloadCaptureProvided
            && emptyDifference.UnreachedCollectionEntryCaptureProvided
            && emptyDifference.ExactCleanExitAdmitted == false
            && negativeDifference.Outcome
                == NetworkDispatcherCollectionGateOutcome.ZeroCollectionEntry
            && negativeDifference.DifferenceValue == uint.MaxValue
            && negativeDifference.ShiftedDifference == uint.MaxValue
            && negativeDifference.ShiftFlags is {
                Carry: true, Parity: true, Auxiliary: null, Zero: false,
                Sign: true, Overflow: null
            }
            && negativeDifference.SubtractFlags is {
                Carry: false, Parity: true, Auxiliary: false, Zero: false,
                Sign: true, Overflow: false
            }
            && negativeDifference.CollectionTestFlags is {
                Carry: false, Parity: true, Auxiliary: null, Zero: false,
                Sign: true, Overflow: false
            }
            && negativeDifference.SelectedDestinationRawFileOffset == 0x00BBDE4D
            && negativeDifference.BoundedCompletionBoundaryReached);

        NetworkDispatcherCollectionGatePlan wrappedReceiver =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                parentReceiver: 0xFFFFFF80,
                end: 0x30,
                start: 0x20));
        check("Receiver-relative DWORD addresses wrap independently",
            wrappedReceiver.CollectionESI == 0xFFFFFF80
            && wrappedReceiver.Reads[1].Address == 0x28
            && wrappedReceiver.Reads[2].Address == 0x24
            && wrappedReceiver.FreshEndAddress == 0x28
            && wrappedReceiver.FreshStartAddress == 0x24);

        NetworkDispatcherCollectionGatePlan independentReload =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                end: 0x1005,
                start: 0x1000,
                reload: 0x2000,
                collectionEntry: 0));
        NetworkDispatcherCollectionGatePlan nonzeroEntry =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                end: 0x1005,
                start: 0x1000,
                reload: 0x2000,
                collectionEntry: 0xABCD));
        NetworkDispatcherCollectionGatePlan missingReload =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                end: 0x1005,
                start: 0x1000,
                collectionEntry: 0xABCD));
        NetworkDispatcherCollectionGatePlan missingEntry =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                end: 0x1005,
                start: 0x1000,
                reload: 0x2000));
        check("The post-SUB reload is independent, a captured zero entry selects 4D, and nonzero selects 66",
            independentReload.FreshStartAddress == 0x10A4
            && independentReload.ReloadAddress == 0x10A4
            && independentReload.Reads[1] is {
                Kind: NetworkDispatcherCollectionGateReadKind.FreshEndDWORD,
                Value: 0x1005,
                Captured: true
            }
            && independentReload.Reads[2].Value == 0x1000
            && independentReload.Reads[3] is {
                Kind: NetworkDispatcherCollectionGateReadKind.FreshReloadDWORD,
                Value: 0x2000,
                Captured: true
            }
            && independentReload.CollectionEntryAddress == 0x2000
            && independentReload.CollectionEntryValue == 0
            && independentReload.Outcome
                == NetworkDispatcherCollectionGateOutcome.ZeroCollectionEntry
            && independentReload.NonzeroEntryBranchTaken == false
            && independentReload.SelectedDestinationRawFileOffset == 0x00BBDE4D
            && !independentReload.OrderedEffects.Any(effect =>
                effect.RawFileOffset >= independentReload.SelectedDestinationRawFileOffset)
            && independentReload.Pushes.IsEmpty
            && nonzeroEntry.Outcome
                == NetworkDispatcherCollectionGateOutcome.NonzeroCollectionEntry
            && nonzeroEntry.NonzeroEntryBranchTaken == true
            && nonzeroEntry.SelectedDestinationRawFileOffset == 0x00BBDE66
            && !nonzeroEntry.OrderedEffects.Any(effect =>
                effect.RawFileOffset >= nonzeroEntry.SelectedDestinationRawFileOffset)
            && nonzeroEntry.Pushes.IsEmpty
            && missingReload.Outcome
                == NetworkDispatcherCollectionGateOutcome.MissingFreshReloadDWORD
            && missingReload.FlagsAtStop == missingReload.CollectionTestFlags
            && missingReload.UnreachedCollectionEntryCaptureProvided
            && missingEntry.Outcome
                == NetworkDispatcherCollectionGateOutcome.MissingFreshCollectionEntry
            && missingEntry.Reads[^1] is {
                Address: 0x2000, Captured: false, Value: null
            }
            && missingEntry.FlagsAtStop == missingEntry.CollectionTestFlags
            && missingEntry.StopAtRawFileOffsetExclusive == 0x00BBDE46);

        NetworkDispatcherCollectionGatePlan emptyESIFromParent =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                parentReceiver: 0));
        NetworkDispatcherCollectionGatePlan logPathMissingESI =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 1,
                esp: 0x4000,
                ecx: 0x5000,
                callReturned: true));
        NetworkDispatcherCollectionGatePlan dirtyDirect =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 2,
                ecx: 0x1234,
                end: 0,
                start: 0));
        check("Parent ESI feeds only the direct route; logging requires a fresh post-call ESI and direct ECX is unused",
            emptyESIFromParent.Reads.Any(read =>
                read.Kind == NetworkDispatcherCollectionGateReadKind.FreshEndDWORD
                && read.Address == 0xA8)
            && emptyESIFromParent.CollectionESI == 0
            && logPathMissingESI.Outcome
                == NetworkDispatcherCollectionGateOutcome.MissingPostLoggingESI
            && logPathMissingESI.FlagsAtStop is {
                Carry: null, Parity: null, Auxiliary: null, Zero: null,
                Sign: null, Overflow: null
            }
            && dirtyDirect.UnreachedEntryECXCaptureProvided
            && dirtyDirect.CaptureStates.EntryECX
                == NetworkDispatcherCollectionGateCaptureState.SuppliedUnused
            && !dirtyDirect.ExactCleanExitAdmitted
            && dirtyDirect.SelectedDestinationRawFileOffset == 0x00BBDE83);

        NetworkDispatcherCollectionGatePlan dirtyParentZero =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                parentArgument: 0,
                parentUnusedOpaqueStatus: true,
                threshold: 2));
        NetworkDispatcherCollectionGatePlan dirtyParentNonzero =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                parentUnusedOpaqueStatus: true,
                threshold: 2));
        check("Parent capture is reprojected strictly and dirty captures cannot erase its selected boundary",
            dirtyParentZero.Outcome
                == NetworkDispatcherCollectionGateOutcome.ParentGateDidNotSelectCollectionEntry
            && dirtyParentZero.ParentBoundedCompletionBoundaryReached
            && !dirtyParentZero.ChildGateAdmitted
            && !dirtyParentZero.BoundedCompletionBoundaryReached
            && dirtyParentZero.ParentGatePlan.SelectedDestinationRawFileOffset == 0x00BBDECA
            && dirtyParentZero.SelectedDestinationRawFileOffset == 0x00BBDECA
            && dirtyParentZero.StopAtRawFileOffsetExclusive == 0x00BBDECA
            && dirtyParentZero.CaptureStates.ThresholdDWORD
                == NetworkDispatcherCollectionGateCaptureState.SuppliedUnused
            && dirtyParentZero.FlagsAtStop?.Zero
                == dirtyParentZero.ParentGatePlan.FlagsAtStop?.Zero
            && dirtyParentZero.Reads.IsEmpty
            && dirtyParentZero.UnreachedThresholdCaptureProvided
            && dirtyParentNonzero.Outcome
                == NetworkDispatcherCollectionGateOutcome.ParentGateExitNotClean
            && dirtyParentNonzero.ParentBoundedCompletionBoundaryReached
            && !dirtyParentNonzero.ChildGateAdmitted
            && !dirtyParentNonzero.BoundedCompletionBoundaryReached
            && dirtyParentNonzero.ParentGatePlan.SelectedDestinationRawFileOffset == 0x00BBDE0E
            && dirtyParentNonzero.SelectedDestinationRawFileOffset == 0x00BBDE0E
            && dirtyParentNonzero.StopAtRawFileOffsetExclusive == 0x00BBDE0E
            && dirtyParentNonzero.Reads.IsEmpty);

        NetworkDispatcherCollectionGatePlan logged =
            NetworkDispatcherCollectionGateProjection.Project(Capture(
                threshold: 0,
                esp: 8,
                ecx: 0x11223344,
                callReturned: true,
                postLoggingESI: 0x1000,
                end: 0,
                start: 0));
        check("Committed pushes wrap ESP in order and parent effects precede fresh child reads",
            logged.Pushes is [
                { RawFileOffset: 0x00BBDE17, StackPointerBefore: 8,
                    StackPointerAfter: 4, Value: 0x11223344, Committed: true },
                { RawFileOffset: 0x00BBDE18, StackPointerBefore: 4,
                    StackPointerAfter: 0, Value: 0, Committed: true },
                { RawFileOffset: 0x00BBDE1A, StackPointerBefore: 0,
                    StackPointerAfter: 0xFFFFFFFC, Value: 0x01BB4F3C, Committed: true },
                { RawFileOffset: 0x00BBDE1F, StackPointerBefore: 0xFFFFFFFC,
                    StackPointerAfter: 0xFFFFFFF8, Value: 1, Committed: true }
            ]
            && logged.OrderedEffects[0].RawFileOffset
                < NetworkDispatcherCollectionGateProjection.ThresholdCompareRawFileOffset
            && logged.OrderedEffects.Any(effect =>
                effect.Kind == NetworkDispatcherCollectionGateOrderedEffectKind.ThresholdDWORDRead)
            && logged.OrderedEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(logged.OrderedEffects.Select(effect => effect.RawFileOffset)
                    .OrderBy(offset => offset))
            && logged.SelectedDestinationRawFileOffset == 0x00BBDE83
            && !logged.OrderedEffects.Any(effect =>
                effect.RawFileOffset >= NetworkDispatcherCollectionGateProjection.EmptyContinuationRawFileOffset)
            && !logged.OrderedEffects.Any(effect =>
                effect.RawFileOffset >= NetworkDispatcherCollectionGateProjection.ZeroEntryContinuationRawFileOffset)
            && !logged.BoundedContinuationEntered);
    }

    private static NetworkDispatcherCollectionGateCapture Capture(
        uint? threshold = null,
        uint? esp = null,
        uint? ecx = null,
        bool? callReturned = null,
        uint? postLoggingESI = null,
        uint? end = null,
        uint? start = null,
        uint? reload = null,
        uint? collectionEntry = null,
        uint? parentReceiver = 0x1000,
        byte parentArgument = 1,
        bool parentUnusedOpaqueStatus = false)
    {
        var parent = new NetworkDispatcherLockGateCapture(
            ResultEAX: 1,
            EntryESIReceiver: parentReceiver,
            FreshCounterValue: 1,
            EntryEBP: 0x2000,
            FreshArgumentByte: parentArgument,
            OpaqueImportReturnedNormally: parentUnusedOpaqueStatus ? true : null);
        return new NetworkDispatcherCollectionGateCapture(
            parent,
            EntryESP: esp,
            EntryECX: ecx,
            FreshThresholdDWORD: threshold,
            OpaqueCallReturnedNormally: callReturned,
            FreshPostLoggingESI: postLoggingESI,
            FreshEndDWORD: end,
            FreshStartDWORD: start,
            FreshReloadDWORD: reload,
            FreshCollectionEntryDWORD: collectionEntry);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Where(character => !char.IsWhiteSpace(character)))
            .ToUpperInvariant();
}

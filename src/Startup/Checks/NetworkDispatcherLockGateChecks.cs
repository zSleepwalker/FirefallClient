using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherLockGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The captured internal window and raw/VA anchors are exact and bounded",
            NetworkDispatcherLockGateProjection.EntryVirtualAddress == 0x00FBE9EB
            && NetworkDispatcherLockGateProjection.EntryRawFileOffset == 0x00BBDDEB
            && NetworkDispatcherLockGateProjection.EntryVirtualAddress
                - NetworkDispatcherLockGateProjection.EntryRawFileOffset
                == NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta
            && NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta == 0x00400C00
                && NetworkDispatcherLockGateProjection.TestRawFileOffset == 0x00BBDDEB
                && NetworkDispatcherLockGateProjection.TestBranchRawFileOffset == 0x00BBDDED
                && NetworkDispatcherLockGateProjection.PushLockRawFileOffset == 0x00BBDDEF
            && Normalize(NetworkDispatcherLockGateProjection.CapturedWindowBytes)
                == "85C0750753FF157842AC01FF4330807D0800C745FC00000000C64664000F84BC000000"
            && NetworkDispatcherLockGateProjection.OpaqueImportCallRawFileOffset
                    == 0x00BBDDF0
            && NetworkDispatcherLockGateProjection.OpaqueImportCellVirtualAddress
                == 0x01AC4278
            && NetworkDispatcherLockGateProjection.IncrementCounterRawFileOffset
                == 0x00BBDDF6
            && NetworkDispatcherLockGateProjection.CompareArgumentRawFileOffset
                == 0x00BBDDF9
            && NetworkDispatcherLockGateProjection.ClearEntryFrameRawFileOffset
                == 0x00BBDDFD
            && NetworkDispatcherLockGateProjection.ClearReceiverByteRawFileOffset
                == 0x00BBDE04
            && NetworkDispatcherLockGateProjection.ZeroBranchRawFileOffset == 0x00BBDE08
            && NetworkDispatcherLockGateProjection.BoundedFallthroughRawFileOffset
                == 0x00BBDE0E
            && NetworkDispatcherLockGateProjection.ZeroContinuationRawFileOffset
                == 0x00BBDECA
            && NetworkDispatcherLockGateProjection.NonzeroContinuationRawFileOffset
                == 0x00BBDE0E
            && NetworkDispatcherLockGateProjection.LockPointerOffsetFromReceiver == 0x6C
            && NetworkDispatcherLockGateProjection.CounterOffsetFromReceiver == 0x9C
            && NetworkDispatcherLockGateProjection.ExecutesNativeInstructions == false);

        NetworkDispatcherLockGatePlan missingEAX =
            NetworkDispatcherLockGateProjection.Project(new());
        check("The initial DWORD TEST requires a fresh ResultEAX and reaches no later operation",
            missingEAX.Outcome == NetworkDispatcherLockGateOutcome.MissingResultEAX
            && !missingEAX.TestReached
            && missingEAX.Reads.IsEmpty
            && missingEAX.Writes.IsEmpty
            && missingEAX.Pushes.IsEmpty
            && missingEAX.OpaqueCalls.IsEmpty
            && missingEAX.FlagsAtStop is null
            && missingEAX.StopAtRawFileOffsetExclusive
                == NetworkDispatcherLockGateProjection.TestRawFileOffset
            && missingEAX.TestFlags is null
            && missingEAX.IncrementFlags is null);

        NetworkDispatcherLockGateCapture zeroPath = Capture(
            resultEAX: 0,
            receiver: 0x1000,
            counter: uint.MaxValue,
            ebp: 0x2000,
            argument: 0,
            opaqueReturnedNormally: true);
        NetworkDispatcherLockGatePlan zero = NetworkDispatcherLockGateProjection.Project(zeroPath);
        check("Zero EAX pushes the derived lock, calls only the opaque cell, then wraps and clears in order",
            zero.Outcome == NetworkDispatcherLockGateOutcome.ZeroArgumentBranchSelected
            && zero.TestBranchTaken == false
            && zero.Pushes is [{ RawFileOffset: 0x00BBDDEF, Value: 0x106C,
                Attempted: true, Committed: true }]
            && zero.OpaqueCalls is [{ RawFileOffset: 0x00BBDDF0,
                ImportCellVirtualAddress: 0x01AC4278, Attempted: true,
                ReturnedNormally: true }]
            && zero.DerivedLockPointer == 0x106C
            && zero.CounterAddress == 0x109C
            && zero.IncrementedCounterValue == 0
            && zero.Reads is [
                { RawFileOffset: 0x00BBDDF6,
                    Kind: NetworkDispatcherLockGateReadKind.FreshCounter,
                    Address: 0x109C, Width: 4, Attempted: true, Captured: true,
                    Value: uint.MaxValue, Committed: false },
                { RawFileOffset: 0x00BBDDF9,
                    Kind: NetworkDispatcherLockGateReadKind.FreshArgumentByte,
                    Address: 0x2008, Width: 1, Attempted: true, Captured: true,
                    Value: 0, Committed: false }]
            && zero.Writes is [
                { RawFileOffset: 0x00BBDDF6,
                    Kind: NetworkDispatcherLockGateWriteKind.CounterIncrement,
                    Address: 0x109C, Width: 4, Value: 0, Committed: true },
                { RawFileOffset: 0x00BBDDFD,
                    Kind: NetworkDispatcherLockGateWriteKind.EntryFrameClear,
                    Address: 0x1FFC, Width: 4, Value: 0, Committed: true },
                { RawFileOffset: 0x00BBDE04,
                    Kind: NetworkDispatcherLockGateWriteKind.ReceiverByteClear,
                    Address: 0x1064, Width: 1, Value: 0, Committed: true }]
            && zero.OrderedEffects.Select(effect => effect.Kind).SequenceEqual(new[] {
                NetworkDispatcherLockGateOrderedEffectKind.LockPush,
                NetworkDispatcherLockGateOrderedEffectKind.OpaqueImportCall,
                NetworkDispatcherLockGateOrderedEffectKind.FreshCounterRead,
                NetworkDispatcherLockGateOrderedEffectKind.CounterIncrement,
                NetworkDispatcherLockGateOrderedEffectKind.FreshArgumentByteRead,
                NetworkDispatcherLockGateOrderedEffectKind.EntryFrameClear,
                NetworkDispatcherLockGateOrderedEffectKind.ReceiverByteClear })
            && zero.OrderedEffects.Select(effect => effect.RawFileOffset).SequenceEqual(
                new uint[] { 0x00BBDDEF, 0x00BBDDF0, 0x00BBDDF6, 0x00BBDDF6,
                    0x00BBDDF9, 0x00BBDDFD, 0x00BBDE04 })
            && zero.OpaqueCallReturnInvalidatedFlags == true
            && zero.TestFlags is {
                Carry: false, Parity: true, Auxiliary: null, Zero: true,
                Sign: false, Overflow: false
            }
            && zero.IncrementFlags is {
                Carry: null, Parity: true, Auxiliary: true, Zero: true,
                Sign: false, Overflow: false
            }
            && zero.CompareFlags is { Carry: false, Parity: true, Auxiliary: false,
                Zero: true, Sign: false, Overflow: false }
            && zero.BranchReached
            && zero.BranchTaken == true
            && zero.SelectedDestinationRawFileOffset == 0x00BBDECA
            && zero.StopAtRawFileOffsetExclusive == 0x00BBDECA
            && zero.BoundedCompletionBoundaryReached
            && !zero.BoundedContinuationEntered
            && zero.ExactCleanExitAdmitted);

        NetworkDispatcherLockGatePlan missingReceiverAtPush =
            NetworkDispatcherLockGateProjection.Project(new NetworkDispatcherLockGateCapture(
                ResultEAX: 0,
                EntryESIReceiver: null,
                FreshCounterValue: 1,
                EntryEBP: 2,
                FreshArgumentByte: 1,
                OpaqueImportReturnedNormally: true));
        check("The zero-result path needs ESI for PUSH and does not commit a fabricated lock value",
            missingReceiverAtPush.Outcome
                == NetworkDispatcherLockGateOutcome.MissingReceiverBeforePush
            && missingReceiverAtPush.FlagsAtStop is {
                Carry: false, Zero: true, Overflow: false
            }
            && missingReceiverAtPush.Pushes.IsEmpty
            && missingReceiverAtPush.OpaqueCalls.IsEmpty
            && missingReceiverAtPush.UnreachedCounterCaptureProvided
            && missingReceiverAtPush.UnreachedOpaqueReturnStatusProvided
            && missingReceiverAtPush.StopAtRawFileOffsetExclusive
                == NetworkDispatcherLockGateProjection.PushLockRawFileOffset
            && missingReceiverAtPush.TestFlags is {
                Carry: false, Parity: true, Auxiliary: null, Zero: true,
                Sign: false, Overflow: false
            });

        NetworkDispatcherLockGatePlan missingCallStatus =
            NetworkDispatcherLockGateProjection.Project(Capture(
                resultEAX: 0,
                receiver: 0x1000,
                counter: 1,
                ebp: 0x2000,
                argument: 0,
                opaqueReturnedNormally: null));
        NetworkDispatcherLockGatePlan failedCall = NetworkDispatcherLockGateProjection.Project(
            Capture(0, 0x1000, 1, 0x2000, 0, false));
        NetworkDispatcherLockGatePlan normalCallMissingEBP =
            NetworkDispatcherLockGateProjection.Project(Capture(
                resultEAX: 0,
                receiver: 0x1000,
                counter: 0x7FFFFFFF,
                ebp: null,
                argument: null,
                opaqueReturnedNormally: true));
        check("Opaque-call status is nullable, its target remains unknown, and flags are unknown after it",
            missingCallStatus.Outcome
                == NetworkDispatcherLockGateOutcome.MissingOpaqueImportReturnStatus
            && missingCallStatus.Pushes.Length == 1
            && missingCallStatus.OpaqueCalls is [{
                ImportCellVirtualAddress: 0x01AC4278, ReturnedNormally: null
            }]
            && missingCallStatus.FlagsAtStop is {
                Carry: false, Parity: true, Auxiliary: null, Zero: true,
                Sign: false, Overflow: false
            }
            && missingCallStatus.StopAtRawFileOffsetExclusive
                == NetworkDispatcherLockGateProjection.OpaqueImportCallRawFileOffset
            && failedCall.Outcome
                == NetworkDispatcherLockGateOutcome.OpaqueImportDidNotReturnNormally
            && failedCall.OpaqueCallReturnInvalidatedFlags is null
            && failedCall.FlagsAtStop is {
                Carry: false, Parity: true, Auxiliary: null, Zero: true,
                Sign: false, Overflow: false
            }
            && failedCall.Reads.IsEmpty
            && failedCall.Writes.IsEmpty
            && failedCall.StopAtRawFileOffsetExclusive
                == NetworkDispatcherLockGateProjection.OpaqueImportCallRawFileOffset
            && normalCallMissingEBP.Outcome
                == NetworkDispatcherLockGateOutcome.MissingEntryEBP
            && normalCallMissingEBP.TestFlags is {
                Carry: false, Parity: true, Auxiliary: null, Zero: true,
                Sign: false, Overflow: false
            }
            && normalCallMissingEBP.IncrementFlags is {
                Carry: null, Parity: true, Auxiliary: true, Zero: false,
                Sign: true, Overflow: true
            }
            && normalCallMissingEBP.FlagsAtStop == normalCallMissingEBP.IncrementFlags);

        NetworkDispatcherLockGatePlan missingCounter = NetworkDispatcherLockGateProjection.Project(
            Capture(1, 0x1000, null, 0x2000, 0x80, opaqueReturnedNormally: true));
        check("A nonzero DWORD TEST skips the opaque import while missing counter capture stops at INC",
            missingCounter.Outcome
                == NetworkDispatcherLockGateOutcome.MissingFreshCounterValue
            && missingCounter.TestBranchTaken == true
            && missingCounter.Pushes.IsEmpty
            && missingCounter.OpaqueCalls.IsEmpty
            && missingCounter.OpaqueCallReturnInvalidatedFlags is null
            && missingCounter.Reads is [{
                RawFileOffset: 0x00BBDDF6,
                Kind: NetworkDispatcherLockGateReadKind.FreshCounter,
                Address: 0x109C, Width: 4, Attempted: true, Captured: false,
                Value: null, Committed: false
            }]
            && missingCounter.FlagsAtStop is {
                Carry: false, Auxiliary: null, Zero: false, Overflow: false
            }
            && missingCounter.UnreachedOpaqueReturnStatusProvided
            && missingCounter.UnreachedEntryEBPCaptureProvided
            && missingCounter.TestFlags is {
                Carry: false, Parity: false, Auxiliary: null, Zero: false,
                Sign: false, Overflow: false
            }
            && missingCounter.IncrementFlags is null);

        NetworkDispatcherLockGatePlan missingReceiverAtIncrement =
            NetworkDispatcherLockGateProjection.Project(Capture(
                resultEAX: 1, receiver: null, counter: 1, ebp: 2, argument: 0,
                opaqueReturnedNormally: true));
        NetworkDispatcherLockGatePlan highResultEAX =
            NetworkDispatcherLockGateProjection.Project(Capture(
                resultEAX: 0x80000000, receiver: null, counter: 0, ebp: null, argument: null));
        NetworkDispatcherLockGatePlan missingCounterAfterCall =
            NetworkDispatcherLockGateProjection.Project(Capture(
                resultEAX: 0, receiver: 0x1000, counter: null, ebp: 2, argument: 0,
                opaqueReturnedNormally: true));
        check("Receiver and counter are required at INC on their reached paths",
            missingReceiverAtIncrement.Outcome
                == NetworkDispatcherLockGateOutcome.MissingReceiverBeforeIncrement
            && missingReceiverAtIncrement.FlagsAtStop is {
                Carry: false, Parity: false, Auxiliary: null, Zero: false,
                Sign: false, Overflow: false
            }
            && missingReceiverAtIncrement.OpaqueCalls.IsEmpty
            && highResultEAX.Outcome
                == NetworkDispatcherLockGateOutcome.MissingReceiverBeforeIncrement
            && highResultEAX.TestBranchTaken == true
            && highResultEAX.FlagsAtStop is {
                Carry: false, Parity: true, Auxiliary: null, Zero: false,
                Sign: true, Overflow: false
            }
            && highResultEAX.TestFlags == highResultEAX.FlagsAtStop
            && highResultEAX.OpaqueCalls.IsEmpty
            && missingCounterAfterCall.Outcome
                == NetworkDispatcherLockGateOutcome.MissingFreshCounterValue
            && missingCounterAfterCall.OpaqueCallReturnInvalidatedFlags == true
            && missingCounterAfterCall.FlagsAtStop is {
                Carry: null, Parity: null, Auxiliary: null, Zero: null,
                Sign: null, Overflow: null
            });

        NetworkDispatcherLockGatePlan signedOverflow =
            NetworkDispatcherLockGateProjection.Project(Capture(
                resultEAX: 1,
                receiver: 0xFFFFFF64,
                counter: 0x7FFFFFFF,
                ebp: null,
                argument: null));
        NetworkDispatcherLockGatePlan wrappedFrame =
            NetworkDispatcherLockGateProjection.Project(Capture(
                resultEAX: 1,
                receiver: 0xFFFFFF64,
                counter: 0,
                ebp: 0x00000002,
                argument: 0x80));
        check("INC preserves prior TEST carry, calculates signed overflow, and wraps derived addresses",
            signedOverflow.Outcome == NetworkDispatcherLockGateOutcome.MissingEntryEBP
            && signedOverflow.DerivedLockPointer == 0xFFFFFFD0
            && signedOverflow.CounterAddress == 0
            && signedOverflow.IncrementedCounterValue == 0x80000000
            && signedOverflow.Writes.Length == 1
            && signedOverflow.Writes[0].Address == 0
            && signedOverflow.TestFlags is {
                Carry: false, Parity: false, Auxiliary: null, Zero: false,
                Sign: false, Overflow: false
            }
            && signedOverflow.IncrementFlags is {
                Carry: false, Parity: true, Auxiliary: true, Zero: false,
                Sign: true, Overflow: true
            }
            && signedOverflow.FlagsAtStop is {
                Carry: false, Parity: true, Auxiliary: true, Zero: false,
                Sign: true, Overflow: true
            }
            && wrappedFrame.Reads[1].Address == 0x0000000A
            && wrappedFrame.Writes[1].Address == 0xFFFFFFFE);

        NetworkDispatcherLockGatePlan missingEBP =
            NetworkDispatcherLockGateProjection.Project(Capture(1, 0x1000, 0, null, 0));
        NetworkDispatcherLockGatePlan missingArgument =
            NetworkDispatcherLockGateProjection.Project(Capture(1, 0x1000, 0, 0x2000, null));
        check("EBP and the independent byte argument are first required at CMP [EBP+8],0",
            missingEBP.Outcome == NetworkDispatcherLockGateOutcome.MissingEntryEBP
            && missingEBP.Writes.Length == 1
            && missingEBP.CompareAddress is null
            && missingEBP.StopAtRawFileOffsetExclusive
                == NetworkDispatcherLockGateProjection.CompareArgumentRawFileOffset
            && missingArgument.Outcome
                == NetworkDispatcherLockGateOutcome.MissingFreshArgumentByte
            && missingArgument.Reads[^1] is {
                Address: 0x2008, Width: 1, Attempted: true, Captured: false,
                Value: null, Committed: false
            }
            && missingArgument.Writes.Length == 1);

        NetworkDispatcherLockGatePlan one = NetworkDispatcherLockGateProjection.Project(
            Capture(1, 0x1000, 0, 0x2000, 1));
        NetworkDispatcherLockGatePlan highByte = NetworkDispatcherLockGateProjection.Project(
            Capture(1, 0x1000, 0, 0x2000, 0x80));
        NetworkDispatcherLockGatePlan highBits = NetworkDispatcherLockGateProjection.Project(
            Capture(1, 0x1000, 0, 0x2000, 0xFF));
        check("Byte CMP/JE handles 1, 80h, and FFh without widening the input",
            one.Outcome == NetworkDispatcherLockGateOutcome.NonzeroArgumentFallthroughSelected
            && one.CompareValue == 1
            && one.CompareFlags is {
                Carry: false, Parity: false, Auxiliary: false, Zero: false,
                Sign: false, Overflow: false
            }
            && one.BranchTaken == false
            && one.SelectedDestinationRawFileOffset == 0x00BBDE0E
            && one.StopAtRawFileOffsetExclusive == 0x00BBDE0E
            && !one.BoundedContinuationEntered
            && highByte.CompareValue == 0x80
            && highByte.CompareFlags is {
                Carry: false, Parity: false, Auxiliary: false, Zero: false,
                Sign: true, Overflow: false
            }
            && highByte.BranchTaken == false
            && highBits.CompareValue == 0xFF
            && highBits.CompareFlags is {
                Carry: false, Parity: true, Auxiliary: false, Zero: false,
                Sign: true, Overflow: false
            }
            && highBits.SelectedDestinationRawFileOffset == 0x00BBDE0E);

        NetworkDispatcherLockGatePlan dirtySkippedCall =
            NetworkDispatcherLockGateProjection.Project(Capture(
                1, 0x1000, 0, 0x2000, 1, opaqueReturnedNormally: true));
        check("Supplied skipped-call status remains visible and prevents exact clean admission",
            one.ExactCleanExitAdmitted
            && dirtySkippedCall.UnreachedOpaqueReturnStatusProvided
            && !dirtySkippedCall.ExactCleanExitAdmitted);
    }

    private static NetworkDispatcherLockGateCapture Capture(
        uint? resultEAX,
        uint? receiver,
        uint? counter,
        uint? ebp,
        byte? argument,
        bool? opaqueReturnedNormally = null) =>
        new(
            resultEAX,
            receiver,
            counter,
            ebp,
            argument,
            opaqueReturnedNormally);

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Where(character => !char.IsWhiteSpace(character)))
            .ToUpperInvariant();
}
